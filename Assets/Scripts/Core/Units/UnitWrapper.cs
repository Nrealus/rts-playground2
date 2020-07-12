using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nrealus.Extensions;
using UnityEngine;
using System;
using Core.Tasks;
using Core.MapMarkers;
using Core.Handlers;
using Gamelogic.Extensions;
using Core.Selection;
using Nrealus.Extensions.ReferenceWrapper;
using Nrealus.Extensions.Tree;
using Nrealus.Extensions.Observer;

namespace Core.Units
{

    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    ///<summary>
    /// The RefWrapper for Unit.
    /// Follows a tree structure (implements ITreeNodeBase<UnitGroupWrapper>).
    ///</summary>
    public class UnitWrapper : RefWrapper<Unit>, ITreeNodeBase<UnitWrapper>, ITaskSubject, ISelectable
    {

        #region ITaskSubject explicit implementations

        RefWrapper ITaskSubject.GetTaskSubjectAsReferenceWrapperNonGeneric()
        {
            return this;
        }

        Y ITaskSubject.GetTaskSubjectAsReferenceWrapperSpecific<Y>()
        {
            return this as Y;
        }

        #endregion

        #region ISelectable explicit implementations

        private EasyObserver<string, (Selector,bool)> onSelectionStateChange = new EasyObserver<string, (Selector, bool)>();

        public EasyObserver<string, (Selector,bool)> GetOnSelectionStateChangeObserver()
        {
            return onSelectionStateChange;
        }
        
        void ISelectable.InvokeOnSelectionStateChange(Selector selector, bool b)
        {
            onSelectionStateChange.Invoke((selector, b));
        }

        RefWrapper ISelectable.GetSelectableAsReferenceWrapperNonGeneric()
        {
            return this;
        }

        Y ISelectable.GetSelectableAsReferenceWrapperSpecific<Y>()
        {
            return this as Y;
        }

        #endregion
        
        public bool IsWrappedObjectNotNull()
        {
            return GetWrappedReference() != null;
        }

        private TaskPlan _taskPlan;
        public TaskPlan GetTaskPlan()
        {
            return _taskPlan;
        }
    
        public void SetTaskPlan(TaskPlan tp)
        {
            _taskPlan = tp;
        }

        public UnitWrapper(Unit wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
            _taskPlan = new TaskPlan();

            if (!GetWrappedReference().isVirtualUnit)
            {
                UIHandler.GetUIOrdobMenu().AddUnitToOrdob(this);
                _parent.Value = null;
            }

            SubscribeOnClearance(() => 
            {
                GetTaskPlan().Clear();
                _taskPlan = null; 
            });
            
            SubscribeOnClearance(() => 
            {
                if (!GetWrappedReference().isVirtualUnit)
                    UIHandler.GetUIOrdobMenu().RemoveUnitFromOrdob(this);
            });

            _parent.ForceInvokeOnValueChange();

            // PREVIOUS ORDER 
            /*SubscribeOnClearance(() => 
            {
                if (!WrappedObject.isVirtualUnit)
                    UIHandler.GetUIOrderOfBattleMenu().RemoveUnitFromOrderOfBattle(this);

                GetTaskPlan().Clear();
                _taskPlan = null; 
            });*/

            _childNodes = new List<UnitWrapper>();
        }

        #region Tree Structure

        private Dictionary<string,Action> _actDict = new Dictionary<string,Action>();        
        private ObservedValue<UnitWrapper> _parent = new ObservedValue<UnitWrapper>(null);
        
        public void SubscribeOnParentChange(string key, Action action)
        {
            if (!_actDict.ContainsKey(key))
            {
                _actDict.Add(key, action);
                _parent.OnValueChange += action;
            }
        }

        public void UnsubscribeOnParentChange(string key)
        {
            _parent.OnValueChange -= _actDict[key];
        }

        private List<UnitWrapper> _childNodes;

        public UnitWrapper GetThisNode() { return this; }

        public UnitWrapper GetParentNode() { return _parent.Value; }

        public void Internal_SetParentNode(UnitWrapper newParent)
        {
            _parent.Value = newParent;
        }

        public bool IsLeaf()
        {
            return GetChildNodes().Count == 0;
        }

        public bool IsRoot()
        {
            return GetParentNode() == null;
        }

        public List<UnitWrapper> GetChildNodes()
        {
            return _childNodes;
        }
        
        public void SetChildNodes(List<UnitWrapper> childNodes)
        {
            _childNodes = childNodes;
        }

        public List<UnitWrapper> GetLeafChildren()
        {
            return GetChildNodes().Where(x => x.IsLeaf()).ToList();
        }

        public List<UnitWrapper> GetNonLeafChildren()
        {
            return GetChildNodes().Where(x => !x.IsLeaf()).ToList();
        }

        public UnitWrapper GetRootNode()
        {
            if (GetParentNode() == null)
                return GetThisNode();

            return GetParentNode().GetRootNode();
        }

        public void AddChild(UnitWrapper child)
        {
            if (child.GetParentNode() != null)
                child.GetParentNode().RemoveChild(child);
            child.Internal_SetParentNode(GetThisNode());
            GetChildNodes().Add(child);
        }

        public void AddChildren(IEnumerable<UnitWrapper> children)
        {
            foreach (UnitWrapper child in children)
                AddChild(child);
        }

        public void RemoveChild(UnitWrapper child)
        {
            child.Internal_SetParentNode(null);
            GetChildNodes().Remove(child);
        }

        public void RemoveChildren(IEnumerable<UnitWrapper> children)
        {
            foreach (UnitWrapper child in children)
                RemoveChild(child);
        }

        public void ChangeParentTo(UnitWrapper newParent)
        {
            if (newParent != null)
            {
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
        
        private Queue<UnitWrapper> _bfsqueue = new Queue<UnitWrapper>();
        public List<UnitWrapper> BFSList()
        {
            List<UnitWrapper> result = new List<UnitWrapper>();

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