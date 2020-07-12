using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Helpers;
using Nrealus.Extensions;

namespace Core.MapMarkers
{
    /****** Author : nrealus ****** Last documentation update : 12-07-2020 ******/

    /// <summary>
    /// A class whose subclasses are intended to be "concrete" (MonoBehaviour) "markers" on the map.
    /// These markers may fill different roles : simple indicators of an in-game situation, or "parameter" indicators for Tasks, like an area to target,
    /// and/or elements of interactive UI (as opposed to simple information display) which can be selected, and moved around for example.
    /// (-> see TaskMarker and an implementation : MoveTaskMarker)
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