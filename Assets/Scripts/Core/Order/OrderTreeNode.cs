using Gamelogic.Extensions;
using GlobalManagers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VariousUtilsExtensions;

namespace Core.Orders
{
    public class OrderTreeNode : TreeNodeBase<OrderTreeNode>
    {
        public OrderWrapper orderWrapper;

        public override OrderTreeNode MySelf() { return this; }

        public override OrderTreeNode GetParent() { return _parent; }

        protected override void SetParent(OrderTreeNode newParent)
        {
            _parent = newParent;
        }

        public OrderWrapper GetParentWrapper()
        {
            if (GetParent() != null)
                return GetParent().orderWrapper;
            else
                return null;
        }

        public OrderTreeNode(OrderWrapper orderWrapper)
        {
            ChildNodes = new List<OrderTreeNode>();

            this.orderWrapper = orderWrapper;
        }

        public override bool IsLeaf()
        {
            return ChildNodes.Count == 0;
        }

        public override bool IsRoot()
        {
            return GetParent() == null;
        }

        public override List<OrderTreeNode> GetLeafChildren()
        {
            return ChildNodes.Where(x => x.IsLeaf()).ToList();
        }

        public override List<OrderTreeNode> GetNonLeafChildren()
        {
            return ChildNodes.Where(x => !x.IsLeaf()).ToList();
        }

        public override OrderTreeNode GetRootNode()
        {
            if (GetParent() == null)
                return MySelf();

            return GetParent().GetRootNode();
        }

        public OrderWrapper GetRootWrapper()
        {
            return GetRootNode().orderWrapper;
        }

        public override void AddChild(OrderTreeNode child)
        {
            if (child.GetParent() != null)
                child.GetParent().RemoveChild(child);
            child.SetParent(MySelf());
            ChildNodes.Add(child);
        }

        public override void AddChildren(IEnumerable<OrderTreeNode> children)
        {
            foreach (OrderTreeNode child in children)
                AddChild(child);
        }

        public override void RemoveChild(OrderTreeNode child)
        {
            child.SetParent(null);
            ChildNodes.Remove(child);
        }

        public override void RemoveChildren(IEnumerable<OrderTreeNode> children)
        {
            foreach (OrderTreeNode child in children)
                RemoveChild(child);
        }

        public void ChangeParentTo(OrderTreeNode newParent)
        {
            if (newParent != null)
            {
                if (newParent != null)
                    newParent.AddChild(MySelf());
                //unitWrapper.WrappedObject.transform.SetParent(newParent.unitWrapper.WrappedObject.transform);
            }
            else
            {
                if (GetParent() != null)
                    GetParent().RemoveChild(MySelf());
                //unitWrapper.WrappedObject.transform.SetParent(null);
            }
        }

        private Queue<OrderTreeNode> _bfsqueue = new Queue<OrderTreeNode>();
        public List<OrderWrapper> BFSListMeAndAllChildrenWrappers()
        {
            List<OrderWrapper> result = new List<OrderWrapper>();

            _bfsqueue.Enqueue(MySelf());

            while (_bfsqueue.Count > 0)
            {
                result.Add(_bfsqueue.Peek().orderWrapper);

                var a = _bfsqueue.Dequeue().ChildNodes;
                var c = a.Count;
                for (int k = 0; k < c; k++)
                    _bfsqueue.Enqueue(a[k]);

            }

            _bfsqueue.Clear();

            return result;
        }
    }
}