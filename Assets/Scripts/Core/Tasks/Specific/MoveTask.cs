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

        private UnitTeam subjectAsTeam { get { return GetTaskPlan().GetSubject() as UnitTeam; } }

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

        private MapMarkerWrapper<MoveTaskMarker> _moveTaskMarkerWrapper;
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

        private TaskParams _taskParams = TaskParams.DefaultParam();
        protected override TaskParams InstanceGetParameters()
        {   
            return _taskParams;
        }

        public override bool CompatibleForParallelExecution(Task task)
        {
            if (task is MoveTask)
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
            return true && b;
        }

        protected override void InstanceSetTaskPlan(TaskPlan2 taskPlan)
        {
            base.InstanceSetTaskPlan(taskPlan);

            if (taskPlan != null)
            {
                SubscribeOnDestruction("clearchildrenmovetaskmarkers", () => childrenMoveTaskMarkers.Clear());
    
                UpdateFormationFacing(GetTaskMarker().GetWorldPosition());

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
                    tm.AddWaypointMarker(WaypointMarker.CreateWaypointMarker(wpos));

                    TaskPlan2 chtp = tm.InsertAssociatedTaskIntoPlan(chf, chprevtm);

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
            }

            return false;
        }

        #endregion

        #region Specific behaviour logic

        protected override void EnterExecution()
        {
            GetParameters().plannedStartingTime = TimeHandler.CurrentTime() + new TimeStruct(0,0,15);
        }
        /*protected override void EnterExecution()
        {
            base.EnterExecution();
            foreach (var chmtm in childrenMoveTaskMarkers)
            {
                //chmtm.GetTaskAsMoveTask().waypointMarkersList = waypointMarkersList;
                //change screen positions of children task markers accordingly
            }
        }*/

        private bool _endedPathForAll;
        protected override void UpdateExecution()
        {
            if (subjectAsTeam.IsLeaf())
            {
                _endedPathForAll = true;
                if (IsInPhase(TaskPhase.Execution))
                {
                    if (GetSubject() != null && PathExists() && !PathFinished())
                    {
                        NavigateAlongPath();
                        _endedPathForAll = false;
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

            subjectAsTeam.GetUnit().myMover.MoveToPosition(targetPos, s);

            UpdateFormationFacing(targetPos);

            if (subjectAsTeam.GetUnit().myMover.DistanceConditionToPosition(targetPos, 0.02f))
            {
                currentWaypointIndex++;
            }
        }
        
        private void UpdateFormationFacing(Vector3 targetPos)
        {
            if (!subjectAsTeam.IsVirtualTeam())
            {
                subjectAsTeam.GetUnit().GetFormation().facingAngle =
                    Vector3.SignedAngle(
                    Vector3.right,
                    targetPos - subjectAsTeam.GetUnit().myMover.transform.position, Vector3.down);
            }
        }

        #endregion

    }
}