using System.Collections;
using System.Collections.Generic;
using Core.MapMarkers;
using UnityEngine;
using Nrealus.Extensions;
using Nrealus.Extensions.ReferenceWrapper;

namespace Core.Tasks
{
    /****** Author : nrealus ****** Last documentation update : 09-07-2020 ******/
    
    public interface ITaskSubjectBase
    {
        bool IsWrappedObjectNotNull();        

        //bool IsOrderApplicable(OrderWrapper order);

    }

    /// <summary>
    /// This interface is implemented by classes that can be "subjects" of Tasks, like units that receive and execute them.
    /// They are passed to Tasks (via TaskWrappers)
    /// </summary> 
    public interface ITaskSubject : ITaskSubjectBase
    {

        RefWrapper GetTaskSubjectAsReferenceWrapperNonGeneric();

        Y GetTaskSubjectAsReferenceWrapperSpecific<Y>() where Y : RefWrapper;
     
        TaskPlan GetTaskPlan();

        void SetTaskPlan(TaskPlan taskPlan);

    }

    /*public interface ITaskSubject<T> : ITaskSubject
    {
        //RefWrapper<T> GetOrderableAsReferenceWrapperGeneric();

        //new Y GetOrderableAsReferenceWrapperSpecific<Y>() where Y : ReferenceWrapper<T>;

    }*/
    
}