using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nrealus.Extensions;
using Core.Handlers;
using GlobalManagers;
using System;
using Core.Selection;
using Core.Tasks;
using Nrealus.Extensions.ReferenceWrapper;
//using TaskWrapper = Core.Tasks.TaskWrapper<Core.Tasks.Task>;

namespace Core.Tasks
{

    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    /// <summary>
    /// Subclass of TaskWrapper
    /// </summary>
    /// <typeparamref name="T">Specific type of wrapped Task</typeparamref>
    public class TaskWrapper<T> : TaskWrapper where T : Task
    {
        
        public new T GetWrappedReference()
        {
            return _wrappedObject as T;
        }

        protected void Special_SetWrappedObject(T value)
        {
            _wrappedObject = value;
        }

        public TaskWrapper(T wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
        }

        protected override void Constructor1(Task wrappedObject, Action nullifyPrivateRefToWrapper)
        {
            Special_SetWrappedObject(wrappedObject as T);
            Constructor2(nullifyPrivateRefToWrapper);
        }
        
    }
    
    /// <summary>
    /// The RefWrapper for Task.
    /// </summary>      
    public abstract class TaskWrapper : RefWrapper2<Task>//, ISelectable<Task>
    {

        public TaskWrapper<T> CastWrapper<T>() where T : Task
        {
            return (TaskWrapper<T>) this;
        }

        public TaskWrapper(Task wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
        }
        
    }

}