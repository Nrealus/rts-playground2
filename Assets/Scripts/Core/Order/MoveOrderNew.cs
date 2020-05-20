using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;
using NPBehave;
using Core.MapMarkers;
using Core.Handlers;

namespace Core.Orders
{

    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    /// <summary>
    /// This "active" order (See OrderPlan) allows units to move on the map, given waypoints. It's the most basic and "important" order.
    /// </summary>
    public class MoveOrderNew : Order2
    {
    
        private OrderParams myParameters = OrderParams.DefaultParam();
    
        private List<MapMarkerWrapper<WaypointMarker>> waypointMarkerWrappersList = new List<MapMarkerWrapper<WaypointMarker>>();

        private UnitGroupWrapper unitsGroupWrapper;
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

        protected override IOrderable InstanceGetOrderReceiver()
        {
            return unitsGroupWrapper;
        }
        
        protected override void InstanceSetOrderReceiver(IOrderable orderable, OrderWrapper predecessor, OrderWrapper successor)
        {
            unitsGroupWrapper = orderable as UnitGroupWrapper;
            unitsGroupWrapper.GetOrdersPlan().QueueActiveOrderToPlan(GetMyWrapper(), predecessor, successor);

            foreach (var uw in UnitGroup.GetUnitWrappersInGroup(unitsGroupWrapper))
            {
                currentExecutionStatePerUnit.Add(uw, new UnitWrapperExecutionState());
                uw.unitsGroupWrapper.WrappedObject.SubscribeOnRemovalFromGroup(uw, () =>
                {
                    currentExecutionStatePerUnit[uw].Destroy();
                    currentExecutionStatePerUnit.Remove(uw);
                });

                uw.SubscribeOnClearance(() =>
                {
                    currentExecutionStatePerUnit[uw].Destroy();
                    currentExecutionStatePerUnit.Remove(uw);
                });
                GetMyWrapper().SubscribeOnClearance(() => 
                {
                    currentExecutionStatePerUnit[uw].Destroy();
                    currentExecutionStatePerUnit.Remove(uw);
                });
            }

            SetPhase(GetMyWrapper(), OrderPhase.Staging);
        }

        protected override OrderParams InstanceGetOrderParams()
        {
            return myParameters;
        }

        public MoveOrderNew()
        {
            _myWrapper = new OrderWrapper<MoveOrderNew>(this, () => {_myWrapper = null;});

            CreateAndInitFSM();
        }

        #region Specific behaviour logic

        public void AddWaypoint(MapMarkerWrapper<WaypointMarker> waypointWrapper)
        {
            if (!waypointMarkerWrappersList.Contains(waypointWrapper))
            {
                //waypointWrapper.SubscribeOnClearance(() => RemoveClearedWaypointWrapper(waypointWrapper));
                GetMyWrapper().SubscribeOnClearance(() => waypointWrapper.DestroyWrappedReference());
                this.waypointMarkerWrappersList.Add(waypointWrapper);
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
            foreach (var uw in UnitGroup.GetUnitWrappersInGroup(unitsGroupWrapper))
            {
                if (Order.IsInPhase(GetMyWrapper(), OrderPhase.Execution))
                {
                    if (Order.ReceiverExists(GetMyWrapper()) && PathExists() && !PathFinished(uw))
                    {
                        NavigateAlongPath(uw);
                        _endedPathForAll = false;
                    }
                }
            }

            if (_endedPathForAll)
                EndExecution(GetMyWrapper());

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
            uw.WrappedObject.myMover.MoveToPosition(waypointMarkerWrappersList[currentExecutionStatePerUnit[uw].currentWaypointIndex].GetWrappedAs<WaypointMarker>().transform.position, s);
            if (uw.WrappedObject.myMover.DistanceConditionToPosition(waypointMarkerWrappersList[currentExecutionStatePerUnit[uw].currentWaypointIndex].GetWrappedAs<WaypointMarker>().transform.position, 0.02f))
            {
                //waypointMarkerWrappersList[currentWaypointIndex].DestroyWrappedReference();
                currentExecutionStatePerUnit[uw].currentWaypointIndex++;
            }
        }

        #endregion

    }
}