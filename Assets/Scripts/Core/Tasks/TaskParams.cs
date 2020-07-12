using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nrealus.Extensions;

namespace Core.Tasks
{
    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    /// <summary>
    /// This class is basically a container for various parameters, and "arguments" for orders.
    /// </summary>
    public class TaskParams
    {
        
        public enum TaskExecutionMode
        {
            InstantOverrideAll,
            Chain,
            WaitForReactionAtEnd,
            AskForConfirmationRightBeforeStart,
            
        }

        private List<TaskExecutionMode> _executionMode = new List<TaskExecutionMode>();

        public bool ContainsExecutionMode(TaskExecutionMode mode)
        {
            return _executionMode.Contains(mode);
        }

        public void AddExecutionMode(TaskExecutionMode mode) // Subject to change when this class will get more formal
        {
            _executionMode.Add(mode);
            if (ContainsExecutionMode(TaskExecutionMode.InstantOverrideAll) && mode == TaskExecutionMode.Chain)
                _executionMode.Remove(TaskExecutionMode.InstantOverrideAll);
        }

        public TimeStruct plannedStartingTime { get; /*private*/ set; }

        public bool isPassive { get; /*private*/ set; }

        
        public static TaskParams DefaultParam()
        { 
            var res = new TaskParams();
            res.AddExecutionMode(TaskExecutionMode.InstantOverrideAll);
            return res;
        }

        public static TaskParams PassiveParam()
        { 
            var res = new TaskParams();
            res.AddExecutionMode(TaskExecutionMode.InstantOverrideAll);
            res.isPassive = true;
            return res;
        }


        
        public TaskParams()
        {
            
        }

    }
}