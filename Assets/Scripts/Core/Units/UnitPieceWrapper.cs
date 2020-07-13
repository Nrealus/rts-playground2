using Core.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nrealus.Extensions;
using Core.Selection;
using Core.Tasks;
using GlobalManagers;
using Core.Handlers;
using Core.MapMarkers;
using System.Linq;
using Nrealus.Extensions.ReferenceWrapper;
using Nrealus.Extensions.Observer;

namespace Core.Units
{
    /****** Author : nrealus ****** Last documentation update : 12-07-2020 ******/

    ///<summary>
    /// The RefWrapper for UnitPiece.
    ///</summary>
    public class UnitPieceWrapper : RefWrapper<UnitPiece>, 
        ISelectable
    {

        public UnitWrapper unitWrapper;
        
        public void ChangeUnit(UnitWrapper uwp)
        {
            Unit.AddUnitPieceToUnit(this, uwp, () => { unitWrapper = null; });
            unitWrapper = uwp;
        }
        
        public UnitPieceWrapper(UnitPiece wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
            unitWrapper = Unit.CreateUnit(false, true).GetRefWrapper();
            // weird. should already be given from somwhere else

            ChangeUnit(unitWrapper);
        }

        protected override void Constructor2(Action nullifyPrivateRefToWrapper)
        {
            nullifyPrivateRefToWrapper += () => GetOnSelectionStateChangeObserver().UnsubscribeFromAllEvents();
            base.Constructor2(nullifyPrivateRefToWrapper);
        }

        #region ISelectables explicit implementations

        private EasyObserver<string, (Selector,bool)> onSelectionStateChange = new EasyObserver<string, (Selector, bool)>();

        public EasyObserver<string, (Selector,bool)> GetOnSelectionStateChangeObserver()
        {
            return onSelectionStateChange;
        }
        
        void ISelectable.InvokeOnSelectionStateChange(Selector selector, bool b)
        {
            onSelectionStateChange.Invoke((selector, b));
        }

        RefWrapper ISelectable.GetSelectableAsReferenceWrapperNonGeneric()
        {
            return this;
        }

        /*RefWrapper<UnitPiece> ISelectable<UnitPiece>.GetSelectableAsReferenceWrapperGeneric()
        {
            return this;
        }*/

        Y ISelectable.GetSelectableAsReferenceWrapperSpecific<Y>()
        {
            return this as Y;
        }

        #endregion

        /*#region IOrderables explicit implementations

        RefWrapper ITaskSubject.GetTaskSubjectAsReferenceWrapperNonGeneric()
        {
            return this;
        }

        RefWrapper<Unit> ITaskSubject<Unit>.GetOrderableAsReferenceWrapperGeneric()
        {
            return this;
        }

        Y ITaskSubject.GetTaskSubjectAsReferenceWrapperSpecific<Y>()
        {
            return this as Y;
        }

        #endregion*/
        
        public bool IsWrappedObjectNotNull()
        {
            return GetWrappedReference() != null;
        }

        

    }
}