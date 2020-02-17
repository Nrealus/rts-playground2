using UnityEngine;
using System.Collections.Generic;
using System;
using Core.Units;
using VariousUtilsExtensions;

namespace Core.Orders
{
    public class OrderFactory {
        
        public OrderWrapper<T> CreateOrder<T>() where T : Order
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

        public OrderWrapper<T> CreateOrderAndSetReceiver<T>(IOrderable<Unit> receiverWrapper) where T : Order
        {
            OrderWrapper<T> res = CreateOrder<T>();
            res.SetOrderReceiver(receiverWrapper);
            return res;
        }

        public List<OrderWrapper<T>> CreateOrdersAndSetReceiversHierarchy<T>(List<IOrderable<Unit>> receiverWrappers) where T : Order
        {
            List<OrderWrapper<T>> result = new List<OrderWrapper<T>>();
            List<IOrderable<Unit>> visited  = new List<IOrderable<Unit>>();

            int cc = receiverWrappers.Count;
            for(int i = 0; i < cc; i++)
            {
                IOrderable<Unit> parent = GetClosestParentContainedInList(receiverWrappers[i], receiverWrappers);
                if(parent != null)
                {
                    OrderWrapper<T> r = null;
                    OrderWrapper<T> parentWrapper = null;
                    if (!visited.Contains(receiverWrappers[i]))
                    {
                        r = CreateOrderAndSetReceiver<T>(receiverWrappers[i]);
                        result.Add(r);
                        visited.Add(receiverWrappers[i]);
                    }
                    else
                    {
                        foreach(OrderWrapper<T> vv in result)
                        {
                            if(((IOrderable<Unit>)vv.GetOrderReceiver()) == receiverWrappers[i])
                            {
                                r = vv;
                                break;
                            }
                        }
                    }

                    bool b = false;
                    foreach (var vv in result)
                    {
                        if(vv != r && ((IOrderable<Unit>)vv.GetOrderReceiver()) == parent)
                        {
                            b = true;
                            break;
                        }
                    }
                    if (!b)
                    {
                        Debug.Log("ffffff");
                        parentWrapper = CreateOrderAndSetReceiver<T>((IOrderable<Unit>)parent);
                        result.Add(parentWrapper);
                        visited.Add(parent);
                    }

                    if(parentWrapper != null)
                        r.WrappedObject.SetParentOrder(parentWrapper.WrappedObject);
                }
                else
                {
                    var r = CreateOrderAndSetReceiver<T>(receiverWrappers[i]);
                    result.Add(r);                        
                    visited.Add(receiverWrappers[i]);
                }
            }

            return result;
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