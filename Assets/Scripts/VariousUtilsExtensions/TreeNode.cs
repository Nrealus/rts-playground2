using System.Collections.Generic;
using System.Linq;

namespace VariousUtilsExtensions
{

    //------------------------------------------------------------------------------
    /// <summary>
    /// Generic Tree Node base class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    //------------------------------------------------------------------------------
    public abstract class TreeNodeBase<T> where T : TreeNodeBase<T>
    {

        public abstract T MySelf();

        protected T _parent;

        public abstract T GetParent();

        protected abstract void SetParent(T newParent);

        public List<T> ChildNodes { get; protected set; }

        public abstract bool IsLeaf();

        public abstract bool IsRoot();

        public abstract List<T> GetLeafChildren();

        public abstract List<T> GetNonLeafChildren();

        public abstract T GetRootNode();

        public abstract void AddChild(T child);

        public abstract void AddChildren(IEnumerable<T> children);

        public abstract void RemoveChild(T child);

        public abstract void RemoveChildren(IEnumerable<T> children);

    }

    public class SimpleTreeNode<T> : TreeNodeBase<SimpleTreeNode<T>>
    {
        public T obj { get; private set; }

        public override SimpleTreeNode<T> MySelf() { return this; }

        public override SimpleTreeNode<T> GetParent() { return _parent; }

        protected override void SetParent(SimpleTreeNode<T> newParent)
        {
            _parent = newParent;
        }

        public SimpleTreeNode(T obj)
        {
            ChildNodes = new List<SimpleTreeNode<T>>();

            this.obj = obj;
        }

        public override bool IsLeaf()
        {
            return ChildNodes.Count == 0;
        }

        public override bool IsRoot()
        {
            return GetParent() == null;
        }

        public override List<SimpleTreeNode<T>> GetLeafChildren()
        {
            return ChildNodes.Where(x => x.IsLeaf()).ToList();
        }

        public override List<SimpleTreeNode<T>> GetNonLeafChildren()
        {
            return ChildNodes.Where(x => !x.IsLeaf()).ToList();
        }

        public override SimpleTreeNode<T> GetRootNode()
        {
            if (GetParent() == null)
                return MySelf();

            return GetParent().GetRootNode();
        }

        public override void AddChild(SimpleTreeNode<T> child)
        {
            if (child.GetParent() != null)
                child.GetParent().RemoveChild(child);
            child.SetParent(MySelf());
            ChildNodes.Add(child);
        }

        public override void AddChildren(IEnumerable<SimpleTreeNode<T>> children)
        {
            foreach (SimpleTreeNode<T> child in children)
                AddChild(child);
        }

        public override void RemoveChild(SimpleTreeNode<T> child)
        {
            child.SetParent(null);
            ChildNodes.Remove(child);
        }

        public override void RemoveChildren(IEnumerable<SimpleTreeNode<T>> children)
        {
            foreach (SimpleTreeNode<T> child in children)
                RemoveChild(child);
        }

        public void ChangeParentTo(SimpleTreeNode<T> newParent)
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
        
        private Queue<SimpleTreeNode<T>> _bfsqueue = new Queue<SimpleTreeNode<T>>();
        public List<T> BFSList()
        {
            List<T> result = new List<T>();

            _bfsqueue.Enqueue(MySelf());

            while (_bfsqueue.Count > 0)
            {
                result.Add(_bfsqueue.Peek().obj);

                var a = _bfsqueue.Dequeue().ChildNodes;
                var c = a.Count;
                for (int k = 0; k < c; k++)
                    _bfsqueue.Enqueue(a[k]);

            }

            _bfsqueue.Clear();

            return result;
        }

        public SimpleTreeNode<T> BFSFindNode(T toFind)
        {
            _bfsqueue.Enqueue(MySelf());

            while (_bfsqueue.Count > 0)
            {
                if (_bfsqueue.Peek().obj.Equals(toFind))
                    return _bfsqueue.Peek();

                var a = _bfsqueue.Dequeue().ChildNodes;
                var c = a.Count;
                for (int k = 0; k < c; k++)
                    _bfsqueue.Enqueue(a[k]);
            }

            _bfsqueue.Clear();

            return null;
        }

    }


}