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
}