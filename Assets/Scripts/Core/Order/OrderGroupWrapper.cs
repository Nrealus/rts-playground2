using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;
using Core.Handlers;
using GlobalManagers;

namespace Core.Orders
{

    public class OrderGroupWrapper<T> : OrderGroupWrapper where T : Order
    {
        public T ConvertWrappedToT()
        {
            return WrappedObject as T;
        }

        public OrderGroupWrapper(OrderGroup wrappedObject) : base(wrappedObject)
        {

        }
        
    }

    public class OrderGroupWrapper : ReferenceWrapper<OrderGroup>
    {

        private static OrderHandler GetOrderHandler()
        {
            return GameManager.Instance.currentMainHandler.orderHandler;
        }


        public OrderGroupWrapper(OrderGroup wrappedObject) : base(wrappedObject)
        {

        }       


        private List<OrderGroupWrapper> orderGroupWrappersList = new List<OrderGroupWrapper>();

        public List<OrderGroupWrapper> GetChildOrderGroupWrappers()
        {
            return new List<OrderGroupWrapper>(orderGroupWrappersList);
        }

        public void AddChildOrderGroupWrapper(OrderGroupWrapper ogw)
        {
            orderGroupWrappersList.Add(ogw);
        }

        private List<OrderWrapper> orderWrappersList = new List<OrderWrapper>();

        public List<OrderWrapper> GetChildOrderWrappers()
        {
            return new List<OrderWrapper>(orderWrappersList);
        }

        public void AddChildOrderWrapper(OrderWrapper ow)
        {
            orderWrappersList.Add(ow);
        }

    }

}