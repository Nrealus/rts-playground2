using System.Collections.Generic;
using System.Linq;

namespace Nrealus.Extensions.Tree
{

    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    /// <summary>
    /// This is an interface which allows us to implement a recursive tree structure for a class.
    /// (See SimpleTreeNode<T> for a default example of an implementation of this interface)
    /// </summary>
    /// <typeparam name="T">The type for the nodes of the tree. They must implement ITreeNodeBase<T> too.</typeparam>
    public interface ITreeNodeBase<T> where T : ITreeNodeBase<T>
    {

        T GetThisNode();

        T GetParentNode();

        void Internal_SetParentNode(T newParent);

        bool IsLeaf();

        bool IsRoot();

        List<T> GetChildNodes();
        
        void SetChildNodes(List<T> nodes);
        
        List<T> GetLeafChildren();

        List<T> GetNonLeafChildren();

        T GetRootNode();

        void AddChild(T child);

        void AddChildren(IEnumerable<T> children);

        void RemoveChild(T child);

        void RemoveChildren(IEnumerable<T> children);

    }

}