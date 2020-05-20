using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Orders;
using Core.Units;

namespace Core.MapMarkers
{
    /****** Author : nrealus ****** Last documentation update : 14-05-2020 ******/

    /// <summary>
    /// A MapMarker subclass, used as "widgets" for Orders, or information windows about them.
    /// They are linked to an OrderWrapper. By default, they follow the position of the order's receiver.
    /// For a UnitGroup receiver, the default position is the mean position of all the units in the group.
    /// </summary>   
    public class OrderMarker : MapMarker
    {

        /*private static Camera _cam;
        public Camera GetMyCamera()
        {
            if(_cam == null)
                _cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

            return _cam;
        }*/

        public static OrderMarker CreateInstance(OrderWrapper orderWrapper)
        {
            OrderMarker res = Instantiate<OrderMarker>(
                GameObject.Find("ResourcesList").GetComponent<ResourcesListComponent>().orderMarkerPrefab,
                GameObject.Find("WorldUICanvas").transform);

            res.Init(orderWrapper);

            return res;
        }
        
        private OrderWrapper orderWrapper;

        private void Init(OrderWrapper orderWrapper)
        {

            this.orderWrapper = orderWrapper;
            orderWrapper.SubscribeOnClearance(DestroyMe);

            _myWrapper = new MapMarkerWrapper<OrderMarker>(this, () => {_myWrapper = null;});
            GetMyWrapper<OrderMarker>().SubscribeOnClearance(DestroyMe);

            transform.position = Order.GetReceiverWorldPosition(orderWrapper);
        }

        private void Update()
        {
            if(Order.GetReceiver(orderWrapper) != null)
            {
                transform.position = Order.GetReceiverWorldPosition(orderWrapper);
            }
            //else
            //    GetMyWrapper<OrderMarker>().WrappedObject.ClearWrapper();
            /*if(Input.GetKeyDown(KeyCode.T))
            {
                Order.EndExecution(ordWrapper);
            }*/
        }

        private void DestroyMe()
        {
            Destroy(gameObject);
        }
    }
}
