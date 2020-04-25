using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;
using NPBehave;
using Core.MapMarkers;
using Core.Handlers;
using Core.BuiltStructures;

namespace Core.Orders
{
    public class BuildOrder : Order
    {

        private MapMarkerWrapper<BuildingMarker> _buildingMarkerWrapper;
        private MapMarkerWrapper<BuildingMarker> buildingMarkerWrapper
        {
            get
            {
                if(builtStructureWrapper != null && builtStructureWrapper.WrappedObject != null)
                    return builtStructureWrapper.buildingMarkerWrapper;
                else
                    return _buildingMarkerWrapper;
            }

            set
            {
                _buildingMarkerWrapper = value;
            }
        }

        private BuiltStructureWrapper builtStructureWrapper;
        
        private Root behaviourTree;
        private Clock btClock;

        private UnitWrapper unitWrapper;
        private OrderParams myParameters = OrderParams.DefaultParam();

        private OrderWrapper<MoveOrder> predecessorMoveOrderWrapper = null;

        protected override IOrderable InstanceGetOrderReceiver()
        {
            return unitWrapper;
        }
        
        protected override void InstanceSetOrderReceiver(IOrderable orderable, OrderWrapper predecessor, OrderWrapper successor)
        {
           
            unitWrapper = orderable as UnitWrapper;

            unitWrapper.QueueActiveOrderToPlan(GetMyWrapper(), predecessor, successor);

            SetPhase(GetMyWrapper(), OrderPhase.Staging);
                
            /*if (Order.IsInPhase(GetMyWrapper(), OrderPhase.InitialState))
            {
                unitWrapper = orderable as UnitWrapper;

                //orderMarkerWrapper = (new OrderMarker(_myWrapper)).GetMyWrapper<OrderMarker>();
                //GetMyWrapper().SubscribeOnClearance(() => RemoveOrderMarkerAtClearance());
                Order.SetPhase(GetMyWrapper(), OrderPhase.Registration);
            }
            else
            {
                Debug.LogError("There should be no reason for this to happen");
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

        private bool CreateAndExecuteIntermediateMoveOrder()
        {
            if (!CloseEnoughToBuildingMarker() && predecessorMoveOrderWrapper == null)
            {
                predecessorMoveOrderWrapper = OrderFactory.CreatePredecessorOrderWrapperAndSetReceiver<MoveOrder>(unitWrapper, GetMyWrapper());
                
                predecessorMoveOrderWrapper.SubscribeOnClearance(() => predecessorMoveOrderWrapper = null);
                
                predecessorMoveOrderWrapper.GetWrappedAs<MoveOrder>().AddWaypoint(
                    (new WaypointMarker(
                        buildingMarkerWrapper.GetWrappedAs<BuildingMarker>().myPosition
                        + 2*(UnityEngine.Random.Range(0.5f,1)*Vector3.right+UnityEngine.Random.Range(0.5f,1)*Vector3.forward).normalized)
                    ).GetMyWrapper<WaypointMarker>());
                
                GetParameters(GetMyWrapper()).AddExecutionMode(OrderParams.OrderExecutionMode.Chain);

                TryStartExecution(predecessorMoveOrderWrapper);
                
                return false;
            }
            else
            {
                SetPhase(GetMyWrapper(), Order.OrderPhase.ExecutionWaitingTimeToStart);
                return true;
            }
        }

        protected override bool InstanceTryStartExecution()
        {
            if (IsInPhase(GetMyWrapper(), OrderPhase.Staging))
            {

                if(Order.GetReceiver(GetMyWrapper()).IsFirstInlineActiveOrderInPlan(GetMyWrapper()))
                {
                    return CreateAndExecuteIntermediateMoveOrder();
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
                            if(!GetParameters(GetMyWrapper()).isPassive)
                                    Order.EndExecution(Order.GetReceiver(GetMyWrapper()).GetFirstInlineActiveOrderInPlan());
                                return CreateAndExecuteIntermediateMoveOrder();
                        }
                        
                        return false;
                    }
                }
            }

            return false;
        }

        /*protected override bool InstanceIsReadyToStartExecution()
        {
            if (Order.GetReceiver(GetMyWrapper()).IsCurrentOrder(GetMyWrapper())
            && IsInPhase(GetMyWrapper(), OrderPhase.Staging))
            {
                if (!CloseEnoughToBuildingMarker() && predecessorMoveOrderWrapper == null)
                {
                    predecessorMoveOrderWrapper = OrderFactory.CreatePredecessorOrderWrapperAndSetReceiver<MoveOrder>(unitWrapper, GetMyWrapper());
                    
                    predecessorMoveOrderWrapper.SubscribeOnClearance(() => predecessorMoveOrderWrapper = null);
                    
                    predecessorMoveOrderWrapper.GetWrappedAs<MoveOrder>().AddWaypoint(
                        (new WaypointMarker(
                            buildingMarkerWrapper.GetWrappedAs<BuildingMarker>().myPosition
                            + 2*(UnityEngine.Random.Range(-1,1)*Vector3.right+UnityEngine.Random.Range(-1,1)*Vector3.forward).normalized)
                        ).GetMyWrapper<WaypointMarker>());
                    
                    TryStartExecution(predecessorMoveOrderWrapper);
                    
                    return false;
                }
                return true;
            }
            else 
            {
                return false;
            }*/

            /*//if(GetMyWrapper().AmIFirstInQueue())
            //Debug.Log(GetOrderReceiver().GetCurrentOrderInQueue());
            if(Order.GetReceiver(GetMyWrapper()).GetCurrentOrderInQueue() == GetMyWrapper())
            {
                if (predecessorMoveOrderWrapper == null)
                {
                    predecessorMoveOrderWrapper = OrderFactory.CreateOrderWrapperAndSetReceiver<MoveOrder>(unitWrapper);
                    predecessorMoveOrderWrapper.SubscribeOnClearance(() => predecessorMoveOrderWrapper = null);
                    //Order.RegisterAtOrderHandlerAndReceiver(predecessorMoveOrderWrapper, GetMyWrapper(), null);
                    predecessorMoveOrderWrapper.GetWrappedAs<MoveOrder>().AddWaypoint(
                        (new WaypointMarker(buildingMarkerWrapper.GetWrappedAs<BuildingMarker>().myPosition + 2*(Vector3.right+Vector3.forward).normalized)
                            ).GetMyWrapper<WaypointMarker>());
                    Order.TryStartExecution(predecessorMoveOrderWrapper);
                    Debug.Log(Order.IsInPhase(predecessorMoveOrderWrapper, OrderPhase.NotReadyForExecution));
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }*/

            //return true;
        /*}*/

        public BuildOrder()
        {
            btClock = new Clock();
            _myWrapper = new OrderWrapper<BuildOrder>(this, () => {_myWrapper = null;});

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

        public void SetBuildingToBuild(MapMarkerWrapper<BuildingMarker> buildingMarkerWrapper)
        {
            buildingMarkerWrapper.SubscribeOnClearance(() => buildingMarkerWrapper = null);
            GetMyWrapper().SubscribeOnClearance(
                () => { 
                    if (buildingMarkerWrapper != null && ((buildingMarkerWrapper.GetWrappedAs<BuildingMarker>().builtStructureWrapper != null
                    && BuiltStructure.GetHP(buildingMarkerWrapper.GetWrappedAs<BuildingMarker>().builtStructureWrapper) == 0)
                        || buildingMarkerWrapper.GetWrappedAs<BuildingMarker>().builtStructureWrapper == null))
                        buildingMarkerWrapper.DestroyWrappedReference(); 
                });
            this.buildingMarkerWrapper = buildingMarkerWrapper;
        }

        private void UpdateBTClock()
        {
            btClock.Update(Time.deltaTime);
        }

        private Root CreateBehaviourTree()
        {
            return new Root(new Blackboard(btClock), btClock,
                        new Condition(() => { return Order.IsInPhase(GetMyWrapper(), OrderPhase.Execution); },
                            new Sequence(
                                new Condition(() => { return Order.ReceiverExists(GetMyWrapper()) && CloseEnoughToBuildingMarker() && CanActuallyBuild(); }, Stops.LOWER_PRIORITY,
                                    new Action(ActuallyBuild)),
                                new Condition(() => { return FinishedBuilding(); },
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

        private bool CloseEnoughToBuildingMarker()
        {
            if (buildingMarkerWrapper != null
                && Vector3.Distance(buildingMarkerWrapper.GetWrappedAs<BuildingMarker>().myPosition, unitWrapper.WrappedObject.myMover.GetPosition()) <= 2.5f)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CanActuallyBuild()
        {
            if (buildingMarkerWrapper != null
                && _materials > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private float _materials = 150;
        private float _previousmaterials = 150;
        private bool FinishedBuilding()
        {
            if (_materials < 0 || BuiltStructure.GetHP(builtStructureWrapper) >= 100)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private float buildingRate = 0.15f;
        private void ActuallyBuild()
        {
            if (builtStructureWrapper == null && buildingMarkerWrapper != null)
            {
                builtStructureWrapper = buildingMarkerWrapper.GetWrappedAs<BuildingMarker>().CreateAndSetBuiltStructure<BuiltStructure>();
                GetMyWrapper().SubscribeOnClearance(() => builtStructureWrapper = null);
            }

            _previousmaterials = _materials;
            _materials = Mathf.Max(-1, _materials - buildingRate);

            if (_materials > 0)
            {
                if (Mathf.Abs(_materials % 30) < 1)
                    Debug.Log("Building structure...");
                BuiltStructure.AddHP(builtStructureWrapper, buildingRate);
            }

            /*unitWrapper.WrappedObject.myMover.MoveToPosition(waypointMarkerWrappersList[currentWaypointIndex].GetWrappedAs<WaypointMarker>().myPosition, s);
            if (unitWrapper.WrappedObject.myMover.DistanceConditionToPosition(waypointMarkerWrappersList[currentWaypointIndex].GetWrappedAs<WaypointMarker>().myPosition, 0.02f))
            {
                waypointMarkerWrappersList[currentWaypointIndex].DestroyWrappedReference();
                //currentWaypointIndex++;
            }*/
        }

    }
}