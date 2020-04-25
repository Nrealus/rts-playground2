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

        bool QueueActiveOrderToPlan(OrderWrapper wrapper, OrderWrapper predecessor, OrderWrapper successor);

        //bool RemoveActiveOrderFromMyPlan(OrderWrapper wrapper);

        OrderWrapper GetFirstInlineActiveOrderInPlan();

        OrderWrapper GetNextInlineActiveOrderInPlan(OrderWrapper orderWrapper);        

        OrderWrapper GetPreviousInlineActiveOrderInPlan(OrderWrapper orderWrapper);        

        IEnumerable<OrderWrapper> GetAllActiveOrdersFromPlan();

        bool IsFirstInlineActiveOrderInPlan(OrderWrapper wrapper);

        bool QueuePassiveOrderToPlan(OrderWrapper wrapper, OrderWrapper predecessor, OrderWrapper successor);

        //bool RemoveActiveOrderFromMyPlan(OrderWrapper wrapper);

        OrderWrapper GetFirstInlinePassiveOrderInPlan();

        OrderWrapper GetNextInlinePassiveOrderInPlan(OrderWrapper orderWrapper);        

        OrderWrapper GetPreviousInlinePassiveOrderInPlan(OrderWrapper orderWrapper);        

        IEnumerable<OrderWrapper> GetAllPassiveOrdersFromPlan();

        bool IsFirstInlinePassiveOrderInPlan(OrderWrapper wrapper);

        //OrderWrapper GetNextOrderInQueue();

        /*OrderWrapper GetMostRecentAddedOrder();

        OrderWrapper<Z> GetNextOrderInQueueSpecific<Z>() where Z : Order;*/     

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