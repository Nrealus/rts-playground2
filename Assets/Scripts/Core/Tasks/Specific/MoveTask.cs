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
    
        private TaskParams myParameters = TaskParams.DefaultParam();
    
        private List<WaypointMarker> waypointMarkersList = new List<WaypointMarker>();

        public int currentWaypointIndex;
        public bool endedPath;

        private List<MoveTask> childrenMoveTasks = new List<MoveTask>();

        private Unit GetUnitSubject()
        {
            return GetTaskPlan().GetSubject() as Unit;
        }

        protected override void InstanceSetTaskPlan(TaskPlan2 taskPlan)
        {
            base.InstanceSetTaskPlan(taskPlan);

            if (taskPlan != null)
            {
                foreach (var chf in GetUnitSubject().GetFormation().GetChildFormations())
                {
                    //chf.unitWrapper.GetTaskPlan().Clear();

                    var chTask = Task.CreateTask<MoveTask>(/*chf.unitWrapper*/);
                    childrenMoveTasks.Add(chTask);

                    var chTaskPlan = new TaskPlan2(GetUnitSubject());
                    chTaskPlan.AddTaskToPlan(chTask);

                    SubscribeOnDestruction("clearchildrenmovetasks",() => 
                    {
                        childrenMoveTasks.Clear();
                    });
                }

                SetPhase(TaskPhase.Staging);
            }
        }

        protected override TaskParams InstanceGetParameters()
        {
            return myParameters;
        }

        public MoveTask()
        {
            CreateAndInitFSM();
        }

        protected override bool InstanceTryStartExecution()
        {
            if (IsInPhase(TaskPhase.Staging))
            {
                if(/*GetSubject(GetRefWrapper()).*/GetTaskPlan().GetCurrentTaskInPlan() == this)
                {
                    SetPhase(TaskPhase.WaitToStartExecution);
                    foreach (var chmtw in childrenMoveTasks)
                    {
                        chmtw.TryStartExecution();
                        //childrenMoveTasks.StartPlanExecution();
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

        #region Specific behaviour logic

        public void AddWaypoint(WaypointMarker waypoint)
        {
            if (!waypointMarkersList.Contains(waypoint))
            {
                //GetRefWrapper().SubscribeOnClearance(() => RemoveClearedWaypointWrapper(waypointWrapper));
                this.waypointMarkersList.Add(waypoint);
            }
        }

        public void AddWaypoints(IEnumerable<WaypointMarker> waypoints)
        {
            foreach (var wm in waypoints)
            {
                AddWaypoint(wm);
            }
        }

        private void RemoveClearedWaypoint(WaypointMarker wp)
        {
            if (waypointMarkersList.Contains(wp))
            {
                waypointMarkersList.Remove(wp);
            }
        }

        protected override void EnterExecution()
        {
            base.EnterExecution();
            foreach (var chmtw in childrenMoveTasks)
            {
                chmtw.waypointMarkersList = waypointMarkersList;
            }
        }

        private bool _endedPathForAll;
        protected override void UpdateExecution()
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

        private bool PathExists()
        {
            return true;
        }
        
        private bool PathFinished(/*UnitWrapper uw*/)
        {
            return currentWaypointIndex >= waypointMarkersList.Count;
            //return currentExecutionStatePerUnit[uw].currentWaypointIndex >= waypointMarkerWrappersList.Count;
        }

        private float s = 0.05f;
        private void NavigateAlongPath(/*UnitWrapper uw*/)
        {
             
            //var wpos = waypointMarkerWrappersList[currentExecutionStatePerUnit[uw].currentWaypointIndex].GetWrappedReference().transform.position;
            var wpos = waypointMarkersList[currentWaypointIndex].transform.position;

            var targetPos = GetUnitSubject().GetFormation().GetAcceptableMovementTargetPosition(wpos);

            GetUnitSubject().myMover.MoveToPosition(targetPos, s);

            GetUnitSubject().GetFormation().facingAngle =
                Vector3.SignedAngle(Vector3.right, targetPos - GetUnitSubject().myMover.transform.position, Vector3.down);

            if (GetUnitSubject().myMover.DistanceConditionToPosition(targetPos, 0.02f))
            {
                //waypointMarkerWrappersList[currentWaypointIndex].DestroyWrappedReference();
                //currentExecutionStatePerUnit[uw].currentWaypointIndex++;
                currentWaypointIndex++;
            }
        }
        
        /*private void FormUp(Unit u)
        {
            
        }*/

        #endregion

    }
}