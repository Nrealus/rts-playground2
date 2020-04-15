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
        
        private static OrderHandler _instance;
        private static OrderHandler MyInstance
        {
            get
            {
                if(_instance == null)
                    _instance = FindObjectOfType<OrderHandler>(); 
                return _instance;
            }
        }

        private List<OrderWrapper> orderWrappersList = new List<OrderWrapper>();
        
        private void Awake()
        {
        }


        private void Update()
        {
            //ordersBTClock.Update(Time.deltaTime);
            for (int i = orderWrappersList.Count - 1; i >= 0; i--)
            {
                Order.UpdateFSM(orderWrappersList[i]);
            }
        }

        public static bool IsOrderWrapperRegistered(OrderWrapper wrapper)
        {
            return MyInstance.orderWrappersList.Contains(wrapper);                
        }

        public static bool AddToOrderWrapperList(OrderWrapper wrapper)
        {
            if(!IsOrderWrapperRegistered(wrapper))
            {
                MyInstance.orderWrappersList.Add(wrapper);
                return true;
            }
            else
                return false;
        }

        public static bool RemoveFromOrderWrapperList(OrderWrapper wrapper)
        {
            if(IsOrderWrapperRegistered(wrapper))
            {
                MyInstance.orderWrappersList.Remove(wrapper);
                return true;
            }
            else
            {
                return false;
            }
        }


    }
}