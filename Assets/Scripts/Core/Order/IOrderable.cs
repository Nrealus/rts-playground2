using System.Collections;
using System.Collections.Generic;
using Core.MapMarkers;
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
        ReferenceWrapper GetOrderableAsReferenceWrapperNonGeneric();

        Y GetOrderableAsReferenceWrapperSpecific<Y>() where Y : ReferenceWrapper;

        bool AddOrderToList(OrderWrapper wrapper, OrderWrapper predecessor);

        bool RemoveOrderFromList(OrderWrapper wrapper);

        OrderWrapper GetCurrentOrderInQueue();

        //OrderWrapper GetNextOrderInQueue();

        OrderWrapper GetLastAddedOrder();

        OrderWrapper<Z> GetNextOrderInQueueSpecific<Z>() where Z : Order;        

    }


    public interface IOrderable<T> : IOrderable
    {
        ReferenceWrapper<T> GetOrderableAsReferenceWrapperGeneric();

        new Y GetOrderableAsReferenceWrapperSpecific<Y>() where Y : ReferenceWrapper<T>;

    }

    public interface IOrderable<T, Y> : IOrderable<T> where Y : ReferenceWrapper<T>
    {
        Y GetOrderableAsReferenceWrapperSpecific();
    }
    
}