using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;
using NPBehave;
using Core.MapMarkers;

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

        protected override IOrderable InstanceGetOrderReceiver()
        {
            return unitWrapper;
        }
        
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
            orderPhasesFSM.AddState(OrderPhase.InitialState);

            orderPhasesFSM.AddState(OrderPhase.Registration,
                () =>
                {
                    Order.RegisterIfAppropriate(GetMyWrapper());
                });

            orderPhasesFSM.AddState(OrderPhase.NotReadyToStartExecution,
                () =>
                {

                });

            orderPhasesFSM.AddState(OrderPhase.RequestConfirmation,
                () =>
                {
                    if (InstanceIsOrderApplicable()) // individually ?
                    {
                        orderPhasesFSM.CurrentState = OrderPhase.AllGoodToStartExecution;
                    }
                    else
                    {
                        orderPhasesFSM.CurrentState = OrderPhase.NotReadyToStartExecution;
                    }
                });

            orderPhasesFSM.AddState(OrderPhase.AllGoodToStartExecution,
                () =>
                {
                    //GetMyWrapper().RegisterMeIfAppropriate();
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

                if (InstanceGetOrderReceiver().GetCurrentOrderInQueue() != null
                    && Order.GetConfirmationFromReceiver(InstanceGetOrderReceiver().GetCurrentOrderInQueue()))
                {
                    Order.TryStartExecution(InstanceGetOrderReceiver().GetCurrentOrderInQueue());
                }                    
            });
               
            orderPhasesFSM.CurrentState = OrderPhase.InitialState;
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

        public void AddWaypoint(MapMarkerWrapper<WaypointMarker> waypointWrapper)
        {
            if (!waypointMarkerWrappersList.Contains(waypointWrapper))
            {
                waypointWrapper.SubscribeOnClearance(() => RemoveClearedWaypointWrapper(waypointWrapper));
                GetMyWrapper().SubscribeOnClearance(() => waypointWrapper.DestroyWrappedReference());
                this.waypointMarkerWrappersList.Add(waypointWrapper);
            }
        }

        public void AddWaypoints(List<MapMarkerWrapper<WaypointMarker>> waypointWrapper)
        {
            this.waypointMarkerWrappersList.AddRange(waypointWrapper);
        }

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

    }
}