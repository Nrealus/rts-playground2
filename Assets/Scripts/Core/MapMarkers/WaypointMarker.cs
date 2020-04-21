using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.MapMarkers
{
    public class WaypointMarker : MapMarker
    {

        public static void UpdateWaypointMarker(MapMarkerWrapper<WaypointMarker> waypointMarkerWrapper, bool following, Vector3 screenPositionToFollow)
        {
            if(waypointMarkerWrapper.WrappedObject != null)
                waypointMarkerWrapper.GetWrappedAs<WaypointMarker>().waypointMarkerComponent.FollowScreenPosition(following, screenPositionToFollow);
        }

        public Vector3 myPosition { get; private set; }
        
        private WaypointMarkerComponent waypointMarkerComponent;

        public WaypointMarker(Vector3 position)
        {
            this.myPosition = position;
            
            _myWrapper = new MapMarkerWrapper<WaypointMarker>(this, () => {_myWrapper = null;});
            GetMyWrapper<WaypointMarker>().SubscribeOnClearance(DestroyMarkerComponent);

            waypointMarkerComponent = MonoBehaviour.Instantiate<WaypointMarkerComponent>(
                GameObject.Find("ResourcesList").GetComponent<ResourcesListComponent>()
                .waypointMarkerComponentPrefab, GameObject.Find("WorldUICanvas").transform);
            waypointMarkerComponent.transform.position = position;
            waypointMarkerComponent.associatedMarkerWrapper = GetMyWrapper2<MapMarkerWrapper<WaypointMarker>>();
        }

        public override void UpdateMe()
        {
            myPosition = waypointMarkerComponent.transform.position;
        }

        private void DestroyMarkerComponent()
        {
            MonoBehaviour.Destroy(waypointMarkerComponent.gameObject);
            //GetMyWrapper<WaypointMarker>().UnsubscribeOnClearance(DestroyMarkerTransform);
            //GetMyWrapper<WaypointMarker>().UnsubscribeOnClearanceAll();
        }
    }
}
