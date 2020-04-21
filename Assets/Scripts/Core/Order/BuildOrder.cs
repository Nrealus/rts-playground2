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
        private OrderParams myParameters;

        protected override IOrderable InstanceGetOrderReceiver()
        {
            return unitWrapper;
        }
        
        protected override void InstanceSetOrderReceiver(IOrderable orderable)
        {
            if (Order.IsInPhase(GetMyWrapper(), OrderPhase.InitialState))
            {
                unitWrapper = orderable as UnitWrapper;
                //orderMarkerWrapper = (new OrderMarker(_myWrapper)).GetMyWrapper<OrderMarker>();
                //GetMyWrapper().SubscribeOnClearance(() => RemoveOrderMarkerAtClearance());
                Order.SetPhase(GetMyWrapper(), OrderPhase.Registration);
            }
            else
            {
                Debug.LogError("should not happen");
            }
        }

        protected override OrderParams InstanceGetOrderParams()
        {
            if(myParameters == null)
                myParameters = OrderParams.DefaultParams;
            return myParameters;
        }
        
        protected override void InstanceSetOrderParams(OrderParams orderParams)
        {
            myParameters = orderParams;
        }

        protected override bool InstanceIsOrderApplicable()
        {
            //if(GetMyWrapper().AmIFirstInQueue())
            //Debug.Log(GetOrderReceiver().GetCurrentOrderInQueue());
            if(Order.GetReceiver(GetMyWrapper()).GetCurrentOrderInQueue() == GetMyWrapper())
            {
                return true;
            }
            else
            {
                return false;
            }

            //return true;
        }

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
            orderPhasesFSM.AddState(OrderPhase.InitialState);

            orderPhasesFSM.AddState(OrderPhase.Registration,
                () =>
                {
                    Order.RegisterIfAppropriate(GetMyWrapper());
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
                });

            orderPhasesFSM.AddState(OrderPhase.ExecutionWaitingToStart,
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
                    orderPhasesFSM.CurrentState = OrderPhase.Disposed;
               });

            orderPhasesFSM.AddState(OrderPhase.Disposed,
            () =>
            {
                GetMyWrapper().DestroyWrappedReference();

                if (InstanceGetOrderReceiver().GetCurrentOrderInQueue() != null)
                    //&& Order.GetConfirmationFromReceiver(InstanceGetOrderReceiver().GetCurrentOrderInQueue()))
                {
                    Order.TryStartExecution(InstanceGetOrderReceiver().GetCurrentOrderInQueue());
                }                    
            });
               
            orderPhasesFSM.CurrentState = OrderPhase.InitialState;
        }

        public void SetBuildingToBuild(MapMarkerWrapper<BuildingMarker> buildingMarkerWrapper)
        {
            buildingMarkerWrapper.SubscribeOnClearance(() => buildingMarkerWrapper = null);
            GetMyWrapper().SubscribeOnClearance(
                () => { 
                    if (buildingMarkerWrapper != null
                    && BuiltStructure.GetHP(buildingMarkerWrapper.GetWrappedAs<BuildingMarker>().builtStructureWrapper) == 0)
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
                                new Condition(() => { return Order.ReceiverExists(GetMyWrapper()) && CanActuallyBuild(); }, Stops.LOWER_PRIORITY,
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

        private bool CanActuallyBuild()
        {
            if (Vector3.Distance(buildingMarkerWrapper.GetWrappedAs<BuildingMarker>().myPosition, unitWrapper.WrappedObject.myMover.GetPosition())
                < 6 && _materials > 0)
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
                builtStructureWrapper = buildingMarkerWrapper.GetWrappedAs<BuildingMarker>().CreateBuiltStructure<BuiltStructure>();
                GetMyWrapper().SubscribeOnClearance(() => builtStructureWrapper = null);
            }

            _previousmaterials = _materials;
            _materials = Mathf.Max(-1, _materials - buildingRate);

            if (_materials > 0)
            {
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