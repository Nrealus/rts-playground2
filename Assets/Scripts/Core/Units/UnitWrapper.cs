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

namespace Core.Units
{
    public class UnitWrapper : ReferenceWrapper<Unit>, 
        ISelectable<Unit, UnitWrapper>, IOrderable<Unit,UnitWrapper>
    {

        //private List<Order> orderQueue = new List<Order>();

        private OrderHandler orderHandler;

        public UnitWrapper(Unit wrappedObject) : base(wrappedObject)
        {
            orderHandler = GameManager.Instance.currentMainHandler.orderHandler;
        }

        ReferenceWrapper ISelectable.GetMyReferenceWrapperNonGeneric()
        {
            return this;
        }

        ReferenceWrapper IOrderable.GetMyReferenceWrapperNonGeneric()
        {
            return this;
        }

        ReferenceWrapper<Unit> ISelectable<Unit>.GetMyReferenceWrapperGeneric()
        {
            return this;
        }

        ReferenceWrapper<Unit> IOrderable<Unit>.GetMyReferenceWrapperGeneric()
        {
            return this;
        }

        UnitWrapper IOrderable<Unit, UnitWrapper>.GetMyReferenceWrapperSpecific()
        {
            return this;
        }

        UnitWrapper ISelectable<Unit, UnitWrapper>.GetMyReferenceWrapperSpecific()
        {
            return this;
        }

        public bool AmIStillUsed()
        {
            return WrappedObject != null;
        }

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
            if (WrappedObject.GetParentWrapper() != null)
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
            if (WrappedObject.GetParentWrapper() != null)
            {
                bool b = WrappedObject.GetParentWrapper().IsSelected(selector)
                    || WrappedObject.GetParentWrapper().IsHighlighted(selector);
                return b || WrappedObject.GetParentWrapper().IsThereAnyParentHighlightedOrSelected(selector);
            }
            else
            {
                return false;
            }
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