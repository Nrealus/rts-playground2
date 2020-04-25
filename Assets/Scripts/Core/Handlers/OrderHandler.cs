using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;
using Core.Orders;

namespace Core.Handlers
{
    /// <summary>
    /// ---- General Description, by nrealus, last update : 23-04-2020 ----
    ///
    /// Singleton registering all Orders (or rather, OrderWrappers) - they are added to this singleton's list when created and and given a receiver, using the OrderFactory class.
    /// Of course, they are unregistered from the list when they are cleared.
    /// For now, its only use is to update all the Orders (OrderWrappers) in the game loop, by updating their main finite state machine.
    /// </summary>    
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

        public static bool AddToGlobalOrderWrapperList(OrderWrapper wrapper)
        {
            if(!MyInstance.orderWrappersList.Contains(wrapper))
            {
                wrapper.SubscribeOnClearance(() => RemoveFromGlobalOrderWrapperList(wrapper));
                MyInstance.orderWrappersList.Add(wrapper);
                return true;
            }
            else
            {
                Debug.LogError("There should be no reason for this to happen");
                return false;
            }
        }

        public static bool RemoveFromGlobalOrderWrapperList(OrderWrapper wrapper)
        {
            if(MyInstance.orderWrappersList.Contains(wrapper))
            {
                wrapper.UnsubscribeOnClearance(() => RemoveFromGlobalOrderWrapperList(wrapper));
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