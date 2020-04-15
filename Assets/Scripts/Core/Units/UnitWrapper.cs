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

        public ReferenceWrapper<IOrderable> testFunction()
        {
            return null;
        }

        private List<OrderWrapper> orderWrappersList = new List<OrderWrapper>();
        private OrderWrapper currentOrderWrapper;
        private List<MapMarkerWrapper<OrderMarker>> orderMarkersList = new List<MapMarkerWrapper<OrderMarker>>();

        public bool AddOrderToList(OrderWrapper wrapper, OrderWrapper predecessor)
        {
            if(!orderWrappersList.Contains(wrapper))
            {
                if(predecessor == null)
                {
                    if(currentOrderWrapper == null)
                        currentOrderWrapper = wrapper;
                    else
                        orderWrappersList.Add(wrapper);
                }
                else
                {
                    if(currentOrderWrapper == null)
                        currentOrderWrapper = wrapper;
                    else
                        orderWrappersList.Insert(orderWrappersList.IndexOf(predecessor)+1, wrapper);                    
                }
    
                orderMarkersList.Add((new OrderMarker(wrapper)).GetMyWrapper<OrderMarker>());
                return true;
            }
            else
            {
                return false;    
            }
        }

        public bool RemoveOrderFromList(OrderWrapper wrapper)
        {
            if(currentOrderWrapper == wrapper || orderWrappersList.Contains(wrapper))
            {
                if(currentOrderWrapper == wrapper)
                {
                    if(orderWrappersList.Count > 0)
                    {
                        currentOrderWrapper = orderWrappersList[0];
                        orderWrappersList.RemoveAt(0);
                    }
                    else
                        currentOrderWrapper = null;
                }
                else
                    orderWrappersList.Remove(wrapper);
                
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
        }


        public OrderWrapper GetCurrentOrderInQueue()
        {
            return currentOrderWrapper;
        }

        /*public OrderWrapper GetNextOrderInQueue()
        {
            if(orderWrappersList.Count > 0)
                return orderWrappersList[0];
            else
                return null;
        }*/

        public OrderWrapper GetMostRecentAddedOrder()
        {
            if(orderWrappersList.Count > 0)
                return orderWrappersList[orderWrappersList.Count-1];
            else
                return currentOrderWrapper;
        }

        public OrderWrapper<Z> GetNextOrderInQueueSpecific<Z>() where Z : Order
        {
            if(orderWrappersList.Count > 0)
                return orderWrappersList[0].GetWrappedAs<Z>().GetMyWrapper<Z>();
            else
                return null;
        }

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
    }
}