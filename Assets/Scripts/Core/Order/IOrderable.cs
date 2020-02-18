using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;

namespace Core.Orders
{
    public interface IOrderableBase
    {
        bool AmIStillUsed();        

        //bool IsOrderApplicable(OrderWrapper order);

    }

    public interface IOrderable : IOrderableBase
    {
        ReferenceWrapper GetMyReferenceWrapperNonGeneric();
    }


    public interface IOrderable<T> : IOrderable
    {
        ReferenceWrapper<T> GetMyReferenceWrapperGeneric();
    }

    public interface IOrderable<T, Y> : IOrderable<T> where Y : ReferenceWrapper<T>
    {
        Y GetMyReferenceWrapperSpecific();
    }
    
}