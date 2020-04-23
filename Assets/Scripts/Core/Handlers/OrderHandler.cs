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

        public static bool AddToOrderWrapperList(OrderWrapper wrapper)
        {
            if(!MyInstance.orderWrappersList.Contains(wrapper))
            {
                wrapper.SubscribeOnClearance(() => RemoveFromOrderWrapperList(wrapper));
                MyInstance.orderWrappersList.Add(wrapper);
                return true;
            }
            else
            {
                Debug.LogError("There should be no reason for this to happen");
                return false;
            }
        }

        public static bool RemoveFromOrderWrapperList(OrderWrapper wrapper)
        {
            if(MyInstance.orderWrappersList.Contains(wrapper))
            {
                wrapper.UnsubscribeOnClearance(() => RemoveFromOrderWrapperList(wrapper));
                MyInstance.orderWrappersList.Remove(wrapper);
                return true;
            }
            else
            {
                Debug.LogError("There should be no reason for this to happen");
                return false;
            }
        }


    }
}