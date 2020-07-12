using System;
using System.Collections;
using System.Collections.Generic;
using Core.Selection;
using Core.Tasks;
using UnityEngine;
using VariousUtilsExtensions;

namespace Core.MapMarkers
{
    
    public class TaskMarkerWrapper<T> : TaskMarkerWrapper where T : TaskMarker
    {

        public new T GetWrappedReference()
        {
            return _wrappedObject as T;
        }

        protected void Special_SetWrappedObject(T value)
        {
            _wrappedObject = value;
        }

        public TaskMarkerWrapper(T wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
        }

        protected override void Constructor1(MapMarker wrappedObject, Action nullifyPrivateRefToWrapper)
        {
            Special_SetWrappedObject(wrappedObject as T);
            Constructor2(nullifyPrivateRefToWrapper);
        }

    }
    
    /// <summary>
    /// The RefWrapper for Task.
    /// </summary>      
    public abstract class TaskMarkerWrapper : MapMarkerWrapper<TaskMarker>//, ISelectable<Task>
    {
       
        public new TaskMarker GetWrappedReference()
        {
            return _wrappedObject as TaskMarker;
        }

        protected override void Special_SetWrappedObject(MapMarker value)
        {
            _wrappedObject = value;
        }

        public new TaskMarkerWrapper<T> CastWrapper<T>() where T : TaskMarker
        {
            return (TaskMarkerWrapper<T>) this;
        }

        public new T GetCastReference<T>() where T : TaskMarker
        {
            return (T) GetWrappedReference();
        }

        public TaskMarkerWrapper(TaskMarker wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
        }

    }
    
}