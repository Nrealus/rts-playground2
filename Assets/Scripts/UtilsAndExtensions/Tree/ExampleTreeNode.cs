using System.Collections.Generic;
using System.Linq;
using Nrealus.Extensions;

namespace Nrealus.Extensions.Tree
{
    /// <summary>
    /// An example of implementation of the ITreeNodeBase interface.
    /// </summary>
    /// <typeparam name="T">The type of the arbitrary example "obj" data that the nodes hold.</typeparam>
    public class SimpleTreeNode<T> : ITreeNodeBase<SimpleTreeNode<T>>
    {

        private SimpleTreeNode<T> _parent;

        private List<SimpleTreeNode<T>> _childNodes;


        public T obj { get; private set; }

        public SimpleTreeNode<T> GetThisNode() { return this; }

        public SimpleTreeNode<T> GetParentNode() { return _parent; }

        public void Internal_SetParentNode(SimpleTreeNode<T> newParent)
        {
            _parent = newParent;
        }

        public SimpleTreeNode(T obj)
        {
            _childNodes = new List<SimpleTreeNode<T>>();

            this.obj = obj;
        }

        public bool IsLeaf()
        {
            return GetChildNodes().Count == 0;
        }

        public bool IsRoot()
        {
            return GetParentNode() == null;
        }

        public List<SimpleTreeNode<T>> GetChildNodes()
        {
            return _childNodes;
        }
        
        public void SetChildNodes(List<SimpleTreeNode<T>> childNodes)
        {
            _childNodes = childNodes;
        }

        public List<SimpleTreeNode<T>> GetLeafChildren()
        {
            return GetChildNodes().Where(x => x.IsLeaf()).ToList();
        }

        public List<SimpleTreeNode<T>> GetNonLeafChildren()
        {
            return GetChildNodes().Where(x => !x.IsLeaf()).ToList();
        }

        public SimpleTreeNode<T> GetRootNode()
        {
            if (GetParentNode() == null)
                return GetThisNode();

            return GetParentNode().GetRootNode();
        }

        public void AddChild(SimpleTreeNode<T> child)
        {
            if (child.GetParentNode() != null)
                child.GetParentNode().RemoveChild(child);
            child.Internal_SetParentNode(GetThisNode());
            GetChildNodes().Add(child);
        }

        public void AddChildren(IEnumerable<SimpleTreeNode<T>> children)
        {
            foreach (SimpleTreeNode<T> child in children)
                AddChild(child);
        }

        public void RemoveChild(SimpleTreeNode<T> child)
        {
            child.Internal_SetParentNode(null);
            GetChildNodes().Remove(child);
        }

        public void RemoveChildren(IEnumerable<SimpleTreeNode<T>> children)
        {
            foreach (SimpleTreeNode<T> child in children)
                RemoveChild(child);
        }

        public void ChangeParentTo(SimpleTreeNode<T> newParent)
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
        
        private Queue<SimpleTreeNode<T>> _bfsqueue = new Queue<SimpleTreeNode<T>>();
        public List<T> BFSList()
        {
            List<T> result = new List<T>();

            _bfsqueue.Enqueue(GetThisNode());

            while (_bfsqueue.Count > 0)
            {
                result.Add(_bfsqueue.Peek().obj);

                var a = _bfsqueue.Dequeue().GetChildNodes();
                var c = a.Count;
                for (int k = 0; k < c; k++)
                    _bfsqueue.Enqueue(a[k]);

            }

            _bfsqueue.Clear();

            return result;
        }

        public SimpleTreeNode<T> BFSFindNode(T toFind)
        {
            _bfsqueue.Enqueue(GetThisNode());

            while (_bfsqueue.Count > 0)
            {
                if (_bfsqueue.Peek().obj.Equals(toFind))
                    return _bfsqueue.Peek();

                var a = _bfsqueue.Dequeue().GetChildNodes();
                var c = a.Count;
                for (int k = 0; k < c; k++)
                    _bfsqueue.Enqueue(a[k]);
            }

            _bfsqueue.Clear();

            return null;
        }

    }
}