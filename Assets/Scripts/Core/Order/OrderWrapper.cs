using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;
using Core.Handlers;
using GlobalManagers;

namespace Core.Orders
{

    public class OrderWrapper<T> : OrderWrapper where T : Order
    {
        public T ConvertWrappedToT()
        {
            return WrappedObject as T;
        }

        public OrderWrapper(Order wrappedObject) : base(wrappedObject)
        {
        }

        public void SetOrderReceiver(IOrderable<T> orderedUnitWrapper)
        {
            SetOrderReceiver((IOrderable)orderedUnitWrapper);
        }
        
    }

    public abstract class OrderWrapper : ReferenceWrapper<Order>
    {
        private static OrderHandler GetOrderHandler()
        {
            return GameManager.Instance.currentMainHandler.orderHandler;
        }


        public OrderWrapper(Order wrappedObject) : base(wrappedObject)
        {
        }        

        public void UpdateOrderPhaseFSM()
        {
            WrappedObject.UpdateOrderPhaseFSM();
        }

        public bool RegisterMeIfAppropriate()
        {
            if (!GetOrderHandler().OrderWrapperRegistered(this))
            {
                GetOrderHandler().AddToOrderWrapperList(this);
                SubscribeOnClearance(() => UnregisterMe());
                return true;
            }
            else
            {
                Debug.LogError("order was already registered");
                return false;
            }

        }

        private bool UnregisterMe()
        {
            if (GetOrderHandler().OrderWrapperRegistered(this))
            {
                GetOrderHandler().RemoveFromOrderWrapperList(this);
                UnsubscribeOnClearance(() => UnregisterMe()); 
                return true;
            }
            else
            {
                Debug.LogError("order is not registered");
                return false;
            }
        }

        public bool GetConfirmationFromReceiver()
        {
            if (WrappedObject.IsInPhase(Order.OrderPhase.Preparation)
                || WrappedObject.IsInPhase(Order.OrderPhase.NotReadyToStartExecution))
            {
                WrappedObject.SetPhase(Order.OrderPhase.RequestConfirmation);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryStartExecution()
        {
            if(WrappedObject.IsInPhase(Order.OrderPhase.AllGoodToStartExecution))
            {
                WrappedObject.SetPhase(Order.OrderPhase.Execution);
                return true;
            }
            else
            {
                return false;
            }
        }

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

        
        public void SetOrderReceiver(IOrderable orderedUnitWrapper)
        {
            WrappedObject.SetOrderReceiver(orderedUnitWrapper);
        }
        /*
        public IOrderable GetOrderReceiver()
        {
            return WrappedObject.GetOrderReceiver();
        }
        */
    }

}