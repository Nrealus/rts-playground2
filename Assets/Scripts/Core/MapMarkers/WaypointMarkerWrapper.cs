using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.MapMarkers
{
    public class WaypointMarkerWrapper : MapMarkerWrapper<WaypointMarker>
    {
        public WaypointMarkerWrapper(MapMarker wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
        }

        public static void UpdateWaypointMarker(WaypointMarkerWrapper waypointMarkerWrapper, bool following, Vector3 screenPositionToFollow)
        {
            if(waypointMarkerWrapper.WrappedObject != null)
                waypointMarkerWrapper.GetWrappedAs<WaypointMarker>().UpdateWaypointMarker(following, screenPositionToFollow);
        }

    }

}

