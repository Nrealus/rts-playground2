using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.MapMarkers
{
    public class FirePositionMarker : MapMarker
    {

        /*public static void UpdateWaypointMarker(MapMarkerWrapper<FirePositionMarker> firePositionMarkerWrapper, bool following, Vector3 screenPositionToFollow)
        {
            if(firePositionMarkerWrapper.WrappedObject != null)
                firePositionMarkerWrapper.GetWrappedAs<FirePositionMarker>().firePositionMarkerComponent.FollowScreenPosition(following, screenPositionToFollow);
        }*/

        public Vector3 myPosition { get; private set; }
        public float myRadius { get; private set; }

        private FirePositionMarkerComponent firePositionMarkerComponent;

        public FirePositionMarker(Vector3 position, float radius)
        {
            this.myPosition = position;
            this.myRadius = radius;
            
            _myWrapper = new MapMarkerWrapper<FirePositionMarker>(this, () => {_myWrapper = null;});
            GetMyWrapper<FirePositionMarker>().SubscribeOnClearance(DestroyMarkerComponent);

            firePositionMarkerComponent = MonoBehaviour.Instantiate<FirePositionMarkerComponent>(
                GameObject.Find("ResourcesList").GetComponent<ResourcesListComponent>()
                .firePositionMarkerComponentPrefab, GameObject.Find("WorldUICanvas").transform);
            firePositionMarkerComponent.associatedMarkerWrapper = GetMyWrapper2<MapMarkerWrapper<FirePositionMarker>>();

            firePositionMarkerComponent.transform.position = this.myPosition;
            firePositionMarkerComponent.radius = this.myRadius;
        }

        public override void UpdateMe()
        {
            myPosition = firePositionMarkerComponent.transform.position;
            myRadius = firePositionMarkerComponent.radius;
            //Debug.Log(myRadius);
        }

        private void DestroyMarkerComponent()
        {
            MonoBehaviour.Destroy(firePositionMarkerComponent.gameObject);
            //GetMyWrapper<WaypointMarker>().UnsubscribeOnClearance(DestroyMarkerTransform);
            //GetMyWrapper<WaypointMarker>().UnsubscribeOnClearanceAll();
        }
    }
}
