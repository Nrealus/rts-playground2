using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.MapMarkers
{
    public class WaypointMarkerTransform : MonoBehaviour
    {

        public float moveSpeed = 0.5f;
        public float offset = 5f;
        private bool following;

        public MapMarkerWrapper associatedMarkerWrapper;        
            
        private void Start()
        {
            following = false;
        }

        private Vector3 sp, wp;
        private void Update() 
        {
            associatedMarkerWrapper.WrappedObject.UpdateMe();

            sp = Camera.main.WorldToScreenPoint(transform.position);
            sp.z = 0;
            if ((sp-Input.mousePosition).magnitude <= offset)
            {
                if(Input.GetMouseButtonDown(0))
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
                }
            }

            if (following)
            {
                var mp = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y - transform.position.y);
                wp = Camera.main.ScreenToWorldPoint(mp);
                wp.y = transform.position.y;
                transform.position = Vector3.Lerp(transform.position, wp, moveSpeed);
                //transform.position.Set(wp.x, wp.y, wp.z);                
            }
        }

    }   
}