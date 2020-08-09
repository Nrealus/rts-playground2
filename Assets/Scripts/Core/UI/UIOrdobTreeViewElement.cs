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
        private ITreeViewSelectable _associatedTreeViewSelectable;

        private EasyObserver<string,bool> onPressedInBind = new EasyObserver<string, bool>();
        private event Action onUnbind;

        public Unit GetAssociatedUnit() { return _associatedUnitWrapper?.Value; }

        public override ITreeViewSelectable GetAssociatedTreeViewSelectable()
        {
            return _associatedTreeViewSelectable;
        }
        
        #region Initialisation and OnDestruction

        public void Init(Unit unit, RectTransform dragArea, RectTransform containerOfWholeTreeView)
        {
            this.dragArea = dragArea;
            this.containerOfWholeTreeView = containerOfWholeTreeView;
            _associatedUnitWrapper = new UnitWrapper(unit);
        }

        protected override void Awake()
        {
            base.Awake();

            _associatedTreeViewSelectable = GetComponentInChildren<ITreeViewSelectable>();
            _associatedTreeViewSelectable.SetAssociatedTreeViewElement(this);
        }

        public override void OnDestruction()
        {
            onUnbind?.Invoke();
            onUnbind = null;
        }

        #endregion

        #region TreeViewElement overriden event callbacks

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
                if (!newParent.isRoot)
                    GetAssociatedUnit().ChangeParentTo((newParent as UIOrdobTreeViewElement).GetAssociatedUnit());
                else
                    GetAssociatedUnit().ChangeParentTo(null);
            }
            else
                GetAssociatedUnit().ChangeParentTo(GetAssociatedUnit().GetParentActorAsUnit());
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

        public void BindPressEvent(MultiEventObserver binder, Action<object, EventArgs> action)
        {
            var id = binder.AddNewEventAndSubscribeMethodToIt(action);
            onPressedInBind.SubscribeEventHandlerMethod("bindpress",
                _ => binder.InvokeEvent(id, this, new SimpleEventArgs(_)));
            onUnbind += () => UnbindPressEvent(binder, id);
        }

        private void UnbindPressEvent(MultiEventObserver binder, int sourceId)
        {
            onPressedInBind.UnsubscribeEventHandlerMethod("bindpress");
        }

    }
}