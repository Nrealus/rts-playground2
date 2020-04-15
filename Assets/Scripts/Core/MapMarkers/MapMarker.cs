using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Helpers;
using VariousUtilsExtensions;

namespace Core.MapMarkers
{
    public abstract class MapMarker : 
        IHasRefWrapper<MapMarkerWrapper>
    {

        protected MapMarkerWrapper _myWrapper;

        public MapMarkerWrapper GetMyWrapper() { return _myWrapper; }
        public MapMarkerWrapper<T> GetMyWrapper<T>() where T : MapMarker { return _myWrapper as MapMarkerWrapper<T>; }
        public T GetMyWrapper2<T>() where T : MapMarkerWrapper { return _myWrapper as T; }

        /*public void ClearWrapper()
        {
            GetMyWrapper().DestroyWrappedReference();
        }*/

        public abstract void UpdateMe();

    }
}