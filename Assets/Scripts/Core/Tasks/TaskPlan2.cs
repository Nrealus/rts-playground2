using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.MapMarkers;
using UnityEngine;
using Nrealus.Extensions;
using Nrealus.Extensions.Observer;
using Core.Units;
using Nrealus.Extensions.ReferenceWrapper;
using Core.Selection;

namespace Core.Tasks
{

    /****** Author : nrealus ****** Last documentation update : 25-07-2020 ******/

    /// <summary>
    /// A class representing a (for example tactical) plan made up of Tasks that a ITaskSubject may have.
    /// The plan holds a list of tasks, which are supposed to be executed in order, one by one.
    /// There are still aspects of the global model that are subject to change.
    /// For example, for now, an ITaskSubject cannot know what TaskPlans' subject it is.
    /// This will be clarified later, as I don't yet want to exclude the possibility of several plans active at the same for the same subject.
    /// </summary>
    public class TaskPlan2
    {

        #region Main declarations

        private RefWrapper <ITaskAgent> _taskAgent;
        public ITaskAgent GetOwnerAgent() { return _taskAgent?.Value; }

        private List<Task> tasks = new List<Task>();

        #endregion

        public TaskPlan2(ITaskAgent agent)
        {
            _taskAgent = new RefWrapper <ITaskAgent>(agent);
        }

        public void StartPlanExecution()
        {
            if (GetOwnerAgent() != null)
                GetCurrentTaskInPlan().TryStartExecution();
        }

        public void EndPlanExecution()
        {
            if (GetOwnerAgent() != null)
            {
                GetOwnerAgent().EndAndUnregisterOwnedPlan(this);
                foreach (var v in new List<Task>(tasks))
                {
                    v.EndExecution();
                    v.DestroyThis();
                }
                //tasks.Clear();
                _taskAgent = null;
            }
        }

        public bool IsPlanBeingExecuted()
        {
            return GetCurrentTaskInPlan()?.IsInPhase(Task.TaskPhase.Execution) ?? false;
        }

        #region Tasks collection manipulation

        public Task GetCurrentTaskInPlan()
        {
            if (tasks.Count > 0)
                return tasks[0];
            else
                return null;
        }

        public Task GetLastTaskInPlan()
        {
            if (tasks.Count > 0)
                return tasks[tasks.Count-1];
            else
                return null;
        }

        public Task GetTaskInPlanAfter(Task tw)
        {
            var c = tasks.Count;
            for(int i = 0; i < c; i++)
            {
                if (tasks[i] == tw && i <= c-2)
                    return tasks[i+1];
            }
            return null;
        }

        public Task GetTaskInPlanBefore(Task tw)
        {
            var c = tasks.Count;
            for(int i = 0; i < c; i++)
            {
                if (tasks[i] == tw && i >= 1)
                    return tasks[i-1];
            }
            return null;
        }

        public bool AddTaskToPlan(Task t)
        {
            if (t != null && !tasks.Contains(t))
            {
                tasks.Add(t);

                t.SetTaskPlan(this);
                t.SubscribeOnDestruction("removetask", () => { RemoveTaskFromPlan(t); });
                //AddOnClearance(t, () => t.EndExecution()/*Task.DestroyWrappedReference()*/);
                                
                return true;
            }
            else
            {
                return false;    
            }
        }

        private bool RemoveTaskFromPlan(Task t)
        {
            if(tasks.Contains(t))
            {
                tasks.Remove(t);

                t.SetTaskPlan(null);

                t.UnsubscribeOnDestruction("removetask");
                //RemoveOnClearance(t);
                
                return true;
            }
            else
            {
                return false;    
            }
        }

        #endregion

    }

}