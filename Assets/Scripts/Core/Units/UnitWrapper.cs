using Core.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;
using Core.Selection;
using Core.Orders;
using GlobalManagers;
using Core.Handlers;
using Core.MapMarkers;
using System.Linq;

namespace Core.Units
{
    public class UnitWrapper : ReferenceWrapper<Unit>, 
        ISelectable<Unit, UnitWrapper>, IOrderable<Unit,UnitWrapper>
    {

        private OrderHandler orderHandler;

        public UnitWrapper(Unit wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
            orderHandler = GameManager.Instance.currentMainHandler.orderHandler;
        }

        #region ISelectables explicit implementations

        ReferenceWrapper ISelectable.GetSelectableAsReferenceWrapperNonGeneric()
        {
            return this;
        }

        ReferenceWrapper<Unit> ISelectable<Unit>.GetSelectableAsReferenceWrapperGeneric()
        {
            return this;
        }

        Y ISelectable.GetSelectableAsReferenceWrapperSpecific<Y>()
        {
            return this as Y;
        }

        Y ISelectable<Unit>.GetSelectableAsReferenceWrapperSpecific<Y>()
        {
            return this as Y;
        }

        UnitWrapper ISelectable<Unit, UnitWrapper>.GetSelectableAsReferenceWrapperSpecific()
        {
            return this;
        }

        #endregion

        #region IOrderables explicit implementations

        ReferenceWrapper IOrderable.GetOrderableAsReferenceWrapperNonGeneric()
        {
            return this;
        }

        ReferenceWrapper<Unit> IOrderable<Unit>.GetOrderableAsReferenceWrapperGeneric()
        {
            return this;
        }

        Y IOrderable.GetOrderableAsReferenceWrapperSpecific<Y>()
        {
            return this as Y;
        }
         Y IOrderable<Unit>.GetOrderableAsReferenceWrapperSpecific<Y>()
        {
            return this as Y;
        }

        UnitWrapper IOrderable<Unit, UnitWrapper>.GetOrderableAsReferenceWrapperSpecific()
        {
            return this;
        }

        #endregion

        
        public bool AmIStillUsed()
        {
            return WrappedObject != null;
        }
        /*
        public bool IsSelected(Selector selector)
        {
            return selector.IsSelected(this);
        }

        public bool IsHighlighted(Selector selector)
        {
            return selector.IsHighlighted(this);
        }

        public bool IsParentHighlightedOrSelected(Selector selector)
        {
            if (WrappedObject != null && WrappedObject.GetParentWrapper() != null)
            {
                return WrappedObject.GetParentWrapper().IsSelected(selector)
                    || WrappedObject.GetParentWrapper().IsHighlighted(selector);
            }
            else
            {
                return false;
            }
        }

        public bool IsThereAnyParentHighlightedOrSelected(Selector selector)
        {
            if (WrappedObject != null && WrappedObject.GetParentWrapper() != null)
            {
                bool b = WrappedObject.GetParentWrapper().IsSelected(selector)
                    || WrappedObject.GetParentWrapper().IsHighlighted(selector);
                return b || WrappedObject.GetParentWrapper().IsThereAnyParentHighlightedOrSelected(selector);
            }
            else
            {
                return false;
            }
        }*/


        /*private PseudoPetriNet<OrderWrapper> orderWrappersNet = new PseudoPetriNet<OrderWrapper>();
        private List<MapMarkerWrapper<OrderMarker>> orderMarkersList = new List<MapMarkerWrapper<OrderMarker>>();

        public bool AddOrderToMyPlan(OrderWrapper wrapper, OrderWrapper previous, OrderWrapper next)
        {
            if (previous != null && next != null)
                throw new SystemException("Cannot specify both a predecessor and a successor for an order when adding it to Orderable order list");

            if(wrapper != null && !orderWrappersNet.ContainsState(wrapper))
            {

                wrapper.SubscribeOnClearance(() => RemoveOrderFromMyPlan(wrapper));
                SubscribeOnClearance(() => wrapper.DestroyWrappedReference());
                                
                if (previous == null && next == null)
                {
                    orderWrappersNet.AddState(wrapper, null, null, null);
                    orderWrappersNet.AddTokenAtState(wrapper);
                }
                else if (previous != null && next == null)
                {
                    //orderWrappersList.AddAfter(orderWrappersList.Find(previous), wrapper);
                }
                else if (previous == null && next != null)
                {
                    //orderWrappersList.AddBefore(orderWrappersList.Find(next), wrapper);
                }

                var om = (new OrderMarker(wrapper)).GetMyWrapper<OrderMarker>();
                orderMarkersList.Add(om); // TO BE CHANGED OF COURSE, SO ORDER MARKERS APPEAR FROM BOTTOM TO TOP CORRECT SUCCESSION

                return true;
            }
            else
            {
                return false;    
            }
        }

        public bool RemoveOrderFromMyPlan(OrderWrapper wrapper)
        {
            if(orderWrappersNet.ContainsState(wrapper))
            {
                orderWrappersNet.RemoveState(wrapper);

                wrapper.UnsubscribeOnClearance(() => RemoveOrderFromMyPlan(wrapper));
                UnsubscribeOnClearance(() => wrapper.DestroyWrappedReference());
                
                return true;
            }
            else
            {
                return false;    
            }
        }*/
        
        //private SimplePriorityQueue<OrderWrapper, int> orderWrappersQueue = new SimplePriorityQueue<OrderWrapper, int>();
        //private SimplePriorityQueue<MapMarkerWrapper<OrderMarker>, int> orderMarkersQueue = new SimplePriorityQueue<MapMarkerWrapper<OrderMarker>, int>();

        /*private LinkedList<OrderWrapper> orderWrappersList = new LinkedList<OrderWrapper>();
        private LinkedList<MapMarkerWrapper<OrderMarker>> orderMarkersList = new LinkedList<MapMarkerWrapper<OrderMarker>>();

        public bool AddOrderToMine(OrderWrapper wrapper, OrderWrapper previous, OrderWrapper next)
        {
            if (previous != null && next != null)
                throw new SystemException("Cannot specify both a predecessor and a successor for an order when adding it to Orderable order list");

            if(wrapper != null && !orderWrappersList.Contains(wrapper))
            {

                wrapper.SubscribeOnClearance(() => RemoveOrderFromMine(wrapper));
                SubscribeOnClearance(() => wrapper.DestroyWrappedReference());
                                
                if (previous == null && next == null)
                {
                    orderWrappersList.AddLast(wrapper);
                }
                else if (previous != null && next == null)
                {
                    orderWrappersList.AddAfter(orderWrappersList.Find(previous), wrapper);
                }
                else if (previous == null && next != null)
                {
                    orderWrappersList.AddBefore(orderWrappersList.Find(next), wrapper);
                }

                var om = (new OrderMarker(wrapper)).GetMyWrapper<OrderMarker>();
                orderMarkersList.AddFirst(om); // TO BE CHANGED OF COURSE, SO ORDER MARKERS APPEAR FROM BOTTOM TO TOP CORRECT SUCCESSION

                return true;
            }
            else
            {
                return false;    
            }
        }

        public bool RemoveOrderFromMine(OrderWrapper wrapper)
        {
            if(orderWrappersList.Contains(wrapper))
            {
                orderWrappersList.Remove(orderWrappersList.Find(wrapper));

                wrapper.UnsubscribeOnClearance(() => RemoveOrderFromMine(wrapper));
                UnsubscribeOnClearance(() => wrapper.DestroyWrappedReference());
                
                return true;
            }
            else
            {
                return false;    
            }
        }*/
        
        /*public bool AddOrderToQueue(OrderWrapper wrapper, OrderWrapper predecessor, OrderWrapper successor)
        {
            if (predecessor != null && successor != null)
                throw new SystemException("Cannot specify both a predecessor and a successor for an order when adding it to Orderable order list");

            if(!orderWrappersQueue.Contains(wrapper))
            {

                wrapper.SubscribeOnClearance(() => RemoveOrderFromList(wrapper));
                SubscribeOnClearance(() => wrapper.DestroyWrappedReference());
                
                if (predecessor == null && successor == null)
                {
                    if(currentOrderWrapper == null)
                        currentOrderWrapper = wrapper;
                    else
                        orderWrappersQueue.Add(wrapper);
                }
                else if (predecessor != null && successor == null)
                {
                    if(currentOrderWrapper == predecessor)
                        orderWrappersQueue.Insert(0, wrapper);
                    else
                        orderWrappersQueue.Insert(orderWrappersQueue.IndexOf(predecessor)+1, wrapper);                    
                }
                else if (predecessor == null && successor != null)
                {
                    if (currentOrderWrapper == successor)
                    {
                        currentOrderWrapper = wrapper;
                        orderWrappersQueue.Insert(0, successor);
                    }
                    else
                    {
                        orderWrappersQueue.Insert(orderWrappersQueue.IndexOf(successor), wrapper);                    
                    }
                }

                var om = (new OrderMarker(wrapper)).GetMyWrapper<OrderMarker>();
                orderMarkersList.Add(om);

                return true;
            }
            else
            {
                return false;    
            }
        }

        public bool RemoveOrderFromList(OrderWrapper wrapper)
        {
            if(currentOrderWrapper == wrapper || orderWrappersQueue.Contains(wrapper))
            {
                wrapper.UnsubscribeOnClearance(() => RemoveOrderFromList(wrapper));
                UnsubscribeOnClearance(() => wrapper.DestroyWrappedReference());

                if(currentOrderWrapper == wrapper)
                {
                    if(orderWrappersQueue.Count > 0)
                    {
                        currentOrderWrapper = orderWrappersQueue[0];
                        orderWrappersQueue.RemoveAt(0);
                    }
                    else
                        currentOrderWrapper = null;
                }
                else
                    orderWrappersQueue.Remove(wrapper);
                
                int c = orderMarkersList.Count;
                for (int i=c-1; i>=0; i--)
                {
                    if (orderMarkersList[i].GetWrappedAs<OrderMarker>().ordWrapper == wrapper
                        //&& orderMarkersList[i].GetWrappedAs<OrderMarker>().ordWrapper != currentOrderWrapper
                        )
                    {
                        orderMarkersList[i].DestroyWrappedReference();
                        orderMarkersList.RemoveAt(i);
                    }
                }
                return true;
            }
            else
            {
                return false;    
            }
        }*/


        /*public OrderWrapper GetCurrentOrderInQueue()
        {
            return currentOrderWrapper;
        }*/

        /*public OrderWrapper GetFirstInlineActiveOrderInPlan()
        {
            if (linearOrderPlan.First != null)
                return linearOrderPlan.First.Value.activeOrder;
            else
                return null;
        }

        public IEnumerable<OrderWrapper> GetFirstInlinePassiveOrdersInPlan()
        {
            return GetPlanNodePassives(linearOrderPlan.First.Value);
        }

        private IEnumerable<OrderWrapper> GetPlanNodePassives(InlineOrdersPlanNode inlineOrdersPlanNode)
        {
            List<OrderWrapper> res = new List<OrderWrapper>();
            foreach (var v in inlineOrdersPlanNode.passiveOrdersTree.ChildNodes)
                res.Add(v.obj);
            return res;
        }

        public bool IsFirstInlineActiveOrderInPlan(OrderWrapper wrapper)
        {
            return GetFirstInlineActiveOrderInPlan() == wrapper;
        }

        public OrderWrapper GetLastInlineActiveOrderInPlan()
        {
            if (linearOrderPlan.Last != null)
                return linearOrderPlan.Last.Value.activeOrder;
            else
                return null;
        }

        public OrderWrapper GetNextInlineActiveOrderInPlan(OrderWrapper wrapper)
        {
            if (Order.GetParameters(wrapper).isPassive)
                throw new System.Exception("Order should not be passive to be a parameter of this method");

            var v = linearOrderPlan.Where((_) => { return _.activeOrder == wrapper; }).First();
            if (linearOrderPlan.Find(v) != null && linearOrderPlan.Find(v).Next != null)
                return linearOrderPlan.Find(v).Next.Value.activeOrder;//.Next;
            else
                return null;
        }

        public OrderWrapper GetPreviousInlinePassiveOrderInPlan(OrderWrapper wrapper)
        {
            if (!Order.GetParameters(wrapper).isPassive)
                throw new System.Exception("Order should be passive to be a parameter of this method");

            //var v = linearOrderPlan.Where((_) => { return _.passiveOrdersTree.BFSFindNode(wrapper).ChildNodes[0].obj != null; }).First();

            OrderWrapper res = null;
            foreach (var v in linearOrderPlan)
            {
                var temp = v.passiveOrdersTree.BFSFindNode(wrapper).GetParent();
                if (temp != null)
                    res=temp.obj;
                if (res != null)
                    return res;
            }
            return null;
        }

        public OrderWrapper GetNextInlinePassiveOrderInPlan(OrderWrapper wrapper)
        {
            if (!Order.GetParameters(wrapper).isPassive)
                throw new System.Exception("Order should be passive to be a parameter of this method");

            //var v = linearOrderPlan.Where((_) => { return _.passiveOrdersTree.BFSFindNode(wrapper).ChildNodes[0].obj != null; }).First();

            OrderWrapper res;
            foreach (var v in linearOrderPlan)
            {
                res = v.passiveOrdersTree.BFSFindNode(wrapper).ChildNodes[0].obj;
                if (res != null)
                    return res;
            }
            return null;
        }

        private void TransferPassivesFromOneInlineToOther(InlineOrdersPlanNode source, InlineOrdersPlanNode destination)
        {
            var temp = source.passiveOrdersTree;
            source.passiveOrdersTree = null;
            destination.passiveOrdersTree = temp;
            // different joining policies ? (union, intersect...)
        }

        /*public OrderWrapper<Z> GetNextOrderInQueueSpecific<Z>() where Z : Order
        {
            if(orderWrappersQueue.Count > 0)
                return orderWrappersQueue[0].GetWrappedAs<Z>().GetMyWrapper<Z>();
            else
                return null;
        }*/

        /*
        public bool IsOrderApplicable(OrderWrapper orderWrapper)
        {
            if(WrappedObject != null && orderWrapper.WrappedObject.IsInPhase(Order.OrderPhase.RequestConfirmation))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        */

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

        public bool IsActiveOrderBeforeOtherInPlan(OrderWrapper wrapper, OrderWrapper beforeWhich)
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

        public bool IsActiveOrderAfterOtherInPlan(OrderWrapper wrapper, OrderWrapper afterWhich)
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

        ///////////////////////

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

        private bool lastAddedActiveOrPassive = true;

        private LinkedList<OrderWrapper> activeOrderPlan = new LinkedList<OrderWrapper>();

        //private PseudoPetriNet<OrderWrapper> orderWrappersNet = new PseudoPetriNet<OrderWrapper>();
        private List<MapMarkerWrapper<OrderMarker>> orderMarkersList = new List<MapMarkerWrapper<OrderMarker>>();

        public bool QueueActiveOrderToPlan(OrderWrapper wrapper, OrderWrapper previous, OrderWrapper next)
        {
            if (previous != null && next != null)
                throw new SystemException("Cannot specify both a predecessor and a successor at the same time for an order when queueing it !");

            if(wrapper != null && activeOrderPlan.Find(wrapper) == null)
            {
                if (Order.GetParameters(wrapper).isPassive)
                    throw new SystemException("Only active (non passive) orders allowed");

                wrapper.SubscribeOnClearance(() => RemoveActiveOrderFromPlan(wrapper));
                SubscribeOnClearance(() => wrapper.DestroyWrappedReference());
                                
                if (previous == null && next == null)
                {
                    lastAddedActiveOrPassive = true;

                    activeOrderPlan.AddLast(wrapper);
                    //orderWrappersNet.AddState(wrapper, null, null, null);
                    //orderWrappersNet.AddTokenAtState(wrapper);
                }
                else if (previous != null && activeOrderPlan.Find(previous) != null)
                {
                    if (Order.GetParameters(wrapper).isPassive || Order.GetParameters(previous).isPassive)
                        throw new SystemException("Only active (non passive) orders allowed");
            
                    activeOrderPlan.AddAfter(activeOrderPlan.Find(previous), wrapper);
                    //orderWrappersList.AddAfter(orderWrappersList.Find(previous), wrapper);
                }
                else if (next != null && activeOrderPlan.Find(next) != null)
                {
                    if (Order.GetParameters(wrapper).isPassive || Order.GetParameters(next).isPassive)
                        throw new SystemException("Only active (non passive) orders allowed");
            
                    activeOrderPlan.AddBefore(activeOrderPlan.Find(next), wrapper);
                    //orderWrappersList.AddBefore(orderWrappersList.Find(next), wrapper);
                }

                var om = (new OrderMarker(wrapper)).GetMyWrapper<OrderMarker>();
                orderMarkersList.Add(om); // TO BE CHANGED OF COURSE, SO ORDER MARKERS APPEAR FROM BOTTOM TO TOP CORRECT SUCCESSION

                return true;
            }
            else
            {
                return false;    
            }
        }

        private LinkedList<OrderWrapper> passiveOrderPlan = new LinkedList<OrderWrapper>();

        public bool QueuePassiveOrderToPlan(OrderWrapper wrapper, OrderWrapper previous, OrderWrapper next)
        {
            if (previous != null && next != null)
                throw new SystemException("Cannot specify both a predecessor and a successor at the same time for an order when queueing it !");

            if(wrapper != null && passiveOrderPlan.Find(wrapper) == null)
            {
                if (!Order.GetParameters(wrapper).isPassive)
                    throw new SystemException("Only passive orders allowed");

                wrapper.SubscribeOnClearance(() => RemovePassiveOrderFromPlan(wrapper));
                SubscribeOnClearance(() => wrapper.DestroyWrappedReference());
                                
                if (previous == null && next == null)
                {
                    lastAddedActiveOrPassive = false;

                    passiveOrderPlan.AddLast(wrapper);
                    //orderWrappersNet.AddState(wrapper, null, null, null);
                    //orderWrappersNet.AddTokenAtState(wrapper);
                }
                else if (previous != null && passiveOrderPlan.Find(previous) != null)
                {
                    if (!Order.GetParameters(wrapper).isPassive || !Order.GetParameters(previous).isPassive)
                        throw new SystemException("Only passive orders allowed");
            
                    passiveOrderPlan.AddAfter(passiveOrderPlan.Find(previous), wrapper);
                    //orderWrappersList.AddAfter(orderWrappersList.Find(previous), wrapper);
                }
                else if (next != null && activeOrderPlan.Find(next) != null)
                {
                    if (!Order.GetParameters(wrapper).isPassive || !Order.GetParameters(next).isPassive)
                        throw new SystemException("Only passive orders allowed");
        
                    passiveOrderPlan.AddBefore(passiveOrderPlan.Find(next), wrapper);
                    //orderWrappersList.AddBefore(orderWrappersList.Find(next), wrapper);
                }

                var om = (new OrderMarker(wrapper)).GetMyWrapper<OrderMarker>();
                orderMarkersList.Add(om); // TO BE CHANGED OF COURSE, SO ORDER MARKERS APPEAR FROM BOTTOM TO TOP CORRECT SUCCESSION

                return true;
            }
            else
            {
                return false;    
            }
        }

        /*public bool AddPassiveOrderToPlan(OrderWrapper wrapper, OrderWrapper previous, OrderWrapper next)
        {
            if (previous != null && next != null)
                throw new SystemException("Cannot specify both a predecessor and a successor for an order when adding it to Orderable order list");

            if(wrapper != null && !linearOrderPlan.Any((_) => { return _.activeOrder == wrapper; }))
            {

                wrapper.SubscribeOnClearance(() => RemoveActiveOrderFromPlan(wrapper));
                SubscribeOnClearance(() => wrapper.DestroyWrappedReference());
                                
                if (previous == null && next == null)
                {
                    
                    if (!Order.GetParameters(wrapper).isPassive)
                        throw new SystemException("Only passive orders allowed");

                    if (linearOrderPlan.First != null)
                    {
                        linearOrderPlan.First.Value.passiveOrdersTree.AddChild(new SimpleTreeNode<OrderWrapper>(wrapper));
                    }
                    else
                    {
                        var iop = new InlineOrdersPlanNode();
                        iop.passiveOrdersTree.AddChild(new SimpleTreeNode<OrderWrapper>(wrapper));
                        linearOrderPlan.AddLast(iop);
                    }
                }
                else if (previous != null && next == null)
                {
                    
                    if (!Order.GetParameters(wrapper).isPassive || !Order.GetParameters(previous).isPassive)
                        throw new SystemException("Only passive orders allowed");

                    var prevInlineNode = linearOrderPlan.Where((_) => { return _.passiveOrdersTree.BFSFindNode(previous) != null; }).First();
                    var trn = prevInlineNode.passiveOrdersTree.BFSFindNode(previous);
                    if (trn != null)
                    {
                        var toAdd = new SimpleTreeNode<OrderWrapper>(wrapper);
                        prev.passiveOrdersTree.BFSFindNode(previous)
                        prev.Value.passiveOrdersTree.AddChild(new SimpleTreeNode<OrderWrapper>(wrapper));
                    }
                    else
                    {
                        var iop = new InlineOrdersPlanNode();
                        iop.passiveOrdersTree.AddChild(new SimpleTreeNode<OrderWrapper>(wrapper));
                        linearOrderPlan.AddLast(iop);
                    }
                }
                else if (previous == null && next != null)
                {
                    //orderWrappersList.AddBefore(orderWrappersList.Find(next), wrapper);
                }

                var om = (new OrderMarker(wrapper)).GetMyWrapper<OrderMarker>();
                orderMarkersList.Add(om); // TO BE CHANGED OF COURSE, SO ORDER MARKERS APPEAR FROM BOTTOM TO TOP CORRECT SUCCESSION

                return true;
            }
            else
            {
                return false;    
            }
        }*/

        private bool RemoveActiveOrderFromPlan(OrderWrapper wrapper)
        {
            if(activeOrderPlan.Find(wrapper) != null)
            {
                activeOrderPlan.Remove(activeOrderPlan.Find(wrapper));
                //orderWrappersNet.RemoveState(wrapper);

                wrapper.UnsubscribeOnClearance(() => RemoveActiveOrderFromPlan(wrapper));
                UnsubscribeOnClearance(() => wrapper.DestroyWrappedReference());
                
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
                //orderWrappersNet.RemoveState(wrapper);

                wrapper.UnsubscribeOnClearance(() => RemovePassiveOrderFromPlan(wrapper));
                UnsubscribeOnClearance(() => wrapper.DestroyWrappedReference());
                
                return true;
            }
            else
            {
                return false;    
            }
        }

    }
}