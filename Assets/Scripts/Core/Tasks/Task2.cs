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

        protected override void InitPhasesFSM()
        {
            
            orderPhasesFSM.AddState(TaskPhase.Initial);

            orderPhasesFSM.AddState(TaskPhase.Staging);

            orderPhasesFSM.AddState(TaskPhase.WaitToStartExecution,
                EnterExecutionWaitingTimeToStart,
                UpdateExecutionWaitingTimeToStart);

            orderPhasesFSM.AddState(TaskPhase.Execution,
                null/*EnterExecution*/,
                UpdateExecution);

            orderPhasesFSM.AddState(TaskPhase.Paused);

            orderPhasesFSM.AddState(TaskPhase.Cancelled,
                () =>
                { },
                () =>
                { });

            orderPhasesFSM.AddState(TaskPhase.End,
               null/*EnterEnd*/,
               () =>
               {
                    SetPhase(TaskPhase.End2);
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

                if (GetTaskPlan()?.GetTaskInPlanAfter(this) != null
                    && GetTaskPlan().GetTaskInPlanAfter(this).GetParameters().ContainsExecutionMode(TaskParams.TaskExecutionMode.Chain))
                {
                    nextActiveOrder = new TaskWrapper(GetTaskPlan().GetTaskInPlanAfter(this));
                }
                /*else
                {
                    GetTaskPlan().EndPlanExecution();
                }*/
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

        /*protected virtual void EnterExecution()
        {
            //GetParameters().plannedStartingTime = TimeHandler.CurrentTime();
        }*/

        protected abstract void UpdateExecution();

        /*protected virtual void EnterEnd()
        {

        }*/

    }
}
