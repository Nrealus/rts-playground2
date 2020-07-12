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

    /****** Author : nrealus ****** Last documentation update : 12-07-2020 ******/

    /// <summary>
    /// CURRENTLY BEING SLIGHTLY REDESIGNED.
    /// This "active" order (See OrderPlan) allows units to move on the map, given waypoints. It's the most basic and "important" order.
    /// </summary>
    public class MoveTask : Task2, IHasRefWrapper<TaskWrapper<MoveTask>>
    {
    
        public new TaskWrapper<MoveTask> GetRefWrapper()
        {
            return _myWrapper as TaskWrapper<MoveTask>;
        }

        private TaskParams myParameters = TaskParams.DefaultParam();
    
        private List<MapMarkerWrapper<WaypointMarker>> waypointMarkerWrappersList = new List<MapMarkerWrapper<WaypointMarker>>();

        private UnitWrapper unitWrapper;
        private class UnitWrapperExecutionState
        {
            public int currentWaypointIndex;
            public bool endedPath;

            public void Destroy()
            {
                // nothing yet lol, just to keep it same as in BuildOrder
            }
        }
        private Dictionary<UnitWrapper, UnitWrapperExecutionState> currentExecutionStatePerUnit = new Dictionary<UnitWrapper, UnitWrapperExecutionState>();

        protected override ITaskSubject InstanceGetSubject()
        {
            return unitWrapper;
        }
        
        protected override void InstanceSetSubject(ITaskSubject subject, TaskWrapper predecessor, TaskWrapper successor)
        {
            unitWrapper = subject as UnitWrapper;
            //unitWrapper.GetTaskPlan().QueueActiveOrderToPlan(GetMyWrapper(), predecessor, successor);

            // TEMPORARY
            //foreach (var uw in Unit.GetUnitPieceWrappersInUnit(unitWrapper))
            foreach (var uw in Unit.GetMyselfAndSubUnitsWrappers(unitWrapper))
            {
                currentExecutionStatePerUnit.Add(uw, new UnitWrapperExecutionState());
                /*uw.WrappedObject.SubscribeOnUnitPieceRemovalFromUnit(uw, () =>
                {
                    currentExecutionStatePerUnit[uw].Destroy();
                    currentExecutionStatePerUnit.Remove(uw);
                });*/

                uw.SubscribeOnClearance(() =>
                {
                    currentExecutionStatePerUnit[uw].Destroy();
                    currentExecutionStatePerUnit.Remove(uw);
                });
                GetRefWrapper().SubscribeOnClearance(() => 
                {
                    currentExecutionStatePerUnit[uw].Destroy();
                    currentExecutionStatePerUnit.Remove(uw);
                });
            }

            SetPhase(GetRefWrapper(), OrderPhase.Staging);
        }

        protected override TaskParams InstanceGetParameters()
        {
            return myParameters;
        }

        public MoveTask()
        {
            _myWrapper = new TaskWrapper<MoveTask>(this, () => {_myWrapper = null;});

            CreateAndInitFSM();
        }

        #region Specific behaviour logic

        public void AddWaypoint(MapMarkerWrapper<WaypointMarker> waypointWrapper)
        {
            if (!waypointMarkerWrappersList.Contains(waypointWrapper))
            {
                //waypointWrapper.SubscribeOnClearance(() => RemoveClearedWaypointWrapper(waypointWrapper));
                GetRefWrapper().SubscribeOnClearance(() => RemoveClearedWaypointWrapper(waypointWrapper));
                //GetMyWrapper().SubscribeOnClearance(() => waypointWrapper.DestroyWrappedReference());
                this.waypointMarkerWrappersList.Add(waypointWrapper);
            }
        }

        public void AddWaypoints(IEnumerable<MapMarkerWrapper<WaypointMarker>> waypointWrappers)
        {
            foreach (var wm in waypointWrappers)
            {
                AddWaypoint(wm);
            }
        }

        private void RemoveClearedWaypointWrapper(MapMarkerWrapper<WaypointMarker> waypointWrapper)
        {
            if (waypointMarkerWrappersList.Contains(waypointWrapper))
            {
                waypointMarkerWrappersList.Remove(waypointWrapper);
            }
        }

        private bool _endedPathForAll;
        protected override void UpdateExecution()
        {
            _endedPathForAll = true;
            //foreach (var uw in Unit.GetUnitPieceWrappersInUnit(unitWrapper))
            foreach (var uw in Unit.GetMyselfAndSubUnitsWrappers(unitWrapper))
            {
                if (Task.IsInPhase(GetRefWrapper(), OrderPhase.Execution))
                {
                    if (Task.SubjectExists(GetRefWrapper()) && PathExists() && !PathFinished(uw))
                    {
                        NavigateAlongPath(uw);
                        _endedPathForAll = false;
                    }
                }
            }

            if (_endedPathForAll)
                EndExecution(GetRefWrapper());

        }

        private bool PathExists()
        {
            return true;
        }
        
        private bool PathFinished(UnitWrapper uw)
        {
            return currentExecutionStatePerUnit[uw].currentWaypointIndex >= waypointMarkerWrappersList.Count;
        }

        private float s = 0.05f;
        private void NavigateAlongPath(UnitWrapper uw)
        {
            uw.GetWrappedReference().myMover.MoveToPosition(waypointMarkerWrappersList[currentExecutionStatePerUnit[uw].currentWaypointIndex].GetWrappedReference().transform.position, s);
            if (uw.GetWrappedReference().myMover.DistanceConditionToPosition(waypointMarkerWrappersList[currentExecutionStatePerUnit[uw].currentWaypointIndex].GetWrappedReference().transform.position, 0.02f))
            {
                //waypointMarkerWrappersList[currentWaypointIndex].DestroyWrappedReference();
                currentExecutionStatePerUnit[uw].currentWaypointIndex++;
            }
        }

        #endregion

    }
}