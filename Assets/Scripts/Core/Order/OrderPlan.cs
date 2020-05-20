using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.MapMarkers;
using UnityEngine;

namespace Core.Orders
{
    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    /// <summary>
    /// Instances of this class store a "plan" of Orders.
    /// Two types of them are maintained here : active and passive ones.
    /// Passive orders are supposed not to have a "distinct end", and multiple passive orders could be active executed in this plan at the same time.
    /// Active orders in this plan can only be executed one at a time.
    /// Each UnitGroup actually has an OrderPlan attached to it.
    /// This encapsulation of "plan logic" actually allows for some Orders to use "elementary" orders as "building blocks" in them, like the BuildOrder.
    /// The BuildOrder has a small private class which keeps track of all the IOrderables (i.e. basically units) in its receiver OrderGroup.
    /// And this small class creates a temporary auxiliary UnitGroup for each one of them. Using the OrderPlan of this auxiliary UnitGroup, the BuildOrder
    /// can create and run a MoveOrder "inside of it", to get closer to the building marker. (See BuildOrderNew)
    /// </summary>
    public class OrderPlan
    {
        
        private event Action onClearance;

        public void Clear()
        {
            if (onClearance != null)
                onClearance.Invoke();
            onClearance = null;
        }

        private void AddOnClearance(Action action)
        {
            onClearance += action;
        }

        private void RemoveOnClearance(Action action)
        {
            onClearance -= action;
        }

        private bool lastAddedActiveOrPassive = true;
        private LinkedList<OrderWrapper> activeOrderPlan = new LinkedList<OrderWrapper>();
        private LinkedList<OrderWrapper> passiveOrderPlan = new LinkedList<OrderWrapper>();


        #region Active Orders In Plan Functions

        public OrderWrapper GetFirstInlineActiveOrderInPlan()
        {
            var v = activeOrderPlan.First;
            if (v != null)
                return v.Value;
            else
                return null;
        }

        public OrderWrapper GetPreviousInlineActiveOrderInPlan(OrderWrapper orderWrapper)
        {
            var v = activeOrderPlan.Find(orderWrapper);
            if (v != null && v.Previous != null)
                return v.Previous.Value;
            else
                return null;
        }

        public OrderWrapper GetNextInlineActiveOrderInPlan(OrderWrapper orderWrapper)
        {
            var v = activeOrderPlan.Find(orderWrapper);
            if (v != null && v.Next != null)
                return v.Next.Value;
            else
                return null;
        }

        public OrderWrapper GetLastInlineActiveOrderInPlan()
        {
            var v = activeOrderPlan.Last;
            if (v != null)
                return v.Value;
            else
                return null;
        }

        public bool IsFirstInlineActiveOrderInPlan(OrderWrapper wrapper)
        {
            return GetFirstInlineActiveOrderInPlan() == wrapper;
        }

        public IEnumerable<OrderWrapper> GetAllActiveOrdersFromPlan()
        {
            return activeOrderPlan;
        }

        public bool IsActiveOrderInPlanBeforeOther(OrderWrapper wrapper, OrderWrapper beforeWhich)
        {
            var node = activeOrderPlan.Find(beforeWhich);

            while (node.Previous != null)
            {
                if (node.Previous.Value == wrapper)
                    return true;
                node = node.Previous;
            }

            return false;
        }

        public bool IsActiveOrderInPlanAfterOther(OrderWrapper wrapper, OrderWrapper afterWhich)
        {
            var node = activeOrderPlan.Find(afterWhich);

            while (node.Next != null)
            {
                if (node.Next.Value == wrapper)
                    return true;
                node = node.Next;
            }

            return false;
        }

        #endregion

        #region Queue And Remove Active Orders To/From Plan

        private List<MapMarkerWrapper<OrderMarker>> orderMarkersList = new List<MapMarkerWrapper<OrderMarker>>();

        public bool QueueActiveOrderToPlan(OrderWrapper wrapper, OrderWrapper previous, OrderWrapper next)
        {
            if (previous != null && next != null)
                throw new SystemException("Cannot specify both a predecessor and a successor at the same time for an order when queueing it !");

            if (wrapper != null && activeOrderPlan.Find(wrapper) == null)
            {
                if (Order.GetParameters(wrapper).isPassive)
                    throw new SystemException("Only active (non passive) orders allowed");

                wrapper.SubscribeOnClearance(() => RemoveActiveOrderFromPlan(wrapper));
                AddOnClearance(() => wrapper.DestroyWrappedReference());
                                
                if (previous == null && next == null)
                {
                    lastAddedActiveOrPassive = true;

                    activeOrderPlan.AddLast(wrapper);
                }
                else if (previous != null && activeOrderPlan.Find(previous) != null)
                {
                    if (Order.GetParameters(wrapper).isPassive || Order.GetParameters(previous).isPassive)
                        throw new SystemException("Only active (non passive) orders allowed");
            
                    activeOrderPlan.AddAfter(activeOrderPlan.Find(previous), wrapper);
                }
                else if (next != null && activeOrderPlan.Find(next) != null)
                {
                    if (Order.GetParameters(wrapper).isPassive || Order.GetParameters(next).isPassive)
                        throw new SystemException("Only active (non passive) orders allowed");
            
                    activeOrderPlan.AddBefore(activeOrderPlan.Find(next), wrapper);
                }

                var om = (OrderMarker.CreateInstance(wrapper)).GetMyWrapper<OrderMarker>();
                orderMarkersList.Add(om); // TO BE CHANGED OF COURSE, SO ORDER MARKERS APPEAR FROM BOTTOM TO TOP CORRECT SUCCESSION

                return true;
            }
            else
            {
                return false;    
            }
        }

        private bool RemoveActiveOrderFromPlan(OrderWrapper wrapper)
        {
            if(activeOrderPlan.Find(wrapper) != null)
            {
                activeOrderPlan.Remove(activeOrderPlan.Find(wrapper));

                wrapper.UnsubscribeOnClearance(() => RemoveActiveOrderFromPlan(wrapper));
                RemoveOnClearance(() => wrapper.DestroyWrappedReference());
                
                return true;
            }
            else
            {
                return false;    
            }
        }

        #endregion

        #region Passive Orders In Plan Functions

        public OrderWrapper GetFirstInlinePassiveOrderInPlan()
        {
            var v = passiveOrderPlan.First;
            if (v != null)
                return v.Value;
            else
                return null;
        }

        public OrderWrapper GetPreviousInlinePassiveOrderInPlan(OrderWrapper orderWrapper)
        {
            var v = passiveOrderPlan.Find(orderWrapper);
            if (v != null && v.Previous != null)
                return v.Previous.Value;
            else
                return null;
        }

        public OrderWrapper GetNextInlinePassiveOrderInPlan(OrderWrapper orderWrapper)
        {
            var v = passiveOrderPlan.Find(orderWrapper);
            if (v != null && v.Next != null)
                return v.Next.Value;
            else
                return null;
        }

        public OrderWrapper GetLastInlinePassiveOrderInPlan()
        {
            var v = passiveOrderPlan.Last;
            if (v != null)
                return v.Value;
            else
                return null;
        }

        public bool IsFirstInlinePassiveOrderInPlan(OrderWrapper wrapper)
        {
            return GetFirstInlinePassiveOrderInPlan() == wrapper;
        }

        public IEnumerable<OrderWrapper> GetAllPassiveOrdersFromPlan()
        {
            return passiveOrderPlan.ToList();
        }

        public OrderWrapper GetLastInlineAnyOrderInPlan()
        {
            LinkedListNode<OrderWrapper> v;
            if (lastAddedActiveOrPassive)
                v = activeOrderPlan.Last;
            else
                v = passiveOrderPlan.Last;
            if (v != null)
                return v.Value;
            else
                return null;
        }

        #endregion

        #region Queue And Remove Passive Orders To/From Plan

        public bool QueuePassiveOrderToPlan(OrderWrapper wrapper, OrderWrapper previous, OrderWrapper next)
        {
            if (previous != null && next != null)
                throw new SystemException("Cannot specify both a predecessor and a successor at the same time for an order when queueing it !");

            if(wrapper != null && passiveOrderPlan.Find(wrapper) == null)
            {
                if (!Order.GetParameters(wrapper).isPassive)
                    throw new SystemException("Only passive orders allowed");

                wrapper.SubscribeOnClearance(() => RemovePassiveOrderFromPlan(wrapper));
                AddOnClearance(() => wrapper.DestroyWrappedReference());
                                
                if (previous == null && next == null)
                {
                    lastAddedActiveOrPassive = false;

                    passiveOrderPlan.AddLast(wrapper);
                }
                else if (previous != null && passiveOrderPlan.Find(previous) != null)
                {
                    if (!Order.GetParameters(wrapper).isPassive || !Order.GetParameters(previous).isPassive)
                        throw new SystemException("Only passive orders allowed");
            
                    passiveOrderPlan.AddAfter(passiveOrderPlan.Find(previous), wrapper);
                }
                else if (next != null && activeOrderPlan.Find(next) != null)
                {
                    if (!Order.GetParameters(wrapper).isPassive || !Order.GetParameters(next).isPassive)
                        throw new SystemException("Only passive orders allowed");
        
                    passiveOrderPlan.AddBefore(passiveOrderPlan.Find(next), wrapper);
                }

                var om = (OrderMarker.CreateInstance(wrapper)).GetMyWrapper<OrderMarker>();
                orderMarkersList.Add(om); // TO BE CHANGED OF COURSE, SO ORDER MARKERS APPEAR FROM BOTTOM TO TOP CORRECT SUCCESSION

                return true;
            }
            else
            {
                return false;    
            }
        }

        private bool RemovePassiveOrderFromPlan(OrderWrapper wrapper)
        {
            if(passiveOrderPlan.Find(wrapper) != null)
            {
                passiveOrderPlan.Remove(passiveOrderPlan.Find(wrapper));

                wrapper.UnsubscribeOnClearance(() => RemovePassiveOrderFromPlan(wrapper));
                RemoveOnClearance(() => wrapper.DestroyWrappedReference());
                
                return true;
            }
            else
            {
                return false;    
            }
        }

        #endregion


    }

}