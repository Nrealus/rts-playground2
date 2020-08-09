using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nrealus.Extensions;
using System.Text;
using Core.Units;

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

        private List <IActorGroup> paramActors = new List <IActorGroup>();

        #endregion

        #region Public functions and methods
        
        public List <IActorGroup> GetParameterActors()
        {
            return paramActors;
        }

        public bool ContainsExecutionMode(TaskExecutionMode mode)
        {
            return executionMode.Contains(mode);
        }

        public void AddExecutionMode(TaskExecutionMode mode) // Actorect to change when this class will get more formal
        {
            executionMode.Add(mode);
            if (ContainsExecutionMode(TaskExecutionMode.InstantOverrideAll) && mode == TaskExecutionMode.Chain)
                executionMode.Remove(TaskExecutionMode.InstantOverrideAll);
        }

        public void AddParameterActors(IEnumerable<IActorGroup> actors)
        {
            foreach(var v in actors)
                AddParameterActor(v);
        }

        public void AddParameterActor(IActorGroup actor)
        {
            if (!paramActors.Contains(actor))
            {
                paramActors.Add(actor);
                actor.SubscribeOnDestruction(removeParamActorKey,
                () => RemoveParameterAgent(actor));
            }
        }

        public void RemoveParameterActors(IEnumerable<IActorGroup> actors)
        {
            foreach (var v in new List <IActorGroup>(actors))
                RemoveParameterAgent(v);
        }

        public void RemoveParameterAgent(IActorGroup actor)
        {
            if (paramActors.Remove(actor))
            {
                actor.UnsubscribeOnDestruction(removeParamActorKey);
            }
        }

        #endregion

        #region Initialisation

        private static int _instcount;
        private string removeParamActorKey;
        public TaskParams()
        {
            _instcount++;
            removeParamActorKey = new StringBuilder("removeparamactor").Append(_instcount).ToString();
        }

        #endregion
    }
}