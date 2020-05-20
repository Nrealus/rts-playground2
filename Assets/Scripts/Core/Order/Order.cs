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

    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/
    
    /// <summary>
    /// The base class for all orders. It works with IOrderable "receivers" of orders, to which the order is given.
    /// It provides the structure for orders, mostly in the form of protected abstract or virtual methods to be implemented by "concrete" subclasses.
    /// It also provides static methods and functions which call the appropriate instance methods or functions, given a IOrderable as a parameter.
    /// This allows to limit accessing .WrappedObject for common things, and has shown to be a nice approach improving clarity and encapsulation aswell as decoupling.   
    /// An Order subclass should be instanciated from the static class OrderFactory.
    /// With time, some things that may become very common subclasses may be bundled into an "intermediate" abstract subclass, or even into this one.
    /// </summary>      
    public abstract class Order :
        IHasRefToRefWrapper<OrderWrapper>
    {
        
        #region Static functions

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

        public static IOrderable GetReceiver(OrderWrapper orderWrapper)
        {
            return orderWrapper.WrappedObject.InstanceGetOrderReceiver();
        }

        public static Vector3 GetReceiverWorldPosition(OrderWrapper orderWrapper)
        {
            if (GetReceiver(orderWrapper) is UnitGroupWrapper)
                return ((UnitGroupWrapper)GetReceiver(orderWrapper)).WrappedObject.GetPosition();
            else
                return Vector3.zero;
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
            return GetReceiver(orderWrapper) != null && GetReceiver(orderWrapper).IsWrappedObjectNotNull();
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