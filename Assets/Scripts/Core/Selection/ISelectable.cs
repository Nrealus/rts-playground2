using Core.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nrealus.Extensions;
using Nrealus.Extensions.ReferenceWrapper;
using Nrealus.Extensions.Observer;

namespace Core.Selection
{

    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    /// <summary>
    /// This interface is implemented by classes whose instances must be able to be "selected" by a Selector object.
    /// </summary>   
    public interface ISelectable
    {
        RefWrapper GetSelectableAsReferenceWrapperNonGeneric();

        Y GetSelectableAsReferenceWrapperSpecific<Y>() where Y : RefWrapper;

        EasyObserver<string,(Selector, bool)> GetOnSelectionStateChangeObserver();

        void InvokeOnSelectionStateChange(Selector selector, bool b);
    }


    /// <summary>
    /// An extension of the ISelectable interface.
    /// </summary>
    /// <typeparam name="T">The type of a class implementing this interface</typeparam>
    /*public interface ISelectable<T> : ISelectable
    {
        //RefWrapper<T> GetSelectableAsReferenceWrapperGeneric();

        //new Y GetSelectableAsReferenceWrapperSpecific<Y>() where Y : RefWrapper<T>;

    }*/

}