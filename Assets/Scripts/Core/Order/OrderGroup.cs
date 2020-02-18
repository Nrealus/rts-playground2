using System.Collections;
using System.Collections.Generic;
using Core.Helpers;
using UnityEngine;

namespace Core.Orders
{
    public class OrderGroup :
        IHasRefWrapper<OrderGroupWrapper>
    {

        protected OrderGroupWrapper _myWrapper;
        public OrderGroupWrapper GetMyWrapper() { return _myWrapper; }
        //public OrderGroupWrapper<T> GetMyWrapper<T>() where T : Order { return _myWrapper as OrderWrapper<T>; }

        public void ClearWrapper()
        {
            GetMyWrapper().DestroyWrappedReference();
            _myWrapper = null;
        }

        public OrderGroup()
        {
            _myWrapper = new OrderGroupWrapper(this);
        }

    }
}