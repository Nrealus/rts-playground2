using System;
using UnityEngine.EventSystems;
using Nrealus.Extensions;
using Core.Units;
using Nrealus.Extensions.Observer;
using UnityEngine;

namespace Core.UI
{
    /****** Author : nrealus ****** Last documentation update : 12-07-2020 ******/

    /// <summary>
    /// Component for UI objects that represent an element of the tree view Order of Battle panel. Subclass of TreeViewElement.
    /// </summary>   
    public class UIOrdobTreeViewElement : TreeViewElement
    {

        private UnitWrapper _associatedUnitWrapper;
        public Unit associatedUnit { get { return _associatedUnitWrapper.Value; } }

        private ITreeViewSelectable _associatedTreeViewSelectable;
        public override ITreeViewSelectable GetAssociatedTreeViewSelectable()
        {
            return _associatedTreeViewSelectable;
        }

        public override void OnDestroyMe()
        {
            onUnbind?.Invoke();
            onUnbind = null;
        }
        
        protected override void Awake()
        {
            base.Awake();

            _associatedTreeViewSelectable = GetComponentInChildren<ITreeViewSelectable>();
            _associatedTreeViewSelectable.SetAssociatedTreeViewElement(this);
        }

        public void Init(Unit unit, RectTransform dragArea, RectTransform containerOfWholeTreeView)
        {
            this.dragArea = dragArea;
            this.containerOfWholeTreeView = containerOfWholeTreeView;
            _associatedUnitWrapper = new UnitWrapper(unit);
        }

        #region ssdfsdfs

        public override void OnDragHoverOn(TreeViewElement hoveredPotentialAttachElements, TreeViewElement previousHoveredPotentialAttachElements)
        {
            if (previousHoveredPotentialAttachElements != null)
                previousHoveredPotentialAttachElements.GetAssociatedTreeViewSelectable().Unhighlight();

            if (hoveredPotentialAttachElements != null)
                hoveredPotentialAttachElements.GetAssociatedTreeViewSelectable().Highlight();
        }

        public override void OnAttachTo(TreeViewElement newParent)
        {
            if (newParent != null)
            {
                newParent.GetAssociatedTreeViewSelectable().Unhighlight();
                associatedUnit.ChangeParentTo((newParent as UIOrdobTreeViewElement).associatedUnit);
            }                
            else
                associatedUnit.ChangeParentTo(null);
        }

        public override void OnPressInOrOut(bool inOrOut, PointerEventData data)
        {
            if (inOrOut)
            {
                GetAssociatedTreeViewSelectable().Select();
                onPressedInBind.Invoke(true);
            }
            else
            {
                if (data != null && data.hovered.Contains(GetAssociatedTreeViewSelectable().GetGameObject()))
                    GetAssociatedTreeViewSelectable().Highlight();
                else
                    GetAssociatedTreeViewSelectable().Deselect();
                onPressedInBind.Invoke(false);
            }
        }

        #endregion

        private EasyObserver<string,bool> onPressedInBind = new EasyObserver<string, bool>();
        private event Action onUnbind;

        public void BindPressEvent(MultiEventObserver binder, Action<object, EventArgs> action)
        {
            var id = binder.AddNewEventAndSubscribeMethodToIt(action);
            onPressedInBind.SubscribeEventHandlerMethod("bindpress",
                (_) => binder.InvokeEvent(id, this, new SimpleEventArgs(_)));
            onUnbind += () => UnbindPressEvent(binder, id);
        }

        private void UnbindPressEvent(MultiEventObserver binder, int sourceId)
        {
            onPressedInBind.UnsubscribeEventHandlerMethod("bindpress");
        }

    }
}