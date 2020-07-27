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
        
        /*#region Local Clearance

        private EasyObserver<Task> onClearance = new EasyObserver<Task>();
        public void Clear()
        {
            onClearance.Invoke();
            onClearance.UnsubscribeAllEventHandlerMethods();
            //GetSubject().RemoveFromPlans(this);
        }

        private void AddOnClearance(Task key, Action action)
        {
            onClearance.SubscribeEventHandlerMethod(key,action);
        }

        private void RemoveOnClearance(Task key)
        {
            onClearance.UnsubscribeEventHandlerMethod(key);
        }

        #endregion*/

        private static int _counter;
        private string _instKey;
        public TaskPlan2(ITaskSubject taskSubject)
        {
            _counter++;
            _instKey = new StringBuilder("unsubontaskplansubjectchange").Append(_counter).ToString();

            SetSubject(taskSubject);
        }

        private RefWrapper<ITaskSubject> _taskSubject;
        public ITaskSubject GetSubject() { return _taskSubject?.Value; }

        /*private*/public void SetSubject(ITaskSubject subject)
        {
            if (GetSubject() != null)
            {
                //GetSubject().Remove(this);
                GetSubject().UnsubscribeOnDestruction(_instKey);
            }

            _taskSubject = new RefWrapper<ITaskSubject>(subject);

            GetSubject().SubscribeOnDestruction(_instKey, () => SetSubject(null) );

            //GetSubject().AddToPlans(this);
        }

        private List<Task> tasks = new List<Task>();

        public void StartPlanExecution()
        {
            if (GetSubject() != null)
                GetCurrentTaskInPlan().TryStartExecution();
        }

        public void StopPlanExecution()
        {
            GetCurrentTaskInPlan().EndExecution();
        }

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

        public Task GetTaskInPlanFollowing(Task tw)
        {
            var c = tasks.Count;
            for(int i = 0; i < c; i++)
            {
                if (tasks[i] == tw && i <= c-2)
                    return tasks[i+1];
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

    }

}