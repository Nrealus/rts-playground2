using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;

namespace Core.Orders
{
    public class OrderParams
    {
        
        public TimeStruct startingTime;

        /*public static OrderParams DefaultParams
        { 
            get 
            {
                var res = new OrderParams();
                res.startingTime = new TimeStruct(9,15,21);
                return res;
            } 
        }*/

        public static OrderParams DefaultParams = new OrderParams();

        public OrderParams()
        {
            
        }

    }
}