using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;
using Core.Orders;

namespace Core.Handlers
{
    public class OrderHandler : MonoBehaviour
    {

        //public NPBehave.Clock ordersBTClock;

        //plebean implementation, this is actually a forest
        private List<OrderWrapper> orderWrappersList = new List<OrderWrapper>();
        //public List<Order> disposeOrdersList;

        private void Awake()
        {
            //ordersBTClock = new NPBehave.Clock();
            //ordersList = new List<Order>();
            //disposeOrdersList = new List<Order>();
        }


        private void Update()
        {
            //ordersBTClock.Update(Time.deltaTime);
            for (int i = orderWrappersList.Count - 1; i >= 0; i--)
            {
                orderWrappersList[i].UpdateOrderPhaseFSM();
            }
        }

        public bool OrderWrapperRegistered(OrderWrapper wrapper)
        {
            return orderWrappersList.Contains(wrapper);                
        }

        public bool AddToOrderWrapperList(OrderWrapper wrapper)
        {
            if(!OrderWrapperRegistered(wrapper))
            {
                orderWrappersList.Add(wrapper);
                return true;
            }
            else
                return false;
        }

        public bool RemoveFromOrderWrapperList(OrderWrapper wrapper)
        {
            if(OrderWrapperRegistered(wrapper))
            {
                orderWrappersList.Remove(wrapper);
                return true;
            }
            else
            {
                return false;
            }
        }


    }
}