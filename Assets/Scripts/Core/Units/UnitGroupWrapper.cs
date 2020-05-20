using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VariousUtilsExtensions;
using UnityEngine;
using System;
using Core.Orders;
using Core.MapMarkers;

namespace Core.Units
{

    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    ///<summary>
    /// The RefWrapper for UnitGroup.
    /// Follows a tree structure (implements ITreeNodeBase<UnitGroupWrapper>). For now, its possible inteded usage is to have groups of groups, or formations of formations.
    ///</summary>
    public class UnitGroupWrapper : RefWrapper<UnitGroup>, ITreeNodeBase<UnitGroupWrapper>, IOrderable<UnitGroup>
    {

        #region IOrderables explicit implementations

        RefWrapper IOrderable.GetOrderableAsReferenceWrapperNonGeneric()
        {
            return this;
        }

        RefWrapper<UnitGroup> IOrderable<UnitGroup>.GetOrderableAsReferenceWrapperGeneric()
        {
            return this;
        }

        Y IOrderable.GetOrderableAsReferenceWrapperSpecific<Y>()
        {
            return this as Y;
        }

        #endregion

        private OrderPlan _ordersPlan;
        public OrderPlan GetOrdersPlan()
        {
            return _ordersPlan;
        }

        public bool IsWrappedObjectNotNull()
        {
            return WrappedObject != null;
        }
    
        public UnitGroupWrapper(UnitGroup wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
            _ordersPlan = new OrderPlan();

            SubscribeOnClearance(() => { GetOrdersPlan().Clear(); _ordersPlan = null; });

            _childNodes = new List<UnitGroupWrapper>();
        }

        public T GetWrappedAs<T>() where T : UnitGroup
        {
            return WrappedObject as T;
        }

        #region Tree Structure

        private UnitGroupWrapper _parent;

        private List<UnitGroupWrapper> _childNodes;

        public UnitGroupWrapper GetThisNode() { return this; }

        public UnitGroupWrapper GetParentNode() { return _parent; }

        public void SetParentNode(UnitGroupWrapper newParent)
        {
            _parent = newParent;
        }

        public bool IsLeaf()
        {
            return GetChildNodes().Count == 0;
        }

        public bool IsRoot()
        {
            return GetParentNode() == null;
        }

        public List<UnitGroupWrapper> GetChildNodes()
        {
            return _childNodes;
        }
        
        public void SetChildNodes(List<UnitGroupWrapper> childNodes)
        {
            _childNodes = childNodes;
        }

        public List<UnitGroupWrapper> GetLeafChildren()
        {
            return GetChildNodes().Where(x => x.IsLeaf()).ToList();
        }

        public List<UnitGroupWrapper> GetNonLeafChildren()
        {
            return GetChildNodes().Where(x => !x.IsLeaf()).ToList();
        }

        public UnitGroupWrapper GetRootNode()
        {
            if (GetParentNode() == null)
                return GetThisNode();

            return GetParentNode().GetRootNode();
        }

        public void AddChild(UnitGroupWrapper child)
        {
            if (child.GetParentNode() != null)
                child.GetParentNode().RemoveChild(child);
            child.SetParentNode(GetThisNode());
            GetChildNodes().Add(child);
        }

        public void AddChildren(IEnumerable<UnitGroupWrapper> children)
        {
            foreach (UnitGroupWrapper child in children)
                AddChild(child);
        }

        public void RemoveChild(UnitGroupWrapper child)
        {
            child.SetParentNode(null);
            GetChildNodes().Remove(child);
        }

        public void RemoveChildren(IEnumerable<UnitGroupWrapper> children)
        {
            foreach (UnitGroupWrapper child in children)
                RemoveChild(child);
        }

        public void ChangeParentTo(UnitGroupWrapper newParent)
        {
            if (newParent != null)
            {
                if (newParent != null)
                    newParent.AddChild(GetThisNode());
                //unitWrapper.WrappedObject.transform.SetParent(newParent.unitWrapper.WrappedObject.transform);
            }
            else
            {
                if (GetParentNode() != null)
                    GetParentNode().RemoveChild(GetThisNode());
                //unitWrapper.WrappedObject.transform.SetParent(null);
            }
        }
        
        private Queue<UnitGroupWrapper> _bfsqueue = new Queue<UnitGroupWrapper>();
        public List<UnitGroupWrapper> BFSList()
        {
            List<UnitGroupWrapper> result = new List<UnitGroupWrapper>();

            _bfsqueue.Enqueue(GetThisNode());

            while (_bfsqueue.Count > 0)
            {
                result.Add(_bfsqueue.Peek().GetThisNode());

                var a = _bfsqueue.Dequeue().GetChildNodes();
                var c = a.Count;
                for (int k = 0; k < c; k++)
                    _bfsqueue.Enqueue(a[k]);

            }

            _bfsqueue.Clear();

            return result;
        }

        #endregion

    }

}