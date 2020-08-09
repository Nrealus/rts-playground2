using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nrealus.Extensions;
using UnityEngine;
using Core.Helpers;
using System;
using Core.Faction;
using Core.Selection;
using Core.Handlers;
using Core.Formations;
using Nrealus.Extensions.Observer;
using Core.Tasks;
using Nrealus.Extensions.ReferenceWrapper;
using Nrealus.Extensions.Tree;
using Gamelogic.Extensions;
using System.Text;

namespace Core.Units
{
    public class UnitGroup : IActorGroup
    {
        
        public static UnitGroup PrepareAndCreateGroupFromSelected(List<ISelectable> selectedEntities)
        {
            UnitGroup res;
            if (selectedEntities.Count == 1)
            {
                res = (selectedEntities[0] as Unit).GenerateGroupFromStructure();
                //res = (selectedEntities[0] as Unit).GenerateGroupFromStructure2();
            }
            else
            {   
                var units = selectedEntities.ConvertAll( _ => _ as Unit);
                //--- Keep as is !!!
                Unit.FlattenUnitsToParentIfAllSiblingsContained(units);
                Unit lca = Unit.GetLowestCommonAncestorUnit(units, false);
                //---
                units.Remove(lca);


                if (units.Count == 0)
                {
                    Debug.Log("ky");
                    res = lca.GenerateGroupFromStructure();
                }
                else
                {
                    Debug.Log("kyyy");
                    res = lca.CreateGroupBySubGroupsUnits(units);
                }
            }
            return res;
        }

        #region IDestroyable implementation
        
        private bool destroying = false;
        private EasyObserver<string> onDestroyed = new EasyObserver<string>();

        public void SubscribeOnDestruction(string key, Action action)
        {
            onDestroyed.SubscribeEventHandlerMethod(key, action);
        }

        public void SubscribeOnDestructionLate(string key, Action action)
        {
            onDestroyed.SubscribeEventHandlerMethodAtEnd(key, action);
        }

        public void SubscribeOnDestruction(string key, Action action, bool combineActionsIfKeyAlreadyExists)
        {
            onDestroyed.SubscribeEventHandlerMethod(key, action, combineActionsIfKeyAlreadyExists);
        }

        public void SubscribeOnDestructionLate(string key, Action action, bool combineActionsIfKeyAlreadyExists)
        {
            onDestroyed.SubscribeEventHandlerMethodAtEnd(key, action, combineActionsIfKeyAlreadyExists);
        }

        public void UnsubscribeOnDestruction(string key)
        {
            onDestroyed.UnsubscribeEventHandlerMethod(key);
        }

        public void DestroyThis()
        {
            destroying = true;

            onDestroyed.Invoke();
            
            foreach (var p in new List<TaskPlan2>(GetThisGroupPlans()))
            {
                p.EndPlanExecution();
            }
            foreach(var u in new List<Unit>(GetActorsAsUnits()))
            {
                RemoveUnitFromGroup(u);
            }
            
            ChangeParentTo(null);

            foreach(var ug in new List<UnitGroup>(GetSubGroupsAsUG()))
            {
                //ug.ChangeParentTo(null);
                //subgroup.DestroyThis(); // destroy them too ? (not for all "types" of groups)
            }

            Debug.Log("group destroyed");
        }

        #endregion

        #region IActorGroup implementation + related

        /*private EasyObserver<Unit, Unit> onUnitAddedToGroup = new EasyObserver<Unit, Unit>();
        public void SubscribeOnUnitAdditionToGroup(Unit u, Action<Unit> act)
        {
            onUnitAddedToGroup.SubscribeEventHandlerMethod(u, act);
        }

        public void UnsubscribeOnUnitAdditionToGroup(Unit u)
        {
            onUnitAddedToGroup.UnsubscribeEventHandlerMethod(u);
        }*/

        public void AddUnitToGroup(Unit unit)
        {
            GetActorsAsUnits().Add(unit);
            unit.GetParticipatedGroupsAsUG().Add(this);
        }

        /*private EasyObserver<Unit, Unit> onUnitRemovedFromGroup = new EasyObserver<Unit, Unit>();
        public void SubscribeOnUnitRemovalFromGroup(Unit u, Action<Unit> act)
        {
            onUnitRemovedFromGroup.SubscribeEventHandlerMethod(u, act);
        }

        public void UnsubscribeOnUnitRemovalFromGroup(Unit u)
        {
            onUnitRemovedFromGroup.UnsubscribeEventHandlerMethod(u);
        }*/

        public void RemoveUnitFromGroup(Unit unit)
        {
            //onUnitRemovedFromGroup.Invoke(unit);
            GetActorsAsUnits().Remove(unit);
            unit.GetParticipatedGroupsAsUG().Remove(this);

            if (!destroying && GetActorsAsUnits().Count == 0)
            {
                Debug.Log("no more units in group");
                DestroyThis();
            }
        }

        #region Units

        private List<Unit> _unitsList = new List<Unit>();
        public List<Unit> GetActorsAsUnits() { return _unitsList; }

        public List<IActor> GetActors() { return _unitsList.ConvertAll(_ => _ as IActor); }

        //public IActor GetFirstActor() { return _unitsList[0]; }

        #endregion

        #region Task Plans

        private static int _instcount2;
        private Dictionary<TaskPlan2,string> removePlanKeyDict = new Dictionary<TaskPlan2, string>();
        public TaskPlan2 CreateAndRegisterNewOwnedPlan()
        {
            _instcount2++;

            TaskPlan2 res = null;

            res = new TaskPlan2(this);
            GetThisGroupPlans().Add(res);

            //removePlanKeyDict.Add(res, new StringBuilder("removeplan").Append(_instcount2).ToString());
            //SubscribeOnDestruction(removePlanKeyDict[res], () => EndAndUnregisterOwnedPlan(res));

            return res;
        }

        public void UnregisterOwnedPlan(TaskPlan2 taskPlan)
        {
            GetThisGroupPlans().Remove(taskPlan);

            if (!destroying && GetSubGroupsPlans(true, true).Count == 0)
                DestroyThis();
        }

        private List<TaskPlan2> _plans = new List<TaskPlan2>();
        public List<TaskPlan2> GetThisGroupPlans()
        {
            return _plans;
        }

        public List<TaskPlan2> GetActorsGroupsPlans() // obviously includes this group's plans
        {
            List<TaskPlan2> res = new List<TaskPlan2>();
            foreach (var v in GetActors())
            {
                res.AddRange(v.GetParticipatedGroupsPlans());
            }
            return res.Distinct().ToList();
        }

        public List<TaskPlan2> GetSubGroupsPlans(bool includeThisGroup, bool recursive)
        {
            List<TaskPlan2> res = new List<TaskPlan2>();

            Queue<UnitGroup> queue = new Queue<UnitGroup>();
            if (includeThisGroup)
                queue.Enqueue(this);
            foreach (var sg in GetSubGroupsAsUG())
                queue.Enqueue(sg);

            while (queue.Count > 0)
            {
                res.AddRange(queue.Peek().GetThisGroupPlans());

                if (recursive)
                {
                    foreach (var sg in queue.Dequeue().GetSubGroupsAsUG())
                        queue.Enqueue(sg);
                }
            }
            return res.Distinct().ToList();
        }

        public List<TaskPlan2> GetActorsGroupsAndSubGroupsPlans(bool includeThisGroup, bool recursive)
        {
            List<TaskPlan2> res = new List<TaskPlan2>();

            Queue<UnitGroup> queue = new Queue<UnitGroup>();
            if (includeThisGroup)
                queue.Enqueue(this);
            foreach (var sg in GetSubGroupsAsUG())
                queue.Enqueue(sg);

            while (queue.Count > 0)
            {
                res.AddRange(queue.Peek().GetActorsGroupsPlans());

                if (recursive)
                {
                    foreach (var sg in queue.Dequeue().GetSubGroupsAsUG())
                        queue.Enqueue(sg);
                }
            }
            return res.Distinct().ToList();
        }

        #endregion

        #region Parent Group

        private Dictionary<string,Action> _actDict = new Dictionary<string,Action>();        
        private ObservedValue<UnitGroup> _parent = new ObservedValue<UnitGroup>(null);

        public void SubscribeOnParentGroupChange(string key, Action action)
        {
            if (!_actDict.ContainsKey(key))
            {
                _actDict.Add(key, action);
                _parent.OnValueChange += action;
            }
        }

        public void UnsubscribeOnParentGroupChange(string key)
        {
            _parent.OnValueChange -= _actDict[key];
        }

        public UnitGroup GetParentGroupAsUG()
        {
            return _parent.Value;
        }

        public IActorGroup GetParentGroup()
        {
            return _parent.Value;
        }
        
        public UnitGroup GetRootGroupAsUG()
        {
            UnitGroup res = GetParentGroupAsUG();
            if (res != null)
                res = res.GetRootGroupAsUG();
            else
                res = this;
            return res;
        }

        public void ChangeParentTo(IActorGroup newParent)
        {
            if (newParent != null)
            {
                (newParent as UnitGroup).Internal_AddSubGroup(this);
            }
            else
            {
                if (GetParentGroupAsUG() != null)
                    GetParentGroupAsUG().Internal_RemoveSubGroup(this);
            }
        }

        public void Internal_SetParentGroup(UnitGroup newParent)
        {
            _parent.Value = newParent as UnitGroup;
        }

        #endregion

        #region Sub Groups

        /*private EasyObserver<string, UnitGroup> onSubGroupAdded = new EasyObserver<string, UnitGroup>();
        public void SubscribeOnSubGroupAddition(string key, Action<UnitGroup> act)
        {
            onSubGroupAdded.SubscribeEventHandlerMethod(key, act);
        }

        public void UnsubscribeOnSubGroupAddition(string key)
        {
            onSubGroupAdded.UnsubscribeEventHandlerMethod(key);
        }

        private EasyObserver<string, UnitGroup> onSubGroupRemoved = new EasyObserver<string, UnitGroup>();
        public void SubscribeOnSubGroupRemoval(string key, Action<UnitGroup> act)
        {
            onSubGroupRemoved.SubscribeEventHandlerMethod(key, act);
        }

        public void UnsubscribeOnSubGroupRemoval(string key)
        {
            onSubGroupRemoved.UnsubscribeEventHandlerMethod(key);
        }*/

        private List<UnitGroup> _subgroups = new List<UnitGroup>();
        public List<UnitGroup> GetSubGroupsAsUG()
        {
            return _subgroups;
        }

        public List<IActorGroup> GetSubGroups()
        {
            return _subgroups.ConvertAll(_ => _ as IActorGroup);
        }

        public void Internal_AddSubGroup(UnitGroup child)
        {
            if (child.GetParentGroupAsUG() != null)
                child.GetParentGroupAsUG().Internal_RemoveSubGroup(child);
            child.Internal_SetParentGroup(this);
            GetSubGroupsAsUG().Add(child);
            //onSubGroupAdded.Invoke(child);
        }

        public void Internal_RemoveSubGroup(UnitGroup child)
        {
            child.Internal_SetParentGroup(null);
            GetSubGroupsAsUG().Remove(child);
            //onSubGroupRemoved.Invoke(child);
        }

        #endregion

        #region Tree structure additional functions

        public bool IsLeaf()
        {
            return GetSubGroupsAsUG().Count == 0;
        }
        
        public bool IsRoot()
        {
            return GetParentGroup() == null;
        }

        #endregion

        #endregion
        /*public List<UnitGroup> GetAllSubGroupsBFS()
        {
            List<UnitGroup> result = new List<UnitGroup>();

            Queue<UnitGroup> bfsqueue = new Queue<UnitGroup>();
            var ch = GetSubGroups();
            foreach (var v in ch)
                bfsqueue.Enqueue(v);

            while (bfsqueue.Count > 0)
            {
                result.Add(bfsqueue.Peek());

                ch = bfsqueue.Dequeue().GetSubGroups();
                foreach (var v in ch)
                    bfsqueue.Enqueue(v);
            }

            return result;
        }*/

        //public bool IsVirtualGroup() { return false;}//return GetUnit() == null; }        


        #region Initialisation

        private static int _instcount;
        private string reattachFormationToNewParentKey;
        private void InitUnits(IEnumerable<Unit> units)
        {
            GetActorsAsUnits().AddRange(units); 
        }

        public UnitGroup(/*Unit unit*/)
        {
            //Debug.Log("group created");
            //InitUnits(new List<Unit>() {unit});
        }

        #endregion

    }
}