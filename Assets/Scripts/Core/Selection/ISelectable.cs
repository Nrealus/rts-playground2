using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;

namespace Core.Selection
{

    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/
    
    /// <summary>
    /// This interface is implemented by classes whose instances must be able to be "selected" by a Selector object.
    /// </summary>   
    public interface ISelectableBase
    {
        //bool IsSelected(Selector selector);

        //bool IsHighlighted(Selector selector);
    }

    /// <summary>
    /// An extension of the ISelectable interface.
    /// </summary>   
    public interface ISelectable : ISelectableBase
    {
        RefWrapper GetSelectableAsReferenceWrapperNonGeneric();

        Y GetSelectableAsReferenceWrapperSpecific<Y>() where Y : RefWrapper;
    }


    /// <summary>
    /// An extension of the ISelectable interface.
    /// </summary>
    /// <typeparam name="T">The type of a class implementing this interface</typeparam>
    public interface ISelectable<T> : ISelectable
    {
        RefWrapper<T> GetSelectableAsReferenceWrapperGeneric();

        //new Y GetSelectableAsReferenceWrapperSpecific<Y>() where Y : ReferenceWrapper<T>;

    }

}