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

    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    /// <summary>
    /// Subclass of OrderWrapper
    /// </summary>
    /// <typeparamref name="T">Specific type of wrapped Order</typeparamref>
    public class OrderWrapper<T> : OrderWrapper where T : Order
    {
        public OrderWrapper(Order wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
        }
        
    }
        
    /// <summary>
    /// The RefWrapper for Order.
    /// </summary>      
    public abstract class OrderWrapper : RefWrapper<Order>
    {
    
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