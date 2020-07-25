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
    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    /// <summary>
    /// OUTDATED
    /// This "passive" order (See OrderPlan) can specify targets for units. WIP (as most things but here especially - UnitROE stuff on the horizon..?)
    /// </summary>
    #if false
    public class EngageAtPositionsTask : Task, IHasRefWrapper<TaskWrapper<EngageAtPositionsTask>>
    {

        public new TaskWrapper<EngageAtPositionsTask> GetRefWrapper()
        {
            return _myWrapper as TaskWrapper<EngageAtPositionsTask>;
        }

        private List<MapMarkerWrapper<FirePositionMarker>> firePositionMarkerWrappersList = new List<MapMarkerWrapper<FirePositionMarker>>();

        private TaskParams myParameters = TaskParams.DefaultParam();

        private UnitWrapper unitWrapper;
        private class UnitWrapperExecutionState
        {
            public int currentWaypointIndex;
            public bool endedPath;

        }
        private Dictionary<UnitWrapper, UnitWrapperExecutionState> currentExecutionStatePerUnit = new Dictionary<UnitWrapper, UnitWrapperExecutionState>();


        protected override ITaskSubject InstanceGetSubject()
        {
            return unitWrapper;
        }
        
        private TaskPlan2 taskPlan;
        protected override TaskPlan2 InstanceGetTaskPlan()
        {
            return taskPlan;
        }

        /*protected override TaskMarkerWrapper InstanceGetTaskMarker()
        {
            throw new System.NotImplementedException();
            //return unitWrapper;
        }*/

        protected override void InstanceSetSubject(ITaskSubject subject, TaskWrapper predecessor, TaskWrapper successor)
        {
           
            unitWrapper = subject as UnitWrapper;

            //unitWrapper.GetTaskPlan().QueuePassiveOrderToPlan(GetRefWrapper(), predecessor, successor);

            SetPhase(GetRefWrapper(), OrderPhase.Staging);
            
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

        protected override TaskParams InstanceGetParameters()
        {
            return myParameters;
        }

        /*protected override void InstanceSetTaskMarker(TaskMarkerWrapper taskMarkerWrapper)
        {
            //this.taskMarkerWrapper = taskMarkerWrapper;
            
            //Task.SetSubject(GetRefWrapper(), null, null, Task.GetTaskMarker(GetRefWrapper()).GetWrappedReference().GetTaskSubjects().GetEnumerator().Current);
        }*/
        
        /*protected override void InstanceSetOrderParams(OrderParams orderParams)
        {
            myParameters = orderParams;
        }*/

        protected override bool InstanceTryStartExecution()
        {
            // replace some passive orders if certain case / situation (specific or not) ?

            if (IsInPhase(GetRefWrapper(), OrderPhase.Staging))
            {
                SetPhase(GetRefWrapper(), OrderPhase.WaitToStartExecution);
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


        public EngageAtPositionsTask()
        {
            _myWrapper = new TaskWrapper<EngageAtPositionsTask>(this, () => {_myWrapper = null;});

            GetParameters(GetRefWrapper()).isPassive = true;

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

        protected override void InitPhasesFSM()
        {
            
            orderPhasesFSM.AddState(OrderPhase.Initial);

            orderPhasesFSM.AddState(OrderPhase.Staging);

            orderPhasesFSM.AddState(OrderPhase.WaitToStartExecution,
                () =>
                {
                    if(!Task.GetParameters(GetRefWrapper()).plannedStartingTime.isInitialized)
                    {
                        Task.SetPhase(GetRefWrapper(), Task.OrderPhase.Execution);
                    }
                    else if (TimeHandler.HasTimeJustPassed(Task.GetParameters(GetRefWrapper()).plannedStartingTime))
                    //    || TimeHandler.HasTimeAlreadyPassed(Order.GetParameters(GetMyWrapper()).startingTime))
                    {
                        Task.SetPhase(GetRefWrapper(), Task.OrderPhase.Execution);                
                    }
                },
                () =>
                {
                    if(TimeHandler.HasTimeJustPassed(Task.GetParameters(GetRefWrapper()).plannedStartingTime))
                    {
                        Task.SetPhase(GetRefWrapper(), Task.OrderPhase.Execution);
                    }
                });

            orderPhasesFSM.AddState(OrderPhase.Execution,
                () =>
                {
                    GetParameters(GetRefWrapper()).plannedStartingTime = TimeHandler.CurrentTime();                
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
                    SetPhase(GetRefWrapper(), OrderPhase.End2);
               });

            bool waitForReactionAtEnd = false;
            TaskWrapper nextActiveOrder = null;

            orderPhasesFSM.AddState(OrderPhase.End2,
            () =>
            {
                if(Task.GetParameters(GetRefWrapper()).ContainsExecutionMode(TaskParams.TaskExecutionMode.WaitForReactionAtEnd))
                {
                    waitForReactionAtEnd = true;
                }
                
                /*if (GetSubject(GetRefWrapper()).GetTaskPlan().GetNextInlinePassiveOrderInPlan(GetRefWrapper()) != null
                    && GetParameters(GetSubject(GetRefWrapper()).GetTaskPlan().GetNextInlinePassiveOrderInPlan(GetRefWrapper())).ContainsExecutionMode(TaskParams.TaskExecutionMode.Chain))
                {
                    nextActiveOrder = InstanceGetSubject().GetTaskPlan().GetNextInlinePassiveOrderInPlan(GetRefWrapper());
                }*/

            },
            () =>
            {
                if (waitForReactionAtEnd == false)
                    SetPhase(GetRefWrapper(), OrderPhase.Disposed);
            });

            orderPhasesFSM.AddState(OrderPhase.Disposed,
            () =>
            {
                GetRefWrapper().DestroyWrappedReference();

                if (nextActiveOrder != null && nextActiveOrder.GetWrappedReference() != null)
                    Task.TryStartExecution(nextActiveOrder);

            });
               
            orderPhasesFSM.CurrentState = OrderPhase.Initial;
        }

        #region Specific behaviour logic

        public void AddFirePosition(MapMarkerWrapper<FirePositionMarker> firePositionWrapper)
        {
            if (firePositionWrapper != null && !firePositionMarkerWrappersList.Contains(firePositionWrapper))
            {
                firePositionWrapper.SubscribeOnClearance(() => RemoveClearedFirePositionWrapper(firePositionWrapper));
                GetRefWrapper().SubscribeOnClearance(() => firePositionWrapper.DestroyWrappedReference());
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
            //foreach (var uw in Unit.GetUnitPieceWrappersInUnit(unitWrapper))
            foreach (var uw in Unit.GetMyselfAndSubUnitsWrappers(unitWrapper))
            {
                if (Task.IsInPhase(GetRefWrapper(), OrderPhase.Execution))
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
    #endif
}