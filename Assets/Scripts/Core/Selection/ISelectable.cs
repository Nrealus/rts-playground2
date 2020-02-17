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
        ReferenceWrapper GetMyReferenceWrapperNonGeneric();
    }


    public interface ISelectable<T> : ISelectable
    {
        ReferenceWrapper<T> GetMyReferenceWrapperGeneric();
    }

    public interface ISelectable<T,Y> : ISelectable<T> where Y : ReferenceWrapper<T>
    {
        Y GetMyReferenceWrapperSpecific();
    }
}