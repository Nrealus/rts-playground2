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

    public class OrderGroupWrapper<T> : OrderGroupWrapper where T : Order
    {
        public T ConvertWrappedToT()
        {
            return WrappedObject as T;
        }

        public OrderGroupWrapper(OrderGroup wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {

        }
        
    }

    public class OrderGroupWrapper : ReferenceWrapper<OrderGroup>
    {

        private static OrderHandler GetOrderHandler()
        {
            return GameManager.Instance.currentMainHandler.orderHandler;
        }


        public OrderGroupWrapper(OrderGroup wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
            SubscribeOnClearance(() => {
                int c = orderWrappersList.Count;
                for(int i = c-1; i>=0; i--)
                {
                    orderWrappersList[i].DestroyWrappedReference();
                };
                orderWrappersList = null;
            });
        }       


        private List<OrderGroupWrapper> orderGroupWrappersList = new List<OrderGroupWrapper>();

        public OrderGroupWrapper[] GetChildOrderGroupWrappers()
        {
            return orderGroupWrappersList.ToArray();
        }

        public void AddChildOrderGroupWrapper(OrderGroupWrapper ogw)
        {
            orderGroupWrappersList.Add(ogw);
        }

        private List<OrderWrapper> orderWrappersList = new List<OrderWrapper>();

        public OrderWrapper[] GetChildOrderWrappers()
        {
            return orderWrappersList.ToArray();
        }

        public void AddChildOrderWrapper(OrderWrapper ow)
        {
            if(!orderWrappersList.Contains(ow))
            {
                orderWrappersList.Add(ow);
                ow.SubscribeOnClearance(() => RemoveChildOrderWrapper(ow));
            }
        }

        public void AddChildOrderWrappers(List<OrderWrapper> ows)
        {
            foreach (var ow in ows)
            {
                AddChildOrderWrapper(ow);
            }
        }

        private void RemoveChildOrderWrapper(OrderWrapper ow)
        {
            if(orderWrappersList.Contains(ow))
            {
                orderWrappersList.Remove(ow);
                //ow.UnsubscribeOnClearance(() => RemoveChildOrderWrapper(ow));
                // if set again to public, it will mean that there can be removal without destruction,
                // which obligates us to unsubscribe this specific handler in that situation
            }
        }

        public void SetOrderGroupMods(OrderParams orderParams)
        {

        }

        /*public bool GetConfirmationFromReceiver()
        {
            bool b = false;
            foreach (var ow in orderWrappersList)
            {
                b &= true && Order.GetConfirmationFromReceiver(ow);
            }
            return b;
        }*/

        public bool TryStartExecution()
        {
            bool b = false;
            foreach (var ow in orderWrappersList)
            {
                b &= true && Order.TryStartExecution(ow);
            }
            return b;
        }
        /*
        public bool PauseExecution()
        {
            if (WrappedObject.IsInPhase(Order.OrderPhase.Execution))
            {
                WrappedObject.SetPhase(Order.OrderPhase.Pause);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UnpauseExecution()
        {
            if (WrappedObject.IsInPhase(Order.OrderPhase.Pause))
            {
                WrappedObject.SetPhase(Order.OrderPhase.Execution);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CancelOrder()
        {
            WrappedObject.SetPhase(Order.OrderPhase.Cancelled);
            return true;
        }

        public bool EndExecution()
        {
            WrappedObject.SetPhase(Order.OrderPhase.End);
            return true;
        }
        */
        

    }

}
