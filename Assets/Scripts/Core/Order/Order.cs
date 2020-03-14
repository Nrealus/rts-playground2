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
        /*public static NPBehave.Clock GetAllOrdersClock()
        {
            return GameManager.Instance.currentMainHandler.orderHandler.ordersBTClock;
        }*/

        //protected OrderOptions orderOptions;

        public enum OrderPhase
        { InitialState,
            Registration, RequestConfirmation, AllGoodToStartExecution, NotReadyToStartExecution,
            Execution, Pause, Cancelled, End, Disposed }
        protected StateMachine<OrderPhase> orderPhasesFSM;

        /*--------*/

        protected OrderWrapper _myWrapper;
        public OrderWrapper GetMyWrapper() { return _myWrapper; }
        public OrderWrapper<T> GetMyWrapper<T>() where T : Order { return _myWrapper as OrderWrapper<T>; }

        //public abstract void ClearWrapper();

        /*--------*/

        public Order()
        {
            //BaseConstructor(); <-- NO : BECAUSE C# CALLS CONSTRUCTORS "FROM TOP TO BOTTOM" (base then derived)
        }

        protected void CreateAndInitFSM()
        {
            orderPhasesFSM = new StateMachine<OrderPhase>();
            OrderPhasesFSMInit();
        }

        protected abstract void OrderPhasesFSMInit();

        public abstract bool IsOrderApplicable();

        public abstract IOrderable GetOrderReceiver();
        
        //public abstract T GetOrderReceiverAsT<T>() where T : IOrderable;

        public abstract void SetOrderReceiver(IOrderable orderable);

        public bool ReceiverExists()
        {
            return GetOrderReceiver() != null && GetOrderReceiver().AmIStillUsed();
        }

        public void SetPhase(OrderPhase phase)
        {
            orderPhasesFSM.CurrentState = phase;
        }

        public bool IsInPhase(OrderPhase phase)
        {
            return orderPhasesFSM.CurrentState == phase;
        }

        public void UpdateOrderPhaseFSM()
        {
            orderPhasesFSM.Update();
        }

        //public abstract void SetOptions(OrderOptions options);
        
    }
}