using UnityEngine;
using System.Collections.Generic;
using System;
using Core.Units;
using VariousUtilsExtensions;

namespace Core.Orders
{
    public static class OrderFactory {
        
        public static OrderGroupWrapper CreateOrderGroup()
        {
            return (new OrderGroup()).GetMyWrapper();
        }

        private static OrderWrapper<T> CreateOrderWrapper<T>() where T : Order
        {
            switch (typeof(T))
            {
                case Type moType when moType == typeof(MoveOrder):
                {
                    MoveOrder mo = new MoveOrder();
                    OrderWrapper<MoveOrder> wrapper = mo.GetMyWrapper<MoveOrder>();
                    return wrapper as OrderWrapper<T>;
                }
                case Type moType when moType == typeof(BuildOrder):
                {
                    BuildOrder mo = new BuildOrder();
                    OrderWrapper<BuildOrder> wrapper = mo.GetMyWrapper<BuildOrder>();
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

        public static OrderWrapper<T> CreateOrderWrapperAndSetReceiver<T>(IOrderable<Unit> receiverWrapper) where T : Order
        {
            OrderWrapper<T> res = CreateOrderWrapper<T>();
            Order.SetReceiver(res, receiverWrapper);
            return res;
        }

        private static IOrderable<Unit> GetClosestParentContainedInList(IOrderable<Unit> uwrppr, List<IOrderable<Unit>> list)
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
        }

    }
}