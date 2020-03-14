using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;

namespace Core.Selection
{
    public interface ISelectableBase
    {
        bool IsSelected(Selector selector);

        bool IsHighlighted(Selector selector);

    }

    public interface ISelectable : ISelectableBase
    {
        ReferenceWrapper GetSelectableAsReferenceWrapperNonGeneric();

        Y GetSelectableAsReferenceWrapperSpecific<Y>() where Y : ReferenceWrapper;
    }


    public interface ISelectable<T> : ISelectable
    {
        ReferenceWrapper<T> GetSelectableAsReferenceWrapperGeneric();

        new Y GetSelectableAsReferenceWrapperSpecific<Y>() where Y : ReferenceWrapper<T>;

    }

    public interface ISelectable<T,Y> : ISelectable<T> where Y : ReferenceWrapper<T>
    {
        Y GetSelectableAsReferenceWrapperSpecific();
    }
}