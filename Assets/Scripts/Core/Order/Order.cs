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
        IHasRefWrapperAndTree<OrderWrapper>
    {
        /*public static NPBehave.Clock GetAllOrdersClock()
        {
            return GameManager.Instance.currentMainHandler.orderHandler.ordersBTClock;
        }*/

        //protected OrderOptions orderOptions;

        public enum OrderPhase
        { Preparation, RequestConfirmation, AllGoodToStartExecution, NotReadyToStartExecution,
            Execution, Pause, Cancelled, End, Disposed }
        protected StateMachine<OrderPhase> orderPhasesFSM;

        /*--------*/

        protected OrderTreeNode orderTreeNode;

        /*--------*/

        protected OrderWrapper _myWrapper;
        public OrderWrapper GetMyWrapper() { return _myWrapper; }
        public OrderWrapper<T> GetMyWrapper<T>() where T : Order { return _myWrapper as OrderWrapper<T>; }

        public void ClearWrapper()
        {
            GetMyWrapper().DestroyWrappedReference();
            _myWrapper = null;
        }

        /*--------*/

        public Order()
        {
            //BaseConstructor(); <-- NO : BECAUSE C# CALLS CONSTRUCTORS "FROM TOP TO BOTTOM" (base then derived)
        }

        protected void BaseConstructor()
        {
            orderTreeNode = new OrderTreeNode(GetMyWrapper());
            orderPhasesFSM = new StateMachine<OrderPhase>();
            OrderPhasesFSMInit();
        }

        protected abstract void OrderPhasesFSMInit();

        public abstract bool IsOrderApplicable();

        public abstract IOrderable GetOrderReceiver();

        public abstract void SetOrderReceiver(IOrderable orderable);

        public bool ReceiverExists()
        {
            return GetOrderReceiver() != null && GetOrderReceiver().AmIStillUsed();
        }

        public void SetPhase(OrderPhase phase)
        {
            orderPhasesFSM.CurrentState = phase;
        }

        public void SetMeAndAllChildrenPhase(OrderPhase phase)
        {
            var list = GetMeAndAllChildrenWrappersList();
            foreach (OrderWrapper v in list)
            {
                v.WrappedObject.SetPhase(phase);
            }
        }

        public bool IsInPhase(OrderPhase phase)
        {
            return orderPhasesFSM.CurrentState == phase;
        }

        public bool AreMeAndAllChildrenInPhase(OrderPhase phase)
        {
            var list = GetMeAndAllChildrenWrappersList();
            var b = false;
            foreach (OrderWrapper v in list)
            {
                b = true;
                if(!v.WrappedObject.IsInPhase(phase))
                {
                    return false;
                }
            }
            return b;
        }

        public void UpdateOrderPhaseFSM()
        {
            orderPhasesFSM.Update();
        }

        public List<OrderWrapper> GetMeAndAllChildrenWrappersList()
        {
            return orderTreeNode.BFSListMeAndAllChildrenWrappers();
        }

        public List<OrderWrapper<T>> GetMeAndAllChildrenWrappersListOfType<T>() where T : Order
        {
            var bfslist = orderTreeNode.BFSListMeAndAllChildrenWrappers();
            List<OrderWrapper<T>> res = new List<OrderWrapper<T>>();
            foreach (var v in bfslist)
            {
                if(v is OrderWrapper<T>)
                    res.Add(v as OrderWrapper<T>);
            }
            return res;
        }

        public OrderWrapper GetParentWrapper()
        {
            return orderTreeNode.GetParentWrapper();
        }

        public bool IsRoot()
        {
            return orderTreeNode.IsRoot();
        }

        public void SetParentOrder(Order order)
        {
            orderTreeNode.ChangeParentTo(order.orderTreeNode);
        }

        //public abstract void SetOptions(OrderOptions options);
        
    }
}