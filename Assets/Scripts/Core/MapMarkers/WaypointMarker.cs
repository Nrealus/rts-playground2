using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.MapMarkers
{
    public class WaypointMarker : MapMarker
    {

        internal void UpdateWaypointMarker(bool following, Vector3 screenPositionToFollow)
        {
            waypointMarkerTransform.FollowScreenPosition(following, screenPositionToFollow); 
        }


        public Vector3 myPosition;
        
        private WaypointMarkerComponent waypointMarkerTransform;

        public WaypointMarker(Vector3 position)
        {
            this.myPosition = position;
            
            _myWrapper = new WaypointMarkerWrapper(this, () => {_myWrapper = null;});
            GetMyWrapper<WaypointMarker>().SubscribeOnClearance(DestroyMarkerTransform);

            waypointMarkerTransform = MonoBehaviour.Instantiate<WaypointMarkerComponent>(
                GameObject.Find("ResourcesList").GetComponent<ResourcesListComponent>()
                .waypointMarkerTransformPrefab, GameObject.Find("WorldUICanvas").transform);
            waypointMarkerTransform.transform.position = position;
            waypointMarkerTransform.associatedMarkerWrapper = GetMyWrapper2<WaypointMarkerWrapper>();
        }

        public override void UpdateMe()
        {
            myPosition = waypointMarkerTransform.transform.position;
        }

        private void DestroyMarkerTransform()
        {
            MonoBehaviour.Destroy(waypointMarkerTransform.gameObject);
            //GetMyWrapper<WaypointMarker>().UnsubscribeOnClearance(DestroyMarkerTransform);
            //GetMyWrapper<WaypointMarker>().UnsubscribeOnClearanceAll();
        }
    }
}
