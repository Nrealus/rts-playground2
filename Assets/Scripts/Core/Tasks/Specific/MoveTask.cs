using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nrealus.Extensions;
using NPBehave;
using Core.MapMarkers;
using Core.Handlers;
using Core.Helpers;
using System.Linq;
using Core.Formations;

namespace Core.Tasks
{

    /****** Author : nrealus ****** Last documentation update : 25-07-2020 ******/

    /// <summary>
    /// Currently being a little bit redesigned (as lots of things)
    /// This "active" Task allows units to move on the map, given waypoints. It's one of the most basic and "important" tasks.
    /// </summary>
    public class MoveTask : Task2
    {

        #region Main declarations

        private Formation subjectAsFormation { get { return GetTaskPlan().GetSubject() as Formation; } }

        private TaskParams _taskParams = TaskParams.DefaultParam();

        private MapMarkerWrapper<MoveTaskMarker> _moveTaskMarkerWrapper;

        public int currentWaypointIndex;
        public bool endedPath;
        private List<MoveTaskMarker> childrenMoveTaskMarkers = new List<MoveTaskMarker>();

        #endregion

        public MoveTask()
        {
            CreateAndInitFSM();
            SubscribeOnDestructionLate("clearparams", () => GetParameters().RemoveParameterSubjects(GetParameters().GetParameterSubjects()));
        }
        
        #region Overriden instance methods and functions

        protected override TaskMarker InstanceGetTaskMarker()
        {
            return _moveTaskMarkerWrapper?.Value;
        }

        protected override void InstanceSetTaskMarker(TaskMarker taskMarker)
        {
            _moveTaskMarkerWrapper = new MapMarkerWrapper<MoveTaskMarker>(taskMarker as MoveTaskMarker);
        }

        public List<WaypointMarker> GetWaypointMarkersList()
        {
            return _moveTaskMarkerWrapper.Value.waypointMarkersList;
        }


        protected override TaskParams InstanceGetParameters()
        {   
            return _taskParams;
        }

        public override bool CompatibleForParallelExecution(Task task)
        {
            if (task is MoveTask)
            {
                MoveTask mvtsk = task as MoveTask;
                var chu = childrenMoveTaskMarkers.Select((_) => (_.GetTask().GetSubject() as Formation).GetUnit());
                if (mvtsk.subjectAsFormation.GetUnit() == subjectAsFormation.GetUnit()
                || mvtsk.subjectAsFormation.GetChildFormations().Select((_) => _.GetUnit()).Intersect(chu).Count() > 0)
                    return false;
            }
            bool b = true;
            foreach (var v in childrenMoveTaskMarkers)
            {
                b = b && v.GetTask().CompatibleForParallelExecution(task);
            }
            return true && b;
        }

        protected override void InstanceSetTaskPlan(TaskPlan2 taskPlan)
        {
            base.InstanceSetTaskPlan(taskPlan);

            if (taskPlan != null)
            {
                SubscribeOnDestruction("clearchildrenmovetaskmarkers", () => childrenMoveTaskMarkers.Clear());
    
                UpdateFormationFacing(GetTaskMarker().GetWorldPosition());
                subjectAsFormation.FormTest();

                MoveTask currentTaskAsMoveTask = GetTaskPlan().GetCurrentTaskInPlan() as MoveTask;

                foreach (var chf in subjectAsFormation.GetChildFormations())
                {

                    /*foreach (var us in chf.GetUnit().GetChildNodes())
                    {
                        (new Formation(us)).ChangeParentTo(chf);
                    }*/
                    
                    Vector3 wpos;
                    wpos = chf.GetAcceptableMovementTargetPosition(GetTaskMarker().GetWorldPosition());

                    var prevtm = GetTaskMarker().GetPreviousTaskMarker()?.GetTask() as MoveTask;

                    TaskMarker chprevtm;
                    if (prevtm != null)
                        chprevtm = prevtm.childrenMoveTaskMarkers.Where((_) => (Formation)_.GetTask().GetSubject() == chf).FirstOrDefault();
                    else
                        chprevtm = null;

                    MoveTaskMarker tm = TaskMarker.CreateInstanceAtWorldPosition<MoveTaskMarker>(wpos);
                    tm.AddWaypoint(WaypointMarker.CreateWaypointMarker(wpos));

                    TaskPlan2 tp = tm.InsertAssociatedTaskInPlan(chf, chprevtm);
                    if (!tp.IsPlanBeingExecuted())
                        tp.StartPlanExecution();

                    tm.SubscribeOnDestruction("removefromparentmovetaskmarkerslist",() => childrenMoveTaskMarkers.Remove(tm));                    
                    childrenMoveTaskMarkers.Add(tm);
                }

                SetPhase(TaskPhase.Staging);
            }
            else
            {
                childrenMoveTaskMarkers.Clear();
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
                    foreach (var chmtm in childrenMoveTaskMarkers)
                    {
                        chmtm.GetTask().TryStartExecution();
                    }
                    return true;
                }
                else
                {
                    /*if (InstanceGetParameters().ContainsExecutionMode(TaskParams.TaskExecutionMode.InstantOverrideAll))
                    {
                        // "delete" orders that start after the starting time of this order
                        foreach (var ow in GetSubject(GetRefWrapper()).GetTaskPlan().GetAllActiveTasksFromPlan())
                        {
                            if (ow != GetRefWrapper() && GetSubject(GetRefWrapper()).GetTaskPlan().IsActiveTaskInPlanAfterOther(GetRefWrapper(), ow))
                            {
                                EndExecution(ow);
                                foreach (var chf in unitWrapper.GetFormation().GetChildFormations())
                                {
                                    chf.unitWrapper.GetTaskPlan().StopPlanExecution();
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        
                        if (GetSubject(GetRefWrapper()).GetTaskPlan().GetFirstInlineActiveTaskInPlan() != null)
                        {
                            SetPhase(GetRefWrapper(), Task.OrderPhase.WaitToStartExecution);  
                            foreach (var chf in unitWrapper.GetFormation().GetChildFormations())
                            {
                                chf.unitWrapper.GetTaskPlan().StartPlanExecution();
                            }
                            return true;
                        }
                        
                        return false;
                    }*/
                    return false;
                }
            }

            return false;
        }

        #endregion

        #region Specific behaviour logic

        protected override void EnterExecution()
        {
            base.EnterExecution();
            foreach (var chmtm in childrenMoveTaskMarkers)
            {
                //chmtm.GetTaskAsMoveTask().waypointMarkersList = waypointMarkersList;
                //change screen positions of children task markers accordingly
            }
        }

        private bool _endedPathForAll;
        protected override void UpdateExecution()
        {
            if (subjectAsFormation.IsLeaf())
            {
                _endedPathForAll = true;
                //foreach (var uw in Unit.GetUnitPieceWrappersInUnit(unitWrapper))
                //foreach (var uw in Unit.GetMyselfAndSubUnitsWrappers(Task.GetSubject(GetRefWrapper()).GetTaskSubjectAsReferenceWrapperSpecific<UnitWrapper>()))
                {
                    if (IsInPhase(TaskPhase.Execution))
                    {
                        if (GetSubject() != null && PathExists() && !PathFinished(/*uw*/))
                        {
                            NavigateAlongPath(/*uw*/);
                            _endedPathForAll = false;
                        }
                    }
                }

                if (_endedPathForAll)
                    EndExecution();
            }
            else
            {
                bool b = true;
                foreach (var v in childrenMoveTaskMarkers)
                {
                    b = b && v.GetTask().IsInPhase(Task.TaskPhase.Disposed);
                }
                if (b)
                    EndExecution();
            }
        }

        private bool PathExists()
        {
            return true;
        }
        
        private bool PathFinished()
        {
            return currentWaypointIndex >= GetWaypointMarkersList().Count;
        }

        private float s = 0.05f;
        private void NavigateAlongPath()
        {   
            var wpos = GetWaypointMarkersList()[currentWaypointIndex].transform.position;

            var targetPos = wpos;//GetUnitSubject().GetFormation().GetAcceptableMovementTargetPosition(wpos);

            subjectAsFormation.GetUnit().myMover.MoveToPosition(targetPos, s);

            UpdateFormationFacing(targetPos);

            if (subjectAsFormation.GetUnit().myMover.DistanceConditionToPosition(targetPos, 0.02f))
            {
                currentWaypointIndex++;
            }
        }
        
        private void UpdateFormationFacing(Vector3 targetPos)
        {
            subjectAsFormation./*GetUnit().GetFormation().*/facingAngle =
                Vector3.SignedAngle(
                Vector3.right,
                targetPos - subjectAsFormation.GetUnit().myMover.transform.position, Vector3.down);

        }

        #endregion

    }
}