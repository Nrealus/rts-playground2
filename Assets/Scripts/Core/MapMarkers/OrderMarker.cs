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
        
        private OrderMarkerComponent orderMarkerComponent;        

        private UnitWrapper uw;

        public OrderMarker(OrderWrapper ordWrapper)
        {
            this.ordWrapper = ordWrapper;
            ordWrapper.SubscribeOnClearance(() => DestroyMarkerTransform());

            _myWrapper = new MapMarkerWrapper<OrderMarker>(this, () => {_myWrapper = null;});
            GetMyWrapper<OrderMarker>().SubscribeOnClearance(DestroyMarkerTransform);

            uw = (UnitWrapper)Order.GetReceiver(ordWrapper);
            orderMarkerComponent = MonoBehaviour.Instantiate<OrderMarkerComponent>(
                GameObject.Find("ResourcesList").GetComponent<ResourcesListComponent>().orderMarkerComponentPrefab,
                GameObject.Find("WorldUICanvas").transform);
            orderMarkerComponent.associatedMarkerWrapper = GetMyWrapper<OrderMarker>();
            orderMarkerComponent.transform.position = uw.WrappedObject.transform.position;
            
        }

        public override void UpdateMe()
        {
            if(uw != null && uw.WrappedObject != null)
                orderMarkerComponent.transform.position = uw.WrappedObject.transform.position;
            //else
            //    GetMyWrapper<OrderMarker>().WrappedObject.ClearWrapper();
            /*if(Input.GetKeyDown(KeyCode.T))
            {
                Order.EndExecution(ordWrapper);
            }*/
        }

        private void DestroyMarkerTransform()
        {
            MonoBehaviour.Destroy(orderMarkerComponent.gameObject);
            //GetMyWrapper<WaypointMarker>().UnsubscribeOnClearance(DestroyMarkerTransform);
            //GetMyWrapper<OrderMarker>().UnsubscribeOnClearanceAll();
        }
    }
}
