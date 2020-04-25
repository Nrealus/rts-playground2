using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Helpers;
using VariousUtilsExtensions;

namespace Core.MapMarkers
{
    public class FirePositionMarkerComponent : MapMarkerComponent, IHasCameraRef
    {

        public float moveSpeed = 0.5f;
        public float offset = 5f;
        public bool following;
        public MapMarkerWrapper<FirePositionMarker> associatedMarkerWrapper;        

        //public Vector3 screenPositionToFollow;
        private float _radius = 1;
        public float radius
        {
            get {
                return _radius;
            }

            set {
                _radius = value;
                transform.localScale = _radius * new Vector3(1,1,1);
            }
        }

        private static Camera _cam;
        public Camera GetMyCamera()
        {
            if(_cam == null)
                //_cam = GameObjectExtension.FindObjectOfTypeAndLayer<Camera>(LayerMask.NameToLayer("UI"));
                _cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

            return _cam;
        }

        private void Start()
        {
            following = false;
        }

        private Vector3 sp, wp;
        private void Update() 
        {
            associatedMarkerWrapper.WrappedObject.UpdateMe();
            //if (GetMyCamera().GetWorldPosCloseToScreenPos(transform.position, Input.mousePosition, offset))
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

        /*internal void FollowScreenPosition(bool following, Vector3 screenPositionToFollow)
        {
            this.following = following;
            if (following)
            {
                transform.position = Vector3.Lerp(transform.position, 
                    GetMyCamera().GetPointedPositionPhysRaycast(screenPositionToFollow), moveSpeed);
                //transform.position.Set(wp.x, wp.y, wp.z);
            }
        }*/
    }   
}