using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.MapMarkers
{
    public class WaypointMarker : MapMarker
    {

        public Vector3 vectorPosition;
        
        private WaypointMarkerTransform waypointMarkerTransform;

        public WaypointMarker(Vector3 position)
        {
            this.vectorPosition = position;
            
            _myWrapper = new MapMarkerWrapper<WaypointMarker>(this, () => {_myWrapper = null;});
            GetMyWrapper<WaypointMarker>().SubscribeOnClearance(DestroyMarkerTransform);

            waypointMarkerTransform = MonoBehaviour.Instantiate<WaypointMarkerTransform>(
                GameObject.Find("ResourcesList").GetComponent<ResourcesListComponent>()
                .waypointMarkerTransformPrefab, GameObject.Find("WorldUICanvas").transform);
            waypointMarkerTransform.transform.position = position;
            waypointMarkerTransform.associatedMarkerWrapper = GetMyWrapper<WaypointMarker>();
        }

        public override void UpdateMe()
        {
            vectorPosition = waypointMarkerTransform.transform.position;
        }

        private void DestroyMarkerTransform()
        {
            MonoBehaviour.Destroy(waypointMarkerTransform.gameObject);
            //GetMyWrapper<WaypointMarker>().UnsubscribeOnClearance(DestroyMarkerTransform);
            //GetMyWrapper<WaypointMarker>().UnsubscribeOnClearanceAll();
        }
    }
}
