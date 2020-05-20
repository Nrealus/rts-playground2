using UnityEngine;
using System.Collections.Generic;
using System;
using Core.Units;
using VariousUtilsExtensions;
using Core.Handlers;

namespace Core.Orders
{
    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/
    
    /// <summary>
    /// This static class is an implementation of the Factory pattern.
    /// It is used to instantiation and minimal configuration (receivers) of "concrete" Order subclass instances of the type passed to the instantiation functions,
    /// thanks to a dark magical trick (see the switch statement.)
    /// And actually, this could even be directly integrated to OrderHandler.
    /// However, for now, this seems to work out well and there is no need whatsoever to merge these two classes.
    /// </summary>      
    public static class OrderFactory
    {
        
        private static OrderWrapper<T> CreateOrderWrapper<T>() where T : Order
        {
            switch (typeof(T))
            {
                /*case Type moType when moType == typeof(MoveOrder):
                {
                    MoveOrder mo = new MoveOrder();
                    OrderWrapper<MoveOrder> wrapper = mo.GetMyWrapper<MoveOrder>();
                    return wrapper as OrderWrapper<T>;
                }*/
                case Type moType when moType == typeof(MoveOrderNew):
                {
                    MoveOrderNew mo = new MoveOrderNew();
                    OrderWrapper<MoveOrderNew> wrapper = mo.GetMyWrapper<MoveOrderNew>();
                    return wrapper as OrderWrapper<T>;
                }
                case Type moType when moType == typeof(BuildOrderNew):
                {
                    BuildOrderNew mo = new BuildOrderNew();
                    OrderWrapper<BuildOrderNew> wrapper = mo.GetMyWrapper<BuildOrderNew>();
                    return wrapper as OrderWrapper<T>;
                }
                case Type moType when moType == typeof(EngageAtPositionsOrderNew):
                {
                    EngageAtPositionsOrderNew mo = new EngageAtPositionsOrderNew();
                    OrderWrapper<EngageAtPositionsOrderNew> wrapper = mo.GetMyWrapper<EngageAtPositionsOrderNew>();
                    return wrapper as OrderWrapper<T>;
                }
                /*case Type MoveOrderType 
                when MoveOrderType == typeof(MoveOrder) :
                    bool j = true;
                    break;*/
                default:
                    throw new ArgumentException(
                    message: "not a recognized type of order");
                    //return null;
            }

        } 

        public static OrderWrapper<T> CreateOrderWrapperWithoutReceiver<T>() where T : Order
        {
            OrderWrapper<T> res = CreateOrderWrapper<T>();
            
            OrderHandler.AddToGlobalOrderWrapperList(res);

            return res;
        }

        public static OrderWrapper<T> CreateOrderWrapperAndSetReceiver<T>(IOrderable<Unit> receiverWrapper) where T : Order
        {
            OrderWrapper<T> res = CreateOrderWrapper<T>();
            
            OrderHandler.AddToGlobalOrderWrapperList(res);

            Order.SetReceiver(res, null, null, receiverWrapper);
            return res;
        }

        public static OrderWrapper<T> CreatePredecessorOrderWrapperAndSetReceiver<T>(IOrderable<Unit> receiverWrapper, OrderWrapper successor) where T : Order
        {
            OrderWrapper<T> res = CreateOrderWrapper<T>();
            
            OrderHandler.AddToGlobalOrderWrapperList(res);

            Order.SetReceiver(res, null, successor, receiverWrapper);
            return res;
        }

        public static OrderWrapper<T> CreateOrderWrapperAndSetReceiver<T>(IOrderable<UnitGroup> receiverWrapper) where T : Order
        {
            OrderWrapper<T> res = CreateOrderWrapper<T>();

            OrderHandler.AddToGlobalOrderWrapperList(res);

            Order.SetReceiver(res, null, null, receiverWrapper);
            return res;
        }

        public static OrderWrapper<T> CreatePredecessorOrderWrapperAndSetReceiver<T>(IOrderable<UnitGroup> receiverWrapper, OrderWrapper successor) where T : Order
        {
            OrderWrapper<T> res = CreateOrderWrapper<T>();
            
            OrderHandler.AddToGlobalOrderWrapperList(res);

            Order.SetReceiver(res, null, successor, receiverWrapper);
            return res;
        }

        /*private static IOrderable<Unit> GetClosestParentContainedInList(IOrderable<Unit> uwrppr, List<IOrderable<Unit>> list)
        {
            IOrderable<Unit> res = null;
            IOrderable<Unit> u = uwrppr;

            while (u == null || Unit.GetParentWrapper(u.GetOrderableAsReferenceWrapperSpecific<UnitWrapper>()) != null)
            {
                u = Unit.GetParentWrapper(u.GetOrderableAsReferenceWrapperSpecific<UnitWrapper>());
                if(list.Contains(u))
                {
                    res = u;
                    break;
                }
            }
            return res;
        }*/

    }
}