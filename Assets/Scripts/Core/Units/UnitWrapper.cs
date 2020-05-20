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
    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    ///<summary>
    /// The RefWrapper for Unit.
    ///</summary>
    public class UnitWrapper : RefWrapper<Unit>, 
        ISelectable<Unit>, IOrderable<Unit>
    {

        public UnitGroupWrapper unitsGroupWrapper;
        
        public void ChangeUnitsFormation(UnitGroupWrapper frmwrp)
        {
            UnitGroup.AddUnitToGroup(this, frmwrp, () => { unitsGroupWrapper = null; });
            unitsGroupWrapper = frmwrp;
        }

        private OrderPlan _ordersPlan;
        public OrderPlan GetOrdersPlan()
        {
            return _ordersPlan;
        }
        
        public UnitWrapper(Unit wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
            _ordersPlan = new OrderPlan();

            SubscribeOnClearance(() => { GetOrdersPlan().Clear(); _ordersPlan = null; });

            unitsGroupWrapper = new UnitGroup(new List<UnitWrapper>() { this }, false).GetMyWrapper();

            ChangeUnitsFormation(unitsGroupWrapper);
        }

        #region ISelectables explicit implementations

        RefWrapper ISelectable.GetSelectableAsReferenceWrapperNonGeneric()
        {
            return this;
        }

        RefWrapper<Unit> ISelectable<Unit>.GetSelectableAsReferenceWrapperGeneric()
        {
            return this;
        }

        Y ISelectable.GetSelectableAsReferenceWrapperSpecific<Y>()
        {
            return this as Y;
        }

        #endregion

        #region IOrderables explicit implementations

        RefWrapper IOrderable.GetOrderableAsReferenceWrapperNonGeneric()
        {
            return this;
        }

        RefWrapper<Unit> IOrderable<Unit>.GetOrderableAsReferenceWrapperGeneric()
        {
            return this;
        }

        Y IOrderable.GetOrderableAsReferenceWrapperSpecific<Y>()
        {
            return this as Y;
        }

        #endregion
        
        public bool IsWrappedObjectNotNull()
        {
            return WrappedObject != null;
        }

        

    }
}