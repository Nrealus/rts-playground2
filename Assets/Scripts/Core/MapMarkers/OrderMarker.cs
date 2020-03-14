using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Orders;
using Core.Units;

namespace Core.MapMarkers
{
    public class OrderMarker : MapMarker
    {

        public OrderWrapper ordWrapper;
        
        private OrderMarkerTransform orderMarkerTransform;        

        private UnitWrapper uw;

        public OrderMarker(OrderWrapper ordWrapper)
        {
            this.ordWrapper = ordWrapper;

            _myWrapper = new MapMarkerWrapper<OrderMarker>(this, () => {_myWrapper = null;});
            GetMyWrapper<OrderMarker>().SubscribeOnClearance(DestroyMarkerTransform);

            orderMarkerTransform = MonoBehaviour.Instantiate<OrderMarkerTransform>(
                GameObject.Find("ResourcesList").GetComponent<ResourcesListComponent>()
                .orderMarkerTransformPrefab, GameObject.Find("Canvas").transform);
            orderMarkerTransform.associatedMarkerWrapper = GetMyWrapper<OrderMarker>();
            
            uw = ((UnitWrapper)ordWrapper.GetOrderReceiver());

        }

        public override void UpdateMe()
        {
            if(uw != null && uw.WrappedObject != null)
                orderMarkerTransform.transform.position = uw.WrappedObject.transform.position;
            //else
            //    GetMyWrapper<OrderMarker>().WrappedObject.ClearWrapper();
            if(Input.GetKeyDown(KeyCode.T))
            {
                ordWrapper.EndExecution();
            }
        }

        private void DestroyMarkerTransform()
        {
            MonoBehaviour.Destroy(orderMarkerTransform.gameObject);
            //GetMyWrapper<WaypointMarker>().UnsubscribeOnClearance(DestroyMarkerTransform);
            //GetMyWrapper<OrderMarker>().UnsubscribeOnClearanceAll();
        }
    }
}
