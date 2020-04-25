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
    public class MoveOrder : Order
    {

        //private MapMarkerWrapper<OrderMarker> orderMarkerWrapper;
        private List<MapMarkerWrapper<WaypointMarker>> waypointMarkerWrappersList = new List<MapMarkerWrapper<WaypointMarker>>();
        private int currentWaypointIndex;

        private Root behaviourTree;
        private Clock btClock;

        private UnitWrapper unitWrapper;
        private OrderParams myParameters = OrderParams.DefaultParam();

        protected override IOrderable InstanceGetOrderReceiver()
        {
            return unitWrapper;
        }
        
        protected override void InstanceSetOrderReceiver(IOrderable orderable, OrderWrapper predecessor, OrderWrapper successor)
        {
           
            unitWrapper = orderable as UnitWrapper;

            unitWrapper.QueueActiveOrderToPlan(GetMyWrapper(), predecessor, successor);

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
            //if(IsReadyToStartExecution(orderWrapper, mode))
            if (IsInPhase(GetMyWrapper(), OrderPhase.Staging))
            {
                if(GetReceiver(GetMyWrapper()).IsFirstInlineActiveOrderInPlan(GetMyWrapper()))
                {
                    SetPhase(GetMyWrapper(), OrderPhase.ExecutionWaitingTimeToStart);
                    return true;
                }
                else
                {
                    if (InstanceGetOrderParams().ContainsExecutionMode(OrderParams.OrderExecutionMode.InstantOverrideAll))
                    {
                        // "delete" orders that start after the starting time of this order

                        foreach (var ow in GetReceiver(GetMyWrapper()).GetAllActiveOrdersFromPlan())
                        {
                            if (ow != GetMyWrapper())
                            {
                                EndExecution(ow);
                            }
                            /*if(!Order.GetParameters(ow).startingTime.isInitialized
                            || Order.GetParameters(ow).startingTime < TimeHandler.CurrentTime())
                            {
                                Order.EndExecution(ow);
                            }*/
                        }
                        
                        if (GetReceiver(GetMyWrapper()).GetFirstInlineActiveOrderInPlan() != null)
                        {
                            SetPhase(GetMyWrapper(), Order.OrderPhase.ExecutionWaitingTimeToStart);  
                            return true;
                        }
                        
                        return false;
                    }

                    return false;
                }
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


        public MoveOrder()
        {
            btClock = new Clock();
            _myWrapper = new OrderWrapper<MoveOrder>(this, () => {_myWrapper = null;});

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
            
            /*orderPhasesFSM.AddState(OrderPhase.Registration,
                () =>
                {
                    Order.RegisterAtOrderHandlerAndReceiver(GetMyWrapper(), null, null);
                });

            orderPhasesFSM.AddState(OrderPhase.NotReadyForExecution,
                () =>
                {

                });

            orderPhasesFSM.AddState(OrderPhase.RequestConfirmation,
                () =>
                {
                    if (InstanceIsOrderApplicable()) // individually ?
                    {
                        orderPhasesFSM.CurrentState = OrderPhase.ReadyForExecution;
                    }
                    else
                    {
                        orderPhasesFSM.CurrentState = OrderPhase.NotReadyForExecution;
                    }
                });

            orderPhasesFSM.AddState(OrderPhase.ReadyForExecution,
                () =>
                {
                    //GetMyWrapper().RegisterMeIfAppropriate();
                });*/

            orderPhasesFSM.AddState(OrderPhase.Initial);

            orderPhasesFSM.AddState(OrderPhase.Staging);

            /*orderPhasesFSM.AddState(OrderPhase.NotReadyForExecution,
                () =>
                {

                });

            orderPhasesFSM.AddState(OrderPhase.ReadyForExecution,
                () =>
                {
                    
                });*/

            orderPhasesFSM.AddState(OrderPhase.ExecutionWaitingTimeToStart,
                () =>
                {
                    if(!Order.GetParameters(GetMyWrapper()).startingTime.isInitialized)
                    {
                        Order.SetPhase(GetMyWrapper(), Order.OrderPhase.Execution);
                    }
                    else if (TimeHandler.HasTimeJustPassed(Order.GetParameters(GetMyWrapper()).startingTime))
                    //    || TimeHandler.HasTimeAlreadyPassed(Order.GetParameters(GetMyWrapper()).startingTime))
                    {
                        Order.SetPhase(GetMyWrapper(), Order.OrderPhase.Execution);                
                    }
                },
                () =>
                {
                    if(TimeHandler.HasTimeJustPassed(Order.GetParameters(GetMyWrapper()).startingTime))
                    {
                        Order.SetPhase(GetMyWrapper(), Order.OrderPhase.Execution);
                    }
                });

            orderPhasesFSM.AddState(OrderPhase.Execution,
                () =>
                {
                    if (behaviourTree == null)
                    {
                        behaviourTree = CreateBehaviourTree();
                        behaviourTree.Start();
                    }
                },
                () =>
                {
                    //if (Input.GetKeyDown(KeyCode.O))
                    //    EndExecution(GetMyWrapper());

                    UpdateBTClock();
                    // in other states too ? (thinking about shared blackboard and associated clock...)
                });

            orderPhasesFSM.AddState(OrderPhase.Pause);

            orderPhasesFSM.AddState(OrderPhase.Cancelled,
                () =>
                {
                    
                },
                () =>
                {
                    DisposeBehaviourTree();
                });

            orderPhasesFSM.AddState(OrderPhase.End,
               () =>
               {

               },
               () =>
               {
                    DisposeBehaviourTree();
                    SetPhase(GetMyWrapper(), OrderPhase.End2);
               });

            bool waitForReactionAtEnd = false;
            OrderWrapper mem = null;

            orderPhasesFSM.AddState(OrderPhase.End2,
            () =>
            {
                if(Order.GetParameters(GetMyWrapper()).ContainsExecutionMode(OrderParams.OrderExecutionMode.WaitForReactionAtEnd))
                {
                    waitForReactionAtEnd = true;
                }
                
                if (GetReceiver(GetMyWrapper()).GetNextInlineActiveOrderInPlan(GetMyWrapper()) != null
                    && GetParameters(GetReceiver(GetMyWrapper()).GetNextInlineActiveOrderInPlan(GetMyWrapper())).ContainsExecutionMode(OrderParams.OrderExecutionMode.Chain))
                {
                    mem = InstanceGetOrderReceiver().GetNextInlineActiveOrderInPlan(GetMyWrapper());
                }
                
                /*if (InstanceGetOrderReceiver().GetNextInlineOrder(GetMyWrapper()) != null
                && GetParameters(InstanceGetOrderReceiver().GetNextInlineOrder(GetMyWrapper())).executionMode == OrderParams.OrderExecutionMode.Chain
                && )
                // && and support for different policies for siblings
                    //&& Order.GetConfirmationFromReceiver(InstanceGetOrderReceiver().GetCurrentOrderInQueue()))
                {
                    mem = InstanceGetOrderReceiver().GetCurrentOrdersInQueue();
                }*/

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

                if (mem != null && mem.WrappedObject != null)
                    Order.TryStartExecution(mem);

            });
               
            orderPhasesFSM.CurrentState = OrderPhase.Initial;
        }

        public void AddWaypoint(MapMarkerWrapper<WaypointMarker> waypointWrapper)
        {
            if (!waypointMarkerWrappersList.Contains(waypointWrapper))
            {
                waypointWrapper.SubscribeOnClearance(() => RemoveClearedWaypointWrapper(waypointWrapper));
                GetMyWrapper().SubscribeOnClearance(() => waypointWrapper.DestroyWrappedReference());
                this.waypointMarkerWrappersList.Add(waypointWrapper);
            }
        }

        /*public void AddWaypoints(List<MapMarkerWrapper<WaypointMarker>> waypointWrapper)
        {
            this.waypointMarkerWrappersList.AddRange(waypointWrapper);
        }*/

        private void RemoveClearedWaypointWrapper(MapMarkerWrapper<WaypointMarker> waypointWrapper)
        {
            if (waypointMarkerWrappersList.Contains(waypointWrapper))
            {
                //waypointWrapper.UnsubscribeOnClearance(() => RemoveClearedWaypointWrapper(waypointWrapper));
                //GetMyWrapper().UnsubscribeOnClearanceAll(() => { 
                //    if (waypointWrapper.WrappedObject != null) 
                //        waypointWrapper.WrappedObject.ClearWrapper();
                //});
                //waypointWrapper.UnsubscribeOnClearanceAll();
                waypointMarkerWrappersList.Remove(waypointWrapper);
            }
        }

        /*private void RemoveOrderMarkerAtClearance()
        {
            orderMarkerWrapper.DestroyWrappedReference();
            //GetMyWrapper().UnsubscribeOnClearance(() => RemoveOrderMarkerAtClearance());
        }*/

        private void UpdateBTClock()
        {
            btClock.Update(Time.deltaTime);
        }

        private Root CreateBehaviourTree()
        {
            return new Root(new Blackboard(btClock), btClock,
                        new Condition(() => { return Order.IsInPhase(GetMyWrapper(), OrderPhase.Execution); },
                            new Sequence(
                                new Condition(() => { return Order.ReceiverExists(GetMyWrapper()) && PathExists() && !PathFinished(); }, Stops.LOWER_PRIORITY,
                                    new Action(NavigateAlongPath)),
                                new Condition(() => { return PathFinished(); },
                                    new Action(() => Order.EndExecution(GetMyWrapper()))))));
        }

        private void DisposeBehaviourTree()
        {
            if (behaviourTree != null && behaviourTree.CurrentState == Node.State.ACTIVE)
            {
                behaviourTree.Stop();
                behaviourTree.Blackboard.Disable();
                behaviourTree = null;
            }
        }

        private bool PathExists()
        {
            return true;
        }
        
        private bool PathFinished()
        {
            return currentWaypointIndex >= waypointMarkerWrappersList.Count;
        }

        private float s = 0.05f;
        private void NavigateAlongPath()
        {
            unitWrapper.WrappedObject.myMover.MoveToPosition(waypointMarkerWrappersList[currentWaypointIndex].GetWrappedAs<WaypointMarker>().myPosition, s);
            if (unitWrapper.WrappedObject.myMover.DistanceConditionToPosition(waypointMarkerWrappersList[currentWaypointIndex].GetWrappedAs<WaypointMarker>().myPosition, 0.02f))
            {
                waypointMarkerWrappersList[currentWaypointIndex].DestroyWrappedReference();
                //currentWaypointIndex++;
            }
        }

    }
}