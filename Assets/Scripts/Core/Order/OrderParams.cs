using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;

namespace Core.Orders
{
    public class OrderParams
    {
        
        public enum OrderExecutionMode
        {
            InstantOverrideAll,
            Chain,
            WaitForReactionAtEnd,
            AskForConfirmationRightBeforeStart,
            
        }

        private List<OrderExecutionMode> _executionMode = new List<OrderExecutionMode>();

        public bool ContainsExecutionMode(OrderExecutionMode mode)
        {
            return _executionMode.Contains(mode);
        }

        public void AddExecutionMode(OrderExecutionMode mode) // Subject to change when this class will get more formal
        {
            _executionMode.Add(mode);
        }

        public TimeStruct startingTime { get; /*private*/ set; }

        public bool isPassive { get; /*private*/ set; }

        
        public static OrderParams DefaultParam() { return new OrderParams(); }

        public static OrderParams PassiveParam()
        { 
            var res = new OrderParams();
            res.isPassive = true;
            return res;
        }


        
        public OrderParams()
        {
            
        }

    }
}