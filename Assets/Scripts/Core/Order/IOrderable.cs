using System.Collections;
using System.Collections.Generic;
using Core.MapMarkers;
using UnityEngine;
using VariousUtilsExtensions;

namespace Core.Orders
{
    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/
    
    /// <summary>
    /// This interface is implemented by classes whose instances can be given orders and passed to Orders and OrderWrappers.
    /// </summary>   
    public interface IOrderableBase
    {
        bool IsWrappedObjectNotNull();        

        //bool IsOrderApplicable(OrderWrapper order);

    }

    /// <summary>
    /// See IOrderableBase
    /// </summary>   
    public interface IOrderable<T> : IOrderable
    {
        RefWrapper<T> GetOrderableAsReferenceWrapperGeneric();

        //new Y GetOrderableAsReferenceWrapperSpecific<Y>() where Y : ReferenceWrapper<T>;

    }


    /// <summary>
    /// See IOrderableBase
    /// </summary>   
    public interface IOrderable : IOrderableBase
    {

        RefWrapper GetOrderableAsReferenceWrapperNonGeneric();

        Y GetOrderableAsReferenceWrapperSpecific<Y>() where Y : RefWrapper;
     
        OrderPlan GetOrdersPlan();

    }
    
}