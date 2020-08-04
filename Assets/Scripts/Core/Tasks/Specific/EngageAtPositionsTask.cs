using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nrealus.Extensions;
using NPBehave;
using Core.MapMarkers;
using Core.Handlers;
using Core.Helpers;

namespace Core.Tasks
{
    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    /// <summary>
    /// OUTDATED
    /// This "passive" order (See OrderPlan) can specify targets for units. WIP (as most things but here especially - UnitROE stuff on the horizon..?)
    /// </summary>
    public class EngageAtPositionsTask : Task2
    {

        #region Main declarations

        private UnitTeam subjectAsTeam { get { return GetTaskPlan().GetSubject() as UnitTeam; } }
        
        private List<UnitTeam> subjectsSubTeams { get { return subjectAsTeam.GetSubTeams(); }}
        //private List<TargetTaskMarker> childrenTargetTaskMarkers = new List<TargetTaskMarker>();

        #endregion

        public EngageAtPositionsTask()
        {
            CreateAndInitFSM();
            SubscribeOnDestructionLate("clearparams", () => GetParameters().RemoveParameterSubjects(GetParameters().GetParameterSubjects()));
        }
        
        #region Overriden instance methods and functions

        private MapMarkerWrapper<TargetTaskMarker> _targetTaskMarkerWrapper;
        protected override TaskMarker InstanceGetTaskMarker()
        {
            return _targetTaskMarkerWrapper?.Value;
        }

        protected override void InstanceSetTaskMarker(TaskMarker taskMarker)
        {
            _targetTaskMarkerWrapper = new MapMarkerWrapper<TargetTaskMarker>(taskMarker as TargetTaskMarker);
        }

        public List<FirePositionMarker> GetFirePositionMarkersList()
        {
            return _targetTaskMarkerWrapper.Value.firePositionMarkers;
        }

        private TaskParams _taskParams = TaskParams.DefaultParam();
        protected override TaskParams InstanceGetParameters()
        {   
            return _taskParams;
        }

        public override bool CompatibleForParallelExecution(Task task)
        {
            bool b = true;
            foreach (var v in childrenTargetTaskMarkers)
            {
                b = b && v.GetTask().CompatibleForParallelExecution(task);
            }
            return true && b;
            /*if (task is MoveTask)
            {
                MoveTask mvtsk = task as MoveTask;
                var chus = childrenMoveTaskMarkers.Select((_) => (_.GetTask().GetSubject() as UnitTeam).GetUnit());
                if (mvtsk.subjectAsTeam.GetUnit() == subjectAsTeam.GetUnit()
                || mvtsk.subjectAsTeam.GetSubTeams().Select((_) => _.GetUnit()).Intersect(chus).Count() > 0)
                    return false;
            }
            bool b = true;
            foreach (var v in childrenMoveTaskMarkers)
            {
                b = b && v.GetTask().CompatibleForParallelExecution(task);
            }
            return true && b;*/
        }

        protected override void InstanceSetTaskPlan(TaskPlan2 taskPlan)
        {
            base.InstanceSetTaskPlan(taskPlan);

            throw new System.NotImplementedException();
            if (taskPlan != null)
            {
                /*SubscribeOnDestruction("clearchildrenmovetaskmarkers", () => childrenMoveTaskMarkers.Clear());

                if (!subjectAsTeam.IsVirtualTeam())
                    subjectAsTeam.GetUnit().GetFormation().FormTest();

                foreach (var chf in subjectAsTeam.GetSubTeams())
                {
                    Vector3 wpos;
                    wpos = chf.GetUnit().GetFormation().GetAcceptableMovementTargetPosition(GetTaskMarker().GetWorldPosition());

                    var prevtm = GetTaskMarker().GetPreviousTaskMarker()?.GetTask() as MoveTask;
                    
                    TaskMarker chprevtm;
                    if (prevtm != null)
                        chprevtm = prevtm.childrenMoveTaskMarkers.Where((_) => (UnitTeam)_.GetTask().GetSubject() == chf).FirstOrDefault();
                    else
                        chprevtm = null;

                    MoveTaskMarker tm = TaskMarker.CreateInstanceAtWorldPosition<MoveTaskMarker>(wpos);
                    tm.AddWaypoint(WaypointMarker.CreateWaypointMarker(wpos));

                    TaskPlan2 chtp = tm.InsertAssociatedTaskIntoPlan(chf, chprevtm);

                    tm.SubscribeOnDestruction("removefromparentmovetaskmarkerslist",() => childrenMoveTaskMarkers.Remove(tm));                    
                    childrenMoveTaskMarkers.Add(tm);
                }*/

                SetPhase(TaskPhase.Staging);
            }
            else
            {
                //childrenMoveTaskMarkers.Clear();
            }
        }

        protected override bool InstanceTryStartExecution()
        {
            if (IsInPhase(TaskPhase.Staging) && GetSubject() != null)
            {
                foreach (var plan in new List<TaskPlan2>(GetSubject().GetPlans()))
                {
                    if (plan.GetCurrentTaskInPlan() != this 
                        && !CompatibleForParallelExecution(plan.GetCurrentTaskInPlan()))
                    {
                        plan.EndPlanExecution();
                    }
                }

                if(GetTaskPlan().GetCurrentTaskInPlan() == this)
                {
                    SetPhase(TaskPhase.WaitToStartExecution);

                    foreach (var chmtm in childrenTargetTaskMarkers)
                    {
                        chmtm.GetTask().TryStartExecution();
                    }
                    return true;
                }
            }

            return false;
        }

        #endregion
        

        #region Specific behaviour logic

        protected override void UpdateExecution()
        {
            /*foreach (var uw in Unit.GetMyselfAndSubUnitsWrappers(unitWrapper))
            {
                if (Task.IsInPhase(GetRefWrapper(), OrderPhase.Execution))
                {
                    EngageTargetsInPositionsROE(uw);
                }
            }*/
        }

        private float s = 5;
        private void EngageTargetsInPositionsROE(UnitWrapper unitWrapper)
        {
            s = Mathf.Max(s - Time.deltaTime, 0);

            if (s == 0)
            {
                Debug.Log("Hello, engaging order still active");
                s = 5;
            }
        }

        #endregion

    }
}