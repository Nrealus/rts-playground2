using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.MapMarkers
{
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