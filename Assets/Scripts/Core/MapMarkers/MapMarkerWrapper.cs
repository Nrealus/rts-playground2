using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;

namespace Core.MapMarkers
{

    public class MapMarkerWrapper<T> : MapMarkerWrapper where T : MapMarker
    {

        public MapMarkerWrapper(MapMarker wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
            
        }        

    }

    public abstract class MapMarkerWrapper : ReferenceWrapper<MapMarker>
    {
        
        public MapMarkerWrapper(MapMarker wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
            
        }        

        public T GetWrappedAs<T>() where T : MapMarker
        {
            return WrappedObject as T;
        }

    }

}