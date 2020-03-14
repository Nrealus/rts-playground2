using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.MapMarkers
{
    public class OrderMarkerTransform : MonoBehaviour
    {

        public MapMarkerWrapper associatedMarkerWrapper;        
            
        private void Start()
        {
            /*transform.GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Button>()
                .onClick.AddListener(() => {
                    associatedMarkerWrapper.GetWrappedAs<OrderMarker>().ordWrapper
                        .TryStartExecution();
                });*/
        }

        private void Update() 
        {
            associatedMarkerWrapper.WrappedObject.UpdateMe();
        }

    }   
}