using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;
using Core.Orders;

namespace Core.Handlers
{
    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    /// <summary>
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
        
        ///<summary>
        /// This list contains all orders' wrappers that currently exist in the whole scene.
        ///</summary>
        private List<OrderWrapper> orderWrappersList = new List<OrderWrapper>();
        
        private void Awake()
        {
        }

        private void Update()
        {
            // Updating all orders.
            for (int i = orderWrappersList.Count - 1; i >= 0; i--)
            {
                Order.UpdateFSM(orderWrappersList[i]);
                // Add parameter for "delta time" ?
            }
        }

        ///<summary>
        /// This method registers an order's wrapper to the list of all orders' wrappers.
        /// This will allow these orders to be updated. This method is intended to be called typically right after an order's creation, as it is now. (See OrderFactory)
        ///</summary>
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

        private static bool RemoveFromGlobalOrderWrapperList(OrderWrapper wrapper)
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