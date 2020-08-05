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
    public class UnitTeam : ITaskAgent
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
            
            foreach(var v in new List<Task>(GetTasksWhereIsInternalSubject()))
            {
                UnregisterTaskWhereAgentIsSubject(v);
            }
        }

        #endregion

        #region ITaskAgent implementation

        private static int _instcount2;
        private Dictionary<TaskPlan2,string> removePlanKeyDict = new Dictionary<TaskPlan2, string>();
        public TaskPlan2 CreateAndRegisterNewOwnedPlan()
        {
            _instcount2++;

            TaskPlan2 res = null;

            res = new TaskPlan2(this);
            GetOwnedPlans().Add(res);

            if (IsVirtualTeam())
                _localPlans.Add(res);

            removePlanKeyDict.Add(res, new StringBuilder("removeplan").Append(_instcount2).ToString());
            SubscribeOnDestruction(removePlanKeyDict[res], () => EndAndUnregisterOwnedPlan(res));

            return res;
        }

        public void EndAndUnregisterOwnedPlan(TaskPlan2 taskPlan)
        {
            if (GetOwnedPlans().Remove(taskPlan))
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
        public List<TaskPlan2> GetOwnedPlans()
        {
            if (IsVirtualTeam())
                return _localPlans;
            else
                return GetUnit().GetPlansOwnedByEquivalentTeams();
        }

        private static int _instcount3;
        private Dictionary<Task,string> _removeTaskWhereSubjectKeyDict = new Dictionary<Task, string>();
        public void RegisterTaskWhereAgentIsSubject(Task task) // storing collection should be shared too
        {
            _instcount3++;

            //GetUnit().Internal_GetTaskWhereEquivalentTeamsAreSubjectsDict().Add(task,
            //_removeTaskWhereSubjectKeyDict.Add(task, new StringBuilder("removetaskwheresubject").Append(_instcount3).ToString());

            GetTasksWhereIsInternalSubject().Add(task);
        }

        public void UnregisterTaskWhereAgentIsSubject(Task task)
        {
            if (GetTasksWhereIsInternalSubject().Remove(task))
            {
                _tasksWhereInternalSubject.Remove(task);
            }
        }

        private List<Task> _tasksWhereInternalSubject = new List<Task>();
        public List<Task> GetTasksWhereIsInternalSubject() // storing collection should be shared too
        {
            if (IsVirtualTeam())
                return _tasksWhereInternalSubject;
            else
                return GetUnit().Internal_GetTasksWhereEquivalentTeamsAreSubjects();
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

        public List<UnitTeam> GetAllSubTeamsBFS()
        {
            List<UnitTeam> result = new List<UnitTeam>();

            Queue<UnitTeam> bfsqueue = new Queue<UnitTeam>();
            var ch = GetSubTeams();
            foreach (var v in ch)
                bfsqueue.Enqueue(v);

            while (bfsqueue.Count > 0)
            {
                result.Add(bfsqueue.Peek());

                ch = bfsqueue.Dequeue().GetSubTeams();
                foreach (var v in ch)
                    bfsqueue.Enqueue(v);
            }

            return result;
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