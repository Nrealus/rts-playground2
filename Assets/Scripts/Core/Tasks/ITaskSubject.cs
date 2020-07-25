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
    /// A class implementing this interface shows that it can be a "subject" of a Task, like a unit that receives and executes one.
    /// There is a possibility that it may expose methods allowing a subject knowledge of TaskPlans which have it as a subject.
    /// </summary> 
    public interface ITaskSubject : IDestroyable
    {
        //void AddToPlans(TaskPlan2 taskPlan);

        //void RemoveFromPlans(TaskPlan2 taskPlan);

        //IEnumerable<TaskPlan2> GetPlans();

        //TaskPlan2 GetTaskPlan();

        //void SetTaskPlan(TaskPlan2 taskPlan);

    }
    
}