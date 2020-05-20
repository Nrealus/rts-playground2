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
    /// This "passive" order (See OrderPlan) can specify targets for units. WIP (as most things but here especially - UnitROE stuff on the horizon..?)
    /// </summary>
    public class EngageAtPositionsOrderNew : Order
    {

        private List<MapMarkerWrapper<FirePositionMarker>> firePositionMarkerWrappersList = new List<MapMarkerWrapper<FirePositionMarker>>();

        private OrderParams myParameters = OrderParams.DefaultParam();

        private UnitGroupWrapper unitFormationWrapper;
        private class UnitWrapperExecutionState
        {
            public int currentWaypointIndex;
            public bool endedPath;

        }
        private Dictionary<UnitWrapper, UnitWrapperExecutionState> currentExecutionStatePerUnit = new Dictionary<UnitWrapper, UnitWrapperExecutionState>();


        protected override IOrderable InstanceGetOrderReceiver()
        {
            return unitFormationWrapper;
        }
        
        protected override void InstanceSetOrderReceiver(IOrderable orderable, OrderWrapper predecessor, OrderWrapper successor)
        {
           
            unitFormationWrapper = orderable as UnitGroupWrapper;

            unitFormationWrapper.GetOrdersPlan().QueuePassiveOrderToPlan(GetMyWrapper(), predecessor, successor);

            SetPhase(GetMyWrapper(), OrderPhase.Staging);
            
            /*if (Order.IsInPhase(GetMyWrapper(), OrderPhase.Initial))
            {
                unitWrapper = orderable as UnitWrapper;
                //orderMarkerWrapper = (new OrderMarker(_myWrapper)).GetMyWrapper<OrderMarker>();
                //GetMyWrapper().SubscribeOnClearance(() => RemoveOrderMarkerAtClearance());
                Order.SetPhase(GetMyWrapper(), OrderPhase.Registration);
            }
            else
            {
                Debug.LogError("should not happen");
            }*/
        }

        protected override OrderParams InstanceGetOrderParams()
        {
            //if(myParameters == null)
            //    myParameters = OrderParams.DefaultParams;
            return myParameters;
        }
        
        /*protected override void InstanceSetOrderParams(OrderParams orderParams)
        {
            myParameters = orderParams;
        }*/

        protected override bool InstanceTryStartExecution()
        {
            // replace some passive orders if certain case / situation (specific or not) ?

            if (IsInPhase(GetMyWrapper(), OrderPhase.Staging))
            {
                SetPhase(GetMyWrapper(), OrderPhase.ExecutionWaitingTimeToStart);
                return true;
            }

            return false;

        }

        /*protected override bool InstanceIsReadyToStartExecution(OrderExecutionMode mode)
        {
            if (Order.GetReceiver(GetMyWrapper()).IsCurrentOrder(GetMyWrapper())
            && IsInPhase(GetMyWrapper(), OrderPhase.Staging))
            {
                return true;
            }
            else 
            {
                return false;
            }

            //return true;
        }*/


        public EngageAtPositionsOrderNew()
        {
            _myWrapper = new OrderWrapper<EngageAtPositionsOrderNew>(this, () => {_myWrapper = null;});

            GetParameters(GetMyWrapper()).isPassive = true;

            CreateAndInitFSM();            
        }

        /*public override void ClearWrapper()
        {
            GetMyWrapper().DestroyWrappedReference();
            _myWrapper = null;
            //orderMarkerWrapper.WrappedObject.ClearWrapper();
            //int c = waypointMarkerWrappersList.Count;
            //for (int i = c-1; i>= 0; i--)
            //{
            //    waypointMarkerWrappersList[i].WrappedObject.ClearWrapper();
            //}
            
        }*/

        protected override void OrderPhasesFSMInit()
        {
            
            orderPhasesFSM.AddState(OrderPhase.Initial);

            orderPhasesFSM.AddState(OrderPhase.Staging);

            orderPhasesFSM.AddState(OrderPhase.ExecutionWaitingTimeToStart,
                () =>
                {
                    if(!Order.GetParameters(GetMyWrapper()).plannedStartingTime.isInitialized)
                    {
                        Order.SetPhase(GetMyWrapper(), Order.OrderPhase.Execution);
                    }
                    else if (TimeHandler.HasTimeJustPassed(Order.GetParameters(GetMyWrapper()).plannedStartingTime))
                    //    || TimeHandler.HasTimeAlreadyPassed(Order.GetParameters(GetMyWrapper()).startingTime))
                    {
                        Order.SetPhase(GetMyWrapper(), Order.OrderPhase.Execution);                
                    }
                },
                () =>
                {
                    if(TimeHandler.HasTimeJustPassed(Order.GetParameters(GetMyWrapper()).plannedStartingTime))
                    {
                        Order.SetPhase(GetMyWrapper(), Order.OrderPhase.Execution);
                    }
                });

            orderPhasesFSM.AddState(OrderPhase.Execution,
                () =>
                {
                    GetParameters(GetMyWrapper()).plannedStartingTime = TimeHandler.CurrentTime();                
                },
                () =>
                {
                    UpdateExecution();
                });

            orderPhasesFSM.AddState(OrderPhase.Pause);

            orderPhasesFSM.AddState(OrderPhase.Cancelled);

            orderPhasesFSM.AddState(OrderPhase.End,
               null,
               () =>
               {
                    SetPhase(GetMyWrapper(), OrderPhase.End2);
               });

            bool waitForReactionAtEnd = false;
            OrderWrapper nextActiveOrder = null;

            orderPhasesFSM.AddState(OrderPhase.End2,
            () =>
            {
                if(Order.GetParameters(GetMyWrapper()).ContainsExecutionMode(OrderParams.OrderExecutionMode.WaitForReactionAtEnd))
                {
                    waitForReactionAtEnd = true;
                }
                
                if (GetReceiver(GetMyWrapper()).GetOrdersPlan().GetNextInlinePassiveOrderInPlan(GetMyWrapper()) != null
                    && GetParameters(GetReceiver(GetMyWrapper()).GetOrdersPlan().GetNextInlinePassiveOrderInPlan(GetMyWrapper())).ContainsExecutionMode(OrderParams.OrderExecutionMode.Chain))
                {
                    nextActiveOrder = InstanceGetOrderReceiver().GetOrdersPlan().GetNextInlinePassiveOrderInPlan(GetMyWrapper());
                }

            },
            () =>
            {
                if (waitForReactionAtEnd == false)
                    SetPhase(GetMyWrapper(), OrderPhase.Disposed);
            });

            orderPhasesFSM.AddState(OrderPhase.Disposed,
            () =>
            {
                GetMyWrapper().DestroyWrappedReference();

                if (nextActiveOrder != null && nextActiveOrder.WrappedObject != null)
                    Order.TryStartExecution(nextActiveOrder);

            });
               
            orderPhasesFSM.CurrentState = OrderPhase.Initial;
        }

        #region Specific behaviour logic

        public void AddFirePosition(MapMarkerWrapper<FirePositionMarker> firePositionWrapper)
        {
            if (firePositionWrapper != null && !firePositionMarkerWrappersList.Contains(firePositionWrapper))
            {
                firePositionWrapper.SubscribeOnClearance(() => RemoveClearedFirePositionWrapper(firePositionWrapper));
                GetMyWrapper().SubscribeOnClearance(() => firePositionWrapper.DestroyWrappedReference());
                firePositionMarkerWrappersList.Add(firePositionWrapper);
            }
        }

        private void RemoveClearedFirePositionWrapper(MapMarkerWrapper<FirePositionMarker> firePositionWrapper)
        {
            if (firePositionMarkerWrappersList.Contains(firePositionWrapper))
            {
                firePositionMarkerWrappersList.Remove(firePositionWrapper);
            }
        }

        /*private void RemoveOrderMarkerAtClearance()
        {
            orderMarkerWrapper.DestroyWrappedReference();
            //GetMyWrapper().UnsubscribeOnClearance(() => RemoveOrderMarkerAtClearance());
        }*/

        private void UpdateExecution()
        {
            foreach (var uw in UnitGroup.GetUnitWrappersInGroup(unitFormationWrapper))
            {
                if (Order.IsInPhase(GetMyWrapper(), OrderPhase.Execution))
                {
                    EngageTargetsInPositionsROE(uw);
                }
            }
        }


        private float s = 5;
        private void EngageTargetsInPositionsROE(UnitWrapper unitWrapper)
        {
            /**/
            s = Mathf.Max(s - Time.deltaTime, 0);

            if (s == 0)
            {
                Debug.Log("Hello, engaging order still active");
                s = 5;
            }
            /*unitWrapper.WrappedObject.myMover.MoveToPosition(firePositionMarkerWrappersList[currentWaypointIndex].GetWrappedAs<WaypointMarker>().myPosition, s);
            if (unitWrapper.WrappedObject.myMover.DistanceConditionToPosition(waypointMarkerWrappersList[currentWaypointIndex].GetWrappedAs<WaypointMarker>().myPosition, 0.02f))
            {
                waypointMarkerWrappersList[currentWaypointIndex].DestroyWrappedReference();
                //currentWaypointIndex++;
            }*/
        }

        #endregion

    }
}