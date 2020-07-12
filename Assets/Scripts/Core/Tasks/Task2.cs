using System.Collections;
using System.Collections.Generic;
using Core.Handlers;
using UnityEngine;

namespace Core.Tasks
{
    public abstract class Task2 : Task
    {

        protected override bool InstanceTryStartExecution()
        {
            if (IsInPhase(GetRefWrapper(), OrderPhase.Staging))
            {
                if(GetSubject(GetRefWrapper()).GetTaskPlan().IsFirstInlineActiveTaskInPlan(GetRefWrapper()))
                {
                    SetPhase(GetRefWrapper(), OrderPhase.ExecutionWaitingTimeToStart);
                    return true;
                }
                else
                {
                    if (InstanceGetParameters().ContainsExecutionMode(TaskParams.TaskExecutionMode.InstantOverrideAll))
                    {
                        // "delete" orders that start after the starting time of this order
                        foreach (var ow in GetSubject(GetRefWrapper()).GetTaskPlan().GetAllActiveTasksFromPlan())
                        {
                            if (ow != GetRefWrapper() && GetSubject(GetRefWrapper()).GetTaskPlan().IsActiveTaskInPlanAfterOther(GetRefWrapper(), ow))
                            {
                                EndExecution(ow);
                            }
                            else
                            {
                                break;
                            }
                        }
                        
                        if (GetSubject(GetRefWrapper()).GetTaskPlan().GetFirstInlineActiveTaskInPlan() != null)
                        {
                            SetPhase(GetRefWrapper(), Task.OrderPhase.ExecutionWaitingTimeToStart);  
                            return true;
                        }
                        
                        return false;
                    }
                }
            }

            return false;
        }


        protected override void InitPhasesFSM()
        {
            
            orderPhasesFSM.AddState(OrderPhase.Initial);

            orderPhasesFSM.AddState(OrderPhase.Staging);

            orderPhasesFSM.AddState(OrderPhase.ExecutionWaitingTimeToStart,
                () =>
                {
                    if(!Task.GetParameters(GetRefWrapper()).plannedStartingTime.isInitialized)
                    {
                        Task.SetPhase(GetRefWrapper(), Task.OrderPhase.Execution);
                    }
                    else if (TimeHandler.HasTimeJustPassed(Task.GetParameters(GetRefWrapper()).plannedStartingTime))
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

            orderPhasesFSM.AddState(OrderPhase.Cancelled,
                () =>
                {
                    
                },
                () =>
                {

                });

            orderPhasesFSM.AddState(OrderPhase.End,
               () =>
               {

               },
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
                
                if (GetSubject(GetRefWrapper()).GetTaskPlan().GetNextInlineActiveTaskInPlan(GetRefWrapper()) != null
                    && GetParameters(GetSubject(GetRefWrapper()).GetTaskPlan().GetNextInlineActiveTaskInPlan(GetRefWrapper())).ContainsExecutionMode(TaskParams.TaskExecutionMode.Chain))
                {
                    nextActiveOrder = InstanceGetSubject().GetTaskPlan().GetNextInlineActiveTaskInPlan(GetRefWrapper());
                }

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

        protected abstract void UpdateExecution();

    }
}
