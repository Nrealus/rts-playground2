using System.Collections;
using System.Collections.Generic;
using Core.Helpers;
using UnityEngine;
using Nrealus.Extensions;

namespace Core.MapMarkers
{
    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    /// <summary>
    /// A MapMarker subclass, currently used as a waypoint for MoveTasks. They are manually entered on the map by the player,
    /// or automatically created by the game logic at some position. They can also be dragged on the screen (map) with a cursor (mouse)
    /// </summary>   
    public class WaypointMarker : MapMarker, IHasRefWrapper<MapMarkerWrapper<WaypointMarker>>
    {

        public new MapMarkerWrapper<WaypointMarker> GetRefWrapper()
        {
            return _myWrapper as MapMarkerWrapper<WaypointMarker>;
        }

        private static Camera _cam;
        public Camera GetMyCamera()
        {
            if(_cam == null)
                _cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

            return _cam;
        }

        public static WaypointMarker CreateInstance(Vector3 position)
        {
            WaypointMarker res = Instantiate<WaypointMarker>(
                GameObject.Find("ResourcesList").GetComponent<ResourcesListComponent>().waypointMarkerPrefab,
                GameObject.Find("UI World Canvas").transform);
            
            res.Init(position);
            
            return res;
        }

        public static void UpdateWaypointMarker(MapMarkerWrapper<WaypointMarker> waypointMarkerWrapper, bool following, Vector3 screenPositionToFollow)
        {
            if(waypointMarkerWrapper.GetWrappedReference() != null)
                waypointMarkerWrapper.GetWrappedReference().FollowScreenPosition(following, screenPositionToFollow);
        }
        
        public float moveSpeed = 0.5f;
        public float offset = 5f;
        public bool following;

        
        private void Init(Vector3 position)
        {
            transform.position = position;

            _myWrapper = new MapMarkerWrapper<WaypointMarker>(this, () => {_myWrapper = null;});
            GetRefWrapper().SubscribeOnClearance(DestroyMe);

            following = false;
        }

        private Vector3 sp, wp;
        private void Update() 
        {

            if (GetMyCamera().GetWorldPosCloseToScreenPos(transform.position, Input.mousePosition, offset))
            {
                /*if(Input.GetMouseButtonDown(0))
                {
                    if (following)
                    {
                        following = false;
                    }
                    else
                    {
                        following = true;
                    }
                }
                if(Input.GetMouseButtonDown(1))
                {
                    associatedMarkerWrapper.DestroyWrappedReference();
                }*/
            }
        }

        private void FollowScreenPosition(bool following, Vector3 screenPositionToFollow)
        {
            this.following = following;
            if (following)
            {
                transform.position = Vector3.Lerp(transform.position, 
                    GetMyCamera().GetPointedPositionPhysRaycast(screenPositionToFollow), moveSpeed);
                //transform.position.Set(wp.x, wp.y, wp.z);
            }
        }

        private void DestroyMe()
        {
            Destroy(gameObject);
            //GetMyWrapper<WaypointMarker>().UnsubscribeOnClearance(DestroyMarkerTransform);
            //GetMyWrapper<WaypointMarker>().UnsubscribeOnClearanceAll();
        }
    }
}
