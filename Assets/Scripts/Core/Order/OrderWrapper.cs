using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;
using Core.Handlers;
using GlobalManagers;
using System;

namespace Core.Orders
{

    public class OrderWrapper<T> : OrderWrapper where T : Order
    {
        public OrderWrapper(Order wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
        }
        
    }

    public abstract class OrderWrapper : ReferenceWrapper<Order>
    {
        private static OrderHandler GetOrderHandler()
        {
            return GameManager.Instance.currentMainHandler.orderHandler;
        }
    
        public OrderWrapper(Order wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
        }        

        public T GetWrappedAs<T>() where T : Order
        {
            return WrappedObject as T;
        }

        /*public bool CancelOrder()
        {
            WrappedObject.SetPhase(Order.OrderPhase.Cancelled);
            return true;
        }*/

        /*
        public IOrderable GetOrderReceiver()
        {
            return WrappedObject.GetOrderReceiver();
        }*/
    }

}