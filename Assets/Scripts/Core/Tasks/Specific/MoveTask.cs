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
    

        private MapMarkerWrapper<MoveTaskMarker> _moveTaskMarkerWrapper;
        private MoveTaskMarker GetMoveTaskMarker() { return _moveTaskMarkerWrapper?.Value; }
        
        public void SetMoveTaskMarker(MoveTaskMarker moveTaskMarker)
        {
            _moveTaskMarkerWrapper = new MapMarkerWrapper<MoveTaskMarker>(moveTaskMarker);
        }

        //private List<WaypointMarker> waypointMarkersList = new List<WaypointMarker>();
        public List<WaypointMarker> GetWaypointMarkersList()
        {
            return GetMoveTaskMarker().waypointMarkersList;
        }
        public int currentWaypointIndex;
        public bool endedPath;

        private List<MoveTaskMarker> childrenMoveTaskMarkers = new List<MoveTaskMarker>();

        private Unit GetUnitSubject()
        {
            return GetTaskPlan().GetSubject() as Unit;
        }

        protected override void InstanceSetTaskPlan(TaskPlan2 taskPlan)
        {
            base.InstanceSetTaskPlan(taskPlan);

            if (taskPlan != null)
            {
                GetUnitSubject().GetFormation().facingAngle =
                    Vector3.SignedAngle(
                        Vector3.right,
                        GetMoveTaskMarker().GetWorldPosition() - GetUnitSubject().myMover.transform.position, Vector3.down);

                GetUnitSubject().GetFormation().FormTest();

                foreach (var chf in GetUnitSubject().GetFormation().GetChildFormations())
                {
                    //chf.unitWrapper.GetTaskPlan().Clear();

                    Vector3 wpos = chf.GetAcceptableMovementTargetPosition(GetMoveTaskMarker().GetWorldPosition());
                                        
                    MoveTaskMarker prevtm = null;
                    //(GetMoveTaskMarker().GetPreviousTaskMarker().GetTask() as MoveTask) : via taskmarker and its predecessor : other possible approach
                    MoveTask curmvtsk = GetTaskPlan().GetCurrentTaskInPlan() as MoveTask;
                    if (curmvtsk != null && curmvtsk != this)
                    {
                        foreach (var chmtmagain in curmvtsk.childrenMoveTaskMarkers)
                        {
                            if ((object)chmtmagain.GetTask().GetSubject() == chf.unit)
                            {
                                prevtm = chmtmagain;
                                break;
                            }
                        }
                    }
                    MoveTaskMarker tm = TaskMarker.CreateInstanceAtWorldPosition<MoveTaskMarker>(wpos, prevtm);
                    tm.GetTaskAsMoveTask().SetMoveTaskMarker(tm);
                    if (prevtm != null)
                        prevtm.GetTask().GetTaskPlan().AddTaskToPlan(tm.GetTask());
                    else
                        (new TaskPlan2(chf.unit)).AddTaskToPlan(tm.GetTask());

                    tm.AddWaypoint(WaypointMarker.CreateInstance(wpos));
                    
                    tm.ConfirmPositioning(true);
                    
                    tm.GetTask().GetParameters().AddExecutionMode(TaskParams.TaskExecutionMode.Chain);

                    tm.SubscribeOnDestruction("removefromparentmovetaskmarkerslist",() => childrenMoveTaskMarkers.Remove(tm));
                    
                    //SelectionHandler.GetUsedSelector().SelectEntity(tm);
                    childrenMoveTaskMarkers.Add(tm);
                }

                SubscribeOnDestruction("clearchildrenmovetaskmarkers", () => childrenMoveTaskMarkers.Clear());

                SetPhase(TaskPhase.Staging);
            }
            else
            {
                //childrenMoveTaskMarkers.Clear() etc ?
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

        #region Specific behaviour logic

        /*public void AddWaypoint(WaypointMarker waypoint)
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
        }*/

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
            return currentWaypointIndex >= GetWaypointMarkersList().Count;//waypointMarkersList.Count;
            //return currentExecutionStatePerUnit[uw].currentWaypointIndex >= waypointMarkerWrappersList.Count;
        }

        private float s = 0.05f;
        private void NavigateAlongPath(/*UnitWrapper uw*/)
        {
             
            //var wpos = waypointMarkerWrappersList[currentExecutionStatePerUnit[uw].currentWaypointIndex].GetWrappedReference().transform.position;
            var wpos = GetWaypointMarkersList()[currentWaypointIndex].transform.position;
            //var wpos = waypointMarkersList[currentWaypointIndex].transform.position;

            var targetPos = wpos;//GetUnitSubject().GetFormation().GetAcceptableMovementTargetPosition(wpos);

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