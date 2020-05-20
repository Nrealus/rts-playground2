using System.Collections.Generic;
using System.Linq;
using VariousUtilsExtensions;

//------------------------------------------------------------------------------
/// <summary>
/// 
/// </summary>
//------------------------------------------------------------------------------
/*namespace Core.Units
{
    public class UnitTreeNode : TreeNodeBase<UnitTreeNode>
    {

        private UnitWrapper unitWrapper;

        public override UnitTreeNode MySelf() { return this; }

        public override UnitTreeNode GetParent() { return _parent; }

        protected override void SetParent(UnitTreeNode newParent)
        {
            _parent = newParent;
        }

        public UnitWrapper GetParentWrapper()
        {
            if (GetParent() != null)
                return GetParent().unitWrapper;
            else
                return null;
        }

        public UnitTreeNode(UnitWrapper unitWrapper)
        {
            ChildNodes = new List<UnitTreeNode>();

            this.unitWrapper = unitWrapper;
        }

        public override bool IsLeaf()
        {
            return ChildNodes.Count == 0;
        }

        public override bool IsRoot()
        {
            return GetParent() == null;
        }

        public override List<UnitTreeNode> GetLeafChildren()
        {
            return ChildNodes.Where(x => x.IsLeaf()).ToList();
        }

        public override List<UnitTreeNode> GetNonLeafChildren()
        {
            return ChildNodes.Where(x => !x.IsLeaf()).ToList();
        }

        public override UnitTreeNode GetRootNode()
        {
            if (GetParent() == null)
                return MySelf();

            return GetParent().GetRootNode();
        }

        public override void AddChild(UnitTreeNode child)
        {
            if (child.GetParent() != null)
                child.GetParent().RemoveChild(child);
            child.SetParent(MySelf());
            ChildNodes.Add(child);
        }

        public override void AddChildren(IEnumerable<UnitTreeNode> children)
        {
            foreach (UnitTreeNode child in children)
                AddChild(child);
        }

        public override void RemoveChild(UnitTreeNode child)
        {
            child.SetParent(null);
            ChildNodes.Remove(child);
        }

        public override void RemoveChildren(IEnumerable<UnitTreeNode> children)
        {
            foreach (UnitTreeNode child in children)
                RemoveChild(child);
        }

        public void ChangeParentTo(UnitTreeNode newParent)
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

        private Queue<UnitTreeNode> _bfsqueue = new Queue<UnitTreeNode>();
        public List<UnitWrapper> BFSListMeAndAllChildrenWrappers()
        {
            List<UnitWrapper> result = new List<UnitWrapper>();

            _bfsqueue.Enqueue(MySelf());

            while (_bfsqueue.Count > 0)
            {
                result.Add(_bfsqueue.Peek().unitWrapper);

                var a = _bfsqueue.Dequeue().ChildNodes;
                var c = a.Count;
                for (int k = 0; k < c; k++)
                    _bfsqueue.Enqueue(a[k]);

            }

            _bfsqueue.Clear();

            return result;
        }

        public List<UnitWrapper> ListChildrenWrappers()
        {
            List<UnitWrapper> res = new List<UnitWrapper>();
            foreach (var v in ChildNodes)
            {
                res.Add(v.unitWrapper);
            }
            return res;
        }

    }
}*/