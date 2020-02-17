using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Orders
{
    public class OrderParameters
    {
        //public IOrderInteracter<Unit> orderGiver;
        private List<object> args = new List<object>();

        public object[] GetArgs()
        {
            return args.ToArray();
        }
    }
}
