using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nrealus.Extensions;
using System.Text;

namespace Core.Tasks
{
    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    /// <summary>
    /// This class is basically a container for various parameters, and "arguments" for orders.
    /// </summary>
    public class TaskParams
    {
        
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
            //res.isPassive = true;
            return res;
        }

        #region Main declarations

        public enum TaskExecutionMode
        {
            InstantOverrideAll,
            Chain,
            WaitForReactionAtEnd,
            AskForConfirmationRightBeforeStart,
        }

        private List<TaskExecutionMode> executionMode = new List<TaskExecutionMode>();

        public TimeStruct plannedStartingTime { get; /*private*/ set; }

        private List<ITaskSubject> paramSubjects = new List<ITaskSubject>();

        #endregion

        #region Public functions and methods
        
        public List<ITaskSubject> GetParameterSubjects()
        {
            return paramSubjects;
        }

        public bool ContainsExecutionMode(TaskExecutionMode mode)
        {
            return executionMode.Contains(mode);
        }

        public void AddExecutionMode(TaskExecutionMode mode) // Subject to change when this class will get more formal
        {
            executionMode.Add(mode);
            if (ContainsExecutionMode(TaskExecutionMode.InstantOverrideAll) && mode == TaskExecutionMode.Chain)
                executionMode.Remove(TaskExecutionMode.InstantOverrideAll);
        }

        public void AddParameterSubjects(IEnumerable<ITaskSubject> subjs)
        {
            foreach(var v in subjs)
                AddParameterSubject(v);
        }

        public void AddParameterSubject(ITaskSubject subj)
        {
            if (!paramSubjects.Contains(subj))
            {
                paramSubjects.Add(subj);
                subj.SubscribeOnDestruction(removeParamSubjKey,
                () => RemoveParameterSubject(subj));
            }
        }

        public void RemoveParameterSubjects(IEnumerable<ITaskSubject> subjs)
        {
            foreach (var v in new List<ITaskSubject>(subjs))
                RemoveParameterSubject(v);
        }

        public void RemoveParameterSubject(ITaskSubject subj)
        {
            if (paramSubjects.Remove(subj))
            {
                subj.UnsubscribeOnDestruction(removeParamSubjKey);
            }
        }

        #endregion

        #region Initialisation

        private static int _instcount;
        private string removeParamSubjKey;
        public TaskParams()
        {
            _instcount++;
            removeParamSubjKey = new StringBuilder("removeparamsubj").Append(_instcount).ToString();
        }

        #endregion
    }
}