using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Helpers;
using VariousUtilsExtensions;

namespace Core.MapMarkers
{
    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    /// <summary>
    /// A class whose subclass are intended to be representations of various marks put on the map, may it be
    /// </summary>   
    public abstract class MapMarker : MonoBehaviour, IHasRefWrapper<MapMarkerWrapper>
    {

        protected MapMarkerWrapper _myWrapper;
        
        public MapMarkerWrapper GetRefWrapper()
        { 
            return _myWrapper;
        }

    }
}