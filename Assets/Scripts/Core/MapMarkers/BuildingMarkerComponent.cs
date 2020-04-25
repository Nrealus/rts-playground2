using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.MapMarkers
{
    /// <summary>
    /// ---- General Description, by nrealus, last update : 23-04-2020 ----
    ///
    /// A Behaviour object linked to a BuildingMarker (through a MapMarkerWrapper)
    /// </summary>   
    public class BuildingMarkerComponent : MapMarkerComponent
    {

        public MapMarkerWrapper associatedMarkerWrapper;        
            
        private void Start()
        {
            
        }

        private void Update() 
        {
            associatedMarkerWrapper.WrappedObject.UpdateMe();
        }

    }   
}