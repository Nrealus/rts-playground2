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
            if (WrappedObject.IsInPhase(Order.OrderPhase.Preparation))
            {
                WrappedObject.SetOrderReceiver(orderedUnitWrapper);
            }
            else
            {
                Debug.Log("Order is not in setup phase anymore");
            }
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
            }
            else
            {
                Debug.LogError("order was already registered");
            }

            if (WrappedObject.IsRoot())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool UnregisterMe()
        {
            if (!GetOrderHandler().OrderWrapperRegistered(this))
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

        public bool TryStartIndividualExecution()
        {
            if(WrappedObject.IsInPhase(Order.OrderPhase.AllGoodToStartExecution))
            {
                WrappedObject.SetMeAndAllChildrenPhase(Order.OrderPhase.Execution);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryStartExecution(bool individualIndependently)
        {
            if (!individualIndependently)
            {
                Debug.Log(WrappedObject.GetMeAndAllChildrenWrappersList().Count);
                if (WrappedObject.AreMeAndAllChildrenInPhase(Order.OrderPhase.AllGoodToStartExecution))
                {
                    WrappedObject.SetMeAndAllChildrenPhase(Order.OrderPhase.Execution);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                var list = WrappedObject.GetMeAndAllChildrenWrappersList();
                bool b = true;
                foreach (OrderWrapper v in list)
                {
                    b = b && TryStartIndividualExecution();
                }
                return b; // if all could start ("individually" though (= even when another order is not ready))
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

        public bool PauseMeAndAllChildrenExecution()
        {
            if (WrappedObject.AreMeAndAllChildrenInPhase(Order.OrderPhase.Execution))
            {
                WrappedObject.SetMeAndAllChildrenPhase(Order.OrderPhase.Pause);
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

        public bool UnpauseMeAndAllChildrenExecution()
        {
            if (WrappedObject.AreMeAndAllChildrenInPhase(Order.OrderPhase.Pause))
            {
                WrappedObject.SetMeAndAllChildrenPhase(Order.OrderPhase.Execution);
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

        public bool CancelMeAndAllChildrenOrder()
        {
            WrappedObject.SetMeAndAllChildrenPhase(Order.OrderPhase.Cancelled);
            return true;
        }

        public void EndExecution()
        {
            CancelOrder();
        }

        
        public void SetOrderReceiver(IOrderable orderedUnitWrapper)
        {
            if (WrappedObject.IsInPhase(Order.OrderPhase.Preparation))
            {
                WrappedObject.SetOrderReceiver(orderedUnitWrapper);
            }
            else
            {
                Debug.Log("Order is not in setup phase anymore");
            }
        }
        
        public IOrderable GetOrderReceiver()
        {
            return WrappedObject.GetOrderReceiver();
        }

    }

}