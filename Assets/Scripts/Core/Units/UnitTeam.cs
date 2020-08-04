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
    public class UnitTeam : ITaskSubject
    {
        
        public static UnitTeam PrepareAndCreateTeamFromSelected(List<ISelectable> selectedEntities)
        {
            UnitTeam res;
            if (selectedEntities.Count == 1)
            {
                res = (selectedEntities[0] as Unit).GenerateTeamFromStructure();
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
                    res = lca.GenerateTeamFromStructure();
                }
                else
                {
                    Debug.Log("kyyy");
                    res = lca.CreateTeamBySubTeamsUnits(units);
                }
            }
            return res;
        }

        /*#region Equality specification

        public override bool Equals(object obj)
        {
            var otherTeam = obj as UnitTeam;

            return object.ReferenceEquals(this, obj)
                || (GetUnit() == otherTeam.GetUnit() && GetSubTeams().EqualContentUnordered(otherTeam.GetSubTeams()));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // Choose large primes to avoid hashing collisions
                const int HashingBase = (int)2166136261;
                const int HashingMultiplier = 16777619;

                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ (!object.ReferenceEquals(null, GetUnit()) ? GetUnit().GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ (!object.ReferenceEquals(null, GetParentTeam()) ? GetParentTeam().GetHashCode() : 0);
                return hash;
            }
        }

        public static bool operator ==(UnitTeam team1, UnitTeam team2)
        {
            if (object.ReferenceEquals(team1, team2))
            {
                return true;
            }

            if(object.ReferenceEquals(null, team2))
            {
                return false;
            }

            return (team1.Equals(team2));
        }

        public static bool operator !=(UnitTeam team1, UnitTeam team2)
        {
            return !(team1 == team2);
        }

        #endregion*/

        #region IDestroyable implementation
        
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
            onDestroyed.Invoke();
        }

        #endregion

        #region ITaskSubject implementation

        private static int _instcount2;
        private Dictionary<TaskPlan2,string> removePlanKeyDict = new Dictionary<TaskPlan2, string>();
        public TaskPlan2 AddNewPlan()
        {
            _instcount2++;

            TaskPlan2 res = null;

            res = new TaskPlan2(this);
            GetPlans().Add(res);
            if (IsVirtualTeam())
                _localPlans.Add(res);

            removePlanKeyDict.Add(res, new StringBuilder("removeplan").Append(_instcount2).ToString());
            SubscribeOnDestruction(removePlanKeyDict[res], () => RemoveAndEndPlan(res));

            return res;
        }

        public void RemoveAndEndPlan(TaskPlan2 taskPlan)
        {
            if (GetPlans().Remove(taskPlan))
            {
                //if (!IsNominalFormation())
                _localPlans.Remove(taskPlan);            
                
                taskPlan.EndPlanExecution();
                UnsubscribeOnDestruction(removePlanKeyDict[taskPlan]);

                if (_localPlans.Count == 0)
                    DestroyThis();
            }
        }

        private List<TaskPlan2> _localPlans = new List<TaskPlan2>();
        public List<TaskPlan2> GetPlans()
        {
            if (IsVirtualTeam())
                return _localPlans;
            else
                return GetUnit().GetPlans();
        }

        private ObservedValue<UnitTeam> _parent = new ObservedValue<UnitTeam>(null);
        public UnitTeam GetParentTeam()
        {
            return _parent.Value;
        }

        public void Internal_SetParentTeam(UnitTeam newParent)
        {
            _parent.Value = newParent;
        }

        public bool IsRoot()
        {
            return GetParentTeam() == null;
        }

        public bool IsLeaf()
        {
            return GetSubTeams().Count == 0;
        }

        private List<UnitTeam> _subteams = new List<UnitTeam>();
        public List<UnitTeam> GetSubTeams()
        {
            return _subteams;
        }

        private void AddSubTeam(UnitTeam child)
        {
            if (child.GetParentTeam() != null)
                child.GetParentTeam().RemoveSubTeam(child);
            child.Internal_SetParentTeam(this);
            GetSubTeams().Add(child);
        }

        private void RemoveSubTeam(UnitTeam child)
        {
            child.Internal_SetParentTeam(null);
            GetSubTeams().Remove(child);
        }

        public void ChangeParentTo(UnitTeam newParent)
        {
            if (newParent != null)
            {
                newParent.AddSubTeam(this);
            }
            else
            {
                if (GetParentTeam() != null)
                    GetParentTeam().RemoveSubTeam(this);
            }
        }

        #endregion

        private UnitWrapper _unitWrapper;
        public Unit GetUnit() { return _unitWrapper.Value; }

        public bool IsVirtualTeam() { return GetUnit() == null; }        


        #region Initialisation

        private static int _instcount;
        private string reattachFormationToNewParentKey;
        private void InitUnit(Unit unit)
        {
            _unitWrapper = new UnitWrapper(unit);   
        }
        
        public UnitTeam(Unit unit)
        {
            InitUnit(unit);
        }

        #endregion

    }
}