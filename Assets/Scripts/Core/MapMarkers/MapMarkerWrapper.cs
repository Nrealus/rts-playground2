using System;
using System.Collections;
using System.Collections.Generic;
using Core.Selection;
using UnityEngine;
using Nrealus.Extensions;
using Nrealus.Extensions.ReferenceWrapper;
using Nrealus.Extensions.Observer;

namespace Core.MapMarkers
{

    /****** Author : nrealus ****** Last documentation update : 12-07-2020 ******/

    /// <summary>
    /// The generic RefWrapper for specific types of MapMarkers.
    /// </summary>      
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
    /// The RefWrapper for MapMarker.
    /// </summary>      
    public abstract class MapMarkerWrapper : RefWrapper2<MapMarker>, ISelectable
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

        Y ISelectable.GetSelectableAsReferenceWrapperSpecific<Y>()
        {
            return this as Y;
        }

        #endregion

    }
    
}