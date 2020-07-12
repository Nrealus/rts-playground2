using System;
using System.Collections;
using System.Collections.Generic;
using Core.Selection;
using UnityEngine;
using VariousUtilsExtensions;

namespace Core.MapMarkers
{

    public class MapMarkerWrapper<T> : MapMarkerWrapper where T : MapMarker
    {

        public new T GetWrappedReference()
        {
            return _wrappedObject as T;
        }

        protected void Special_SetWrappedObject(T value)
        {
            _wrappedObject = value;
        }

        public MapMarkerWrapper(T wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
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
    public abstract class MapMarkerWrapper : RefWrapper2<MapMarker>, ISelectable//, ISelectable<Task>
    {
        
        public MapMarkerWrapper<T> CastWrapper<T>() where T : MapMarker
        {
            return (MapMarkerWrapper<T>) this;
        }

        public MapMarkerWrapper(MapMarker wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
        }

        #region ISelectable explicit implementations

        private EasyObserver<string, (Selector,bool)> onSelectionStateChange = new EasyObserver<string, (Selector, bool)>();

        public EasyObserver<string, (Selector,bool)> GetOnSelectionStateChangeObserver()
        {
            return onSelectionStateChange;
        }
        
        void ISelectable.InvokeOnSelectionStateChange(Selector selector, bool b)
        {
            onSelectionStateChange.Invoke((selector, b));
        }

        RefWrapper ISelectable.GetSelectableAsReferenceWrapperNonGeneric()
        {
            return this;
        }

        /*RefWrapper<Unit> ISelectable<Unit>.GetSelectableAsReferenceWrapperGeneric()
        {
            return this;
        }*/

        Y ISelectable.GetSelectableAsReferenceWrapperSpecific<Y>()
        {
            return this as Y;
        }

        #endregion

    }
    
}