using UnityEngine;
using System.Collections.Generic;
using System;
using Core.Units;
using VariousUtilsExtensions;

namespace Core.Orders
{
    public class OrderFactory {
        
        public OrderGroupWrapper CreateOrderGroup()
        {
            return (new OrderGroup()).GetMyWrapper();
        }

        public OrderWrapper<T> CreateOrderwrapper<T>() where T : Order
        {
            switch (typeof(T))
            {
                case Type moType when moType == typeof(MoveOrder):
                {
                    MoveOrder mo = new MoveOrder();
                    OrderWrapper<MoveOrder> wrapper = mo.GetMyWrapper<MoveOrder>();
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

        public OrderWrapper<T> CreateOrderWrapperAndSetReceiver<T>(IOrderable<Unit> receiverWrapper) where T : Order
        {
            OrderWrapper<T> res = CreateOrderwrapper<T>();
            res.SetOrderReceiver(receiverWrapper);
            return res;
        }

        private IOrderable<Unit> GetClosestParentContainedInList(IOrderable<Unit> uwrppr, List<IOrderable<Unit>> list)
        {
            IOrderable<Unit> res = null;
            IOrderable<Unit> u = uwrppr;

            while (u == null || u.GetMyReferenceWrapperGeneric().WrappedObject.GetParentWrapper() != null)
            {
                u = u.GetMyReferenceWrapperGeneric().WrappedObject.GetParentWrapper();
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