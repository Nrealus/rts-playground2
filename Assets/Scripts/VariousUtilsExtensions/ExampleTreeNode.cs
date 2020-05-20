using Core.Units;
using System.Collections.Generic;
using System.Linq;
using VariousUtilsExtensions;

//------------------------------------------------------------------------------
/// <summary>
/// 
/// </summary>
//------------------------------------------------------------------------------
/*public class ExampleTreeNode<T> : ITreeNodeBase<ExampleTreeNode<T>>
{

    private ReferenceWrapper<T> exampleWrapper;

    public string WrappedObjectName()
    {
        return "name";
    }

    public override ExampleTreeNode<T> GetThisNode() { return this; }

    public override ExampleTreeNode<T> GetParentNode() { return _parent; }

    protected override void SetParentNode(ExampleTreeNode<T> newParent)
    {
        _parent = newParent;
    }

    public ExampleTreeNode(ReferenceWrapper<T> exampleWrapper)
    {
        ChildNodes = new List<ExampleTreeNode<T>>();

        this.exampleWrapper = exampleWrapper;
    }

    public override bool IsLeaf()
    {
        return ChildNodes.Count == 0;
    }

    public override bool IsRoot()
    {
        return GetParentNode() == null;
    }

    public override List<ExampleTreeNode<T>> GetLeafChildren()
    {
        return ChildNodes.Where(x => x.IsLeaf()).ToList();
    }

    public override List<ExampleTreeNode<T>> GetNonLeafChildren()
    {
        return ChildNodes.Where(x => !x.IsLeaf()).ToList();
    }

    public override ExampleTreeNode<T> GetRootNode()
    {
        if (GetParentNode() == null)
            return GetThisNode();

        return GetParentNode().GetRootNode();
    }

    public override void AddChild(ExampleTreeNode<T> child)
    {
        if (child.GetParentNode() != null)
            child.GetParentNode().RemoveChild(child);
        child.SetParentNode(GetThisNode());
        ChildNodes.Add(child);
    }

    public override void AddChildren(IEnumerable<ExampleTreeNode<T>> children)
    {
        foreach (ExampleTreeNode<T> child in children)
            AddChild(child);
    }

    public override void RemoveChild(ExampleTreeNode<T> child)
    {
        child.SetParentNode(null);
        ChildNodes.Remove(child);
    }

    public override void RemoveChildren(IEnumerable<ExampleTreeNode<T>> children)
    {
        foreach (ExampleTreeNode<T> child in children)
            RemoveChild(child);
    }

    public void ChangeParentTo(ExampleTreeNode<T> newParent)
    {
        if(newParent != null)
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

    public List<ExampleTreeNode<T>> BreadthFirstSearchAllChildrenList()
    {
        List<ExampleTreeNode<T>> result = new List<ExampleTreeNode<T>>();
        int i = 0;
        int c = 1;
        List<ExampleTreeNode<T>> temp = new List<ExampleTreeNode<T>>();

        result.Add(GetThisNode());

        while (i<c)
        {
            temp.Clear();
            temp.AddRange(result[i].ChildNodes);
            result.AddRange(temp);

            c += temp.Count;
            i++;
        }

        return result;
    }
    

}*/