using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;
using NPBehave;

namespace Core.Orders
{
    public class MoveOrder : Order
    {

        private List<Vector3> waypointsList = new List<Vector3>();
        private int currentWaypointIndex;

        private Root behaviourTree;
        private Clock btClock;

        private UnitWrapper unitWrapper;

        public override IOrderable GetOrderReceiver()
        {
            return unitWrapper;
        }
        
        public MoveOrder()
        {
            _myWrapper = new OrderWrapper<MoveOrder>(this);

            btClock = new Clock();

            BaseConstructor();
        }

        protected override void OrderPhasesFSMInit()
        {
            orderPhasesFSM.AddState(OrderPhase.Preparation,
                () =>
                {
                    GetMyWrapper().RegisterMeIfAppropriate();
                });

            orderPhasesFSM.AddState(OrderPhase.RequestConfirmation,
                () =>
                {
                    if (IsOrderApplicable()) // individually ?
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
                    //GetMyWrapper().RegisterOrderIfAppropriate(this);
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
                   //receiverWrapper.UnregisterOrder(this);
               },
               () =>
               {
                   DisposeBehaviourTree();
                   orderPhasesFSM.CurrentState = OrderPhase.Disposed;
               });

            orderPhasesFSM.AddState(OrderPhase.Disposed,
            () =>
            {
                ClearWrapper();
            });
               
            orderPhasesFSM.CurrentState = OrderPhase.Preparation;
        }

        public override void SetOrderReceiver(IOrderable orderable)
        {
            if (IsInPhase(OrderPhase.Preparation))
            {
                unitWrapper = orderable as UnitWrapper;
            }
            else
            {
                Debug.LogError("Order is not in setup phase anymore");
            }
        }

        public void AddWaypoints(List<Vector3> waypoints)
        {
            this.waypointsList.AddRange(waypoints);
        }

        public void AddWaypointsMeAndChildren(List<Vector3> waypoints)
        {
            foreach (var v in GetMeAndAllChildrenWrappersListOfType<MoveOrder>())
                ((MoveOrder)v.WrappedObject).AddWaypoints(waypoints);
        }

        private void UpdateBTClock()
        {
            btClock.Update(Time.deltaTime);
        }

        private Root CreateBehaviourTree()
        {
            return new Root(new Blackboard(btClock), btClock,
                        new Condition(() => { return IsInPhase(OrderPhase.Execution); },
                            new Sequence(
                                new Condition(() => { return ReceiverExists()/* && PathExists() && !PathFinished() && !IsInPhase(OrderPhase.Pause)*/; },
                                    new Action(NavigateAlongPath)),
                                new Condition(() => { return PathFinished(); },
                                    new Action(GetMyWrapper().EndExecution)))));
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
            return currentWaypointIndex >= waypointsList.Count;
        }

        private bool PathOntoLastPoint()
        {
            return currentWaypointIndex == waypointsList.Count - 1;
        }

        private float s = 0.1f;
        private void NavigateAlongPath()
        {
            unitWrapper.WrappedObject.myMover.MoveToPosition(waypointsList[currentWaypointIndex], s);

            if (unitWrapper.WrappedObject.myMover.DistanceConditionToPosition(waypointsList[currentWaypointIndex], 0.05f))
            {
                currentWaypointIndex++;
            }

        }

        public override bool IsOrderApplicable()
        {
            return true;
        }

    }
}