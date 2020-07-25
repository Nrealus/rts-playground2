using System.Collections;
using System.Collections.Generic;
using Core.Handlers;
using UnityEngine;

namespace Core.Tasks
{
    public abstract class Task2 : Task
    {

        private TaskPlan2 taskPlan;
        protected override void InstanceSetTaskPlan(TaskPlan2 taskPlan)
        {
            this.taskPlan = taskPlan;
        }

        protected override TaskPlan2 InstanceGetTaskPlan()
        {
            return taskPlan;
        }

        protected override bool InstanceTryStartExecution()
        {
            if (IsInPhase(TaskPhase.Staging))
            {
                if(GetTaskPlan().GetCurrentTaskInPlan() == this)
                {
                    SetPhase(TaskPhase.WaitToStartExecution);
                    return true;
                }
                else
                {
                    if (GetParameters().ContainsExecutionMode(TaskParams.TaskExecutionMode.InstantOverrideAll))
                    {
                        GetTaskPlan().StopPlanExecution();
                        SetPhase(Task.TaskPhase.WaitToStartExecution);  
                        return true;
                    }
                    return false;
                }
            }

            return false;
        }


        protected override void InitPhasesFSM()
        {
            
            orderPhasesFSM.AddState(TaskPhase.Initial);

            orderPhasesFSM.AddState(TaskPhase.Staging);

            orderPhasesFSM.AddState(TaskPhase.WaitToStartExecution,
                EnterExecutionWaitingTimeToStart,
                UpdateExecutionWaitingTimeToStart);

            orderPhasesFSM.AddState(TaskPhase.Execution,
                EnterExecution,
                UpdateExecution);

            orderPhasesFSM.AddState(TaskPhase.Pause);

            orderPhasesFSM.AddState(TaskPhase.Cancelled,
                () =>
                { },
                () =>
                { });

            orderPhasesFSM.AddState(TaskPhase.End,
               () => 
               { },
               () =>
               {
                    SetPhase( TaskPhase.End2);
               });

            
            bool waitForReactionAtEnd = false;

            TaskWrapper nextActiveOrder = null;
            
            orderPhasesFSM.AddState(TaskPhase.End2,
            () =>
            {
                if(GetParameters().ContainsExecutionMode(TaskParams.TaskExecutionMode.WaitForReactionAtEnd))
                {
                    waitForReactionAtEnd = true;
                }
                if (GetTaskPlan().GetNextTaskInPlan(this) != null
                    && GetTaskPlan().GetNextTaskInPlan(this).GetParameters().ContainsExecutionMode(TaskParams.TaskExecutionMode.Chain))
                {
                    nextActiveOrder = new TaskWrapper(GetTaskPlan().GetNextTaskInPlan(this));
                }
            },
            () =>
            {
                if (waitForReactionAtEnd == false)
                    SetPhase(TaskPhase.Disposed);
            });

            orderPhasesFSM.AddState(TaskPhase.Disposed,
            () =>
            {
                DestroyThis();

                if (nextActiveOrder != null && nextActiveOrder.Value != null)
                    nextActiveOrder.Value.TryStartExecution();

            });
               
            orderPhasesFSM.CurrentState = TaskPhase.Initial;
        }

        protected virtual void EnterExecutionWaitingTimeToStart()
        {
            if(!GetParameters().plannedStartingTime.isInitialized)
            {
                SetPhase(Task.TaskPhase.Execution);
            }
            else if (TimeHandler.HasTimeJustPassed(GetParameters().plannedStartingTime))
            {
                SetPhase(Task.TaskPhase.Execution);                
            }
        }

        protected virtual void UpdateExecutionWaitingTimeToStart()
        {
            if(TimeHandler.HasTimeJustPassed(GetParameters().plannedStartingTime))
            {
                SetPhase(Task.TaskPhase.Execution);
            }
        }

        protected virtual void EnterExecution()
        {
            GetParameters().plannedStartingTime = TimeHandler.CurrentTime();
        }

        protected abstract void UpdateExecution();

    }
}
