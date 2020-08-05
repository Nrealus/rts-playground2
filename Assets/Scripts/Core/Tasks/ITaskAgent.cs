using System.Collections;
using System.Collections.Generic;
using Core.MapMarkers;
using UnityEngine;
using Nrealus.Extensions;
using Nrealus.Extensions.ReferenceWrapper;
using Core.Helpers;

namespace Core.Tasks
{
    /****** Author : nrealus ****** Last documentation update : 25-07-2020 ******/
    
    /// <summary>
    /// A class implementing this interface shows that it can be a "agent" of a Task, like a unit that receives and executes one.
    /// There is a possibility that it may expose methods allowing an agent knowledge of TaskPlans which have it as a agent.
    /// </summary> 
    public interface ITaskAgent : IDestroyable
    {
        TaskPlan2 CreateAndRegisterNewOwnedPlan();

        void EndAndUnregisterOwnedPlan(TaskPlan2 taskPlan);

        List<TaskPlan2> GetOwnedPlans();

        void RegisterTaskWhereAgentIsSubject(Task task);

        void UnregisterTaskWhereAgentIsSubject(Task task);

        List<Task> GetTasksWhereIsInternalSubject();
        
    }
    
}