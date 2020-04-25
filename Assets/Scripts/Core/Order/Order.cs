using Core.Handlers;
using Core.Helpers;
using Core.Units;
using Gamelogic.Extensions;
using GlobalManagers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Orders
{

    public abstract class Order :
        IHasRefWrapper<OrderWrapper>
    {
        
        #region Static functions

        /*protected static bool RegisterAtOrderHandlerAndReceiver(OrderWrapper orderWrapper, OrderWrapper predecessor, OrderWrapper successor)
        {
            if (!OrderHandler.IsOrderWrapperRegistered(orderWrapper))
            {
                Order.GetReceiver(orderWrapper).AddOrderToList(orderWrapper, predecessor, successor);
                OrderHandler.AddToOrderWrapperList(orderWrapper);
                orderWrapper.SubscribeOnClearance(() => Order.Unregister(orderWrapper));
                return true;
            }
            elseorderWrapper
            {
                Debug.LogError("order was already registered");
                return false;
            }

        }

        private static bool Unregister(OrderWrapper orderWrapper)
        {
            if (OrderHandler.IsOrderWrapperRegistered(orderWrapper))
            {
                GetReceiver(orderWrapper).RemoveOrderFromList(orderWrapper);
                OrderHandler.RemoveFromOrderWrapperList(orderWrapper);
                //.RemoveFromOrderWrapperList(torderWrapperhis);
                orderWrapper.UnsubscribeOnClearance(() => Order.Unregister(orderWrapper)); 
                return true;
            }
            else
            {
                Debug.LogError("order is not registered");
                return false;
            }
        }

        private static bool GetConfirmationFromReceiver(OrderWrapper orderWrapper)
        {
            if (orderWrapper.WrappedObject != null
                && (Order.IsInPhase(orderWrapper, Order.OrderPhase.Registration)
                || Order.IsInPhase(orderWrapper, Order.OrderPhase.NotReadyForExecution)))
            {
                Order.SetPhase(orderWrapper, Order.OrderPhase.RequestConfirmation);
                return true;
            }
            else
            {
                return false;
            }
        }*/

        public static bool TryStartExecution(OrderWrapper orderWrapper)
        {
            return orderWrapper.WrappedObject.InstanceTryStartExecution();
        }

        public static bool PauseExecution(OrderWrapper orderWrapper)
        {
            if (Order.IsInPhase(orderWrapper, Order.OrderPhase.Execution))
            {
                Order.SetPhase(orderWrapper, Order.OrderPhase.Pause);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool UnpauseExecution(OrderWrapper orderWrapper)
        {
            if (Order.IsInPhase(orderWrapper, Order.OrderPhase.Pause))
            {
                Order.SetPhase(orderWrapper, Order.OrderPhase.Execution);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool EndExecution(OrderWrapper orderWrapper)
        {
            Order.SetPhase(orderWrapper, Order.OrderPhase.End);
            return true;
        }

        public static bool EndExecution(IEnumerable<OrderWrapper> orderWrappers)
        {
            foreach (var ow in orderWrappers)
                EndExecution(ow);
            return true;
        }

        /*public static bool IsReadyToStartExecution(OrderWrapper orderWrapper, OrderExecutionMode mode)
        {
            return orderWrapper.WrappedObject.InstanceIsReadyToStartExecution(mode);
        }*/

        public static IOrderable GetReceiver(OrderWrapper orderWrapper)
        {
            return orderWrapper.WrappedObject.InstanceGetOrderReceiver();
        }

        public static void SetReceiver(OrderWrapper orderWrapper, OrderWrapper predecessor, OrderWrapper successor, IOrderable orderable)
        {
            // unsubscribe this if SetOrderReceiver for another orderable afterwards, potentially ?
            orderWrapper.WrappedObject.InstanceSetOrderReceiver(orderable, predecessor, successor);
        }

        public static OrderParams GetParameters(OrderWrapper orderWrapper)
        {
            return orderWrapper.WrappedObject.InstanceGetOrderParams();
        }

        /*public static void SetParameters(OrderWrapper orderWrapper, OrderParams orderParams)
        {
            orderWrapper.WrappedObject.InstanceSetOrderParams(orderParams);
        }*/

        public static bool ReceiverExists(OrderWrapper orderWrapper)
        {
            return GetReceiver(orderWrapper) != null && GetReceiver(orderWrapper).AmIStillUsed();
        }

        public static void SetPhase(OrderWrapper orderWrapper, OrderPhase phase)
        {
            orderWrapper.WrappedObject.InstanceSetPhase(phase);
        }

        public static bool IsInPhase(OrderWrapper orderWrapper, OrderPhase phase)
        {
            return orderWrapper.WrappedObject.InstanceIsInPhase(phase);
        }
        
        public static void UpdateFSM(OrderWrapper orderWrapper)
        {
            if (orderWrapper != null && orderWrapper.WrappedObject != null)
                orderWrapper.WrappedObject.InstanceUpdateFSM();
        }

        #endregion

        #region Variables, properties etc.

        public enum OrderPhase
        {   Initial,
            /*Registration,*/ Staging,// ReadyForExecution, NotReadyForExecution,
            ExecutionWaitingTimeToStart, Execution, Pause, Cancelled, End, End2, Disposed,  
        }
        protected StateMachine<OrderPhase> orderPhasesFSM;

        /*--------*/

        protected OrderWrapper _myWrapper;
        public OrderWrapper GetMyWrapper() { return _myWrapper; }
        public OrderWrapper<T> GetMyWrapper<T>() where T : Order { return _myWrapper as OrderWrapper<T>; }

        /*--------*/

        #endregion

        public Order()
        {
            //BaseConstructor(); <-- NO : BECAUSE C# CALLS CONSTRUCTORS "FROM TOP TO BOTTOM" (base then derived)
        }

        #region Protected/Private abstract instance methods

        protected abstract OrderParams InstanceGetOrderParams();

        //protected abstract void InstanceSetOrderParams(OrderParams orderParams);

        protected abstract IOrderable InstanceGetOrderReceiver();
        
        //public abstract T GetOrderReceiverAsT<T>() where T : IOrderable;

        protected abstract void InstanceSetOrderReceiver(IOrderable orderable, OrderWrapper predecessor, OrderWrapper successor);

        protected abstract void OrderPhasesFSMInit();

        //protected abstract bool InstanceIsReadyToStartExecution(OrderExecutionMode mode);

        protected abstract bool InstanceTryStartExecution();
        
        #endregion

        #region Protected/Private instance methods

        protected void InstanceSetPhase(OrderPhase phase)
        {
            orderPhasesFSM.CurrentState = phase;
        }

        protected bool InstanceIsInPhase(OrderPhase phase)
        {
            return orderPhasesFSM.CurrentState == phase;
        }

        private void InstanceUpdateFSM()
        {
            orderPhasesFSM.Update();
        }

        protected void CreateAndInitFSM()
        {
            orderPhasesFSM = new StateMachine<OrderPhase>();
            OrderPhasesFSMInit();
        }

        //public abstract void SetOptions(OrderOptions options);

        #endregion
        
    }
}