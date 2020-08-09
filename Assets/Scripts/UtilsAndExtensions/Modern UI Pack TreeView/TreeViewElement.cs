using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

namespace Nrealus.Extensions
{
    public abstract class TreeViewElement : UIBehaviour
    {
        
        //public abstract GraphicRaycaster GetMyGraphicRaycaster();

        [Header("RESOURCES")]
        public RectTransform dragArea;
        //public RectTransform dragObject;
        public RectTransform containerOfWholeTreeView;
        public RectTransform childrenRoot;

        [Header("SETTINGS")]
        public float expandGraphicCutStep;
        public bool isRoot;
        public bool destroyChildrenOnDestruction;

        // Fields

        public bool isOn { get; protected set; }

        public abstract ITreeViewSelectable GetAssociatedTreeViewSelectable();

        private Vector2 originalLocalPointerPosition;
        private Vector3 originalPanelLocalPosition;

        protected bool dragging;

        private Transform previousParent;
        private int positionInPreviousParent;

        private RectTransform DragObjectInternal
        {
            get
            {
                //if (dragObject == null)
                    return (transform as RectTransform);
                //else
                //    return dragObject;
            }
        }

        private RectTransform DragAreaInternal
        {
            get
            {
                if (dragArea == null)
                {
                    RectTransform canvas = transform as RectTransform;
                    while (canvas.parent != null && canvas.parent is RectTransform)
                    {
                        canvas = canvas.parent as RectTransform;
                    }
                    return canvas;
                }
                else
                    return dragArea;
            }
        }

        private TreeViewElement _rootTreeViewElement;
        private TreeViewElement RootTreeViewElement
        {
            get
            {
                if (_rootTreeViewElement == null)
                {
                    foreach (Transform t in containerOfWholeTreeView)
                    {
                        var _temp = t.GetComponent<TreeViewElement>();

                        if (_temp != null && _temp.isRoot)
                        _rootTreeViewElement = _temp;
                    }
                }
                return _rootTreeViewElement;
            }
        }

        #region dddddd

        protected override void Awake() // previously "new"...
        {
            if(dragArea == null)
            {
                try
                {
                    var canvas = (Canvas)GameObject.FindObjectsOfType(typeof(Canvas))[0];
                    dragArea = canvas.GetComponent<RectTransform>();
                }

                catch
                {
                    Debug.LogError("TreeView Element - Drag Area has not been assigned.");
                }
            }

        }

        protected override void Start() // previously "new"...
        {
            OnTransformParentChanged();
        }

        public abstract void OnDestruction();

        public void DestroyMe()
        {

            if (!isRoot && !destroyChildrenOnDestruction)
            {
                var p = FindParentElement();

                TreeViewElement drg = null;
                foreach (Transform t in containerOfWholeTreeView)
                {
                    if (t.GetComponent<TreeViewElement>().dragging)
                        drg = t.GetComponent<TreeViewElement>();
                }

                if (drg != null && drg.previousParent == childrenRoot)
                {
                    drg.previousParent = p.childrenRoot;
                    //onPreviousParentElementDestroyed?.Invoke(p);
                    drg.positionInPreviousParent = p.childrenRoot.childCount - 1;
                }

                foreach (Transform t in childrenRoot)
                {
                    t.SetParent(p.childrenRoot);
                }
                
                OnDestruction();

                Destroy(gameObject);
            }
        }

        #endregion

        protected void Update()
        {
            //AdaptChildrenGraphicSizes();
        }

        /// <summary>
        /// This callback is called if an associated RectTransform has its dimensions changed. The call is also made to all child rect transforms, even if the child transform itself doesn't change - as it could have, depending on its anchoring.
        /// </summary>
        protected override void OnRectTransformDimensionsChange()
        {}

        protected override void OnBeforeTransformParentChanged()
        {}

        protected override void OnTransformParentChanged()
        {
            if (!isRoot && dragging == false)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(RootTreeViewElement.GetComponent<RectTransform>());
                
                GetAssociatedTreeViewSelectable().GetGameObject().GetComponent<RectTransform>().SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal, 
                    FindParentElement().GetAssociatedTreeViewSelectable().GetGameObject().GetComponent<RectTransform>().rect.width - expandGraphicCutStep);

            }
        }

        protected override void OnDidApplyAnimationProperties()
        {}

        protected override void OnCanvasGroupChanged()
        {}

        /// <summary>
        /// Called when the state of the parent Canvas is changed.
        /// </summary>
        protected override void OnCanvasHierarchyChanged()
        {}

        #region Actual On Pointer And Drag Events

        public void ActualOnBeginDrag(PointerEventData data)
        {
            if (!isRoot)
            {
                dragging = true;

                previousParent = transform.parent;
                positionInPreviousParent = transform.GetSiblingIndex();

                transform.SetParent(containerOfWholeTreeView);

                originalPanelLocalPosition = DragObjectInternal.localPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(DragAreaInternal, data.position, data.pressEventCamera, out originalLocalPointerPosition);
                //DragObjectInternal.SetAsLastSibling();

                //DragObjectInternal.SetAsFirstSibling();
                transform.SetAsLastSibling();
            }
        }

        private TreeViewElement _toattachto;
        public void ActualOnDrag(PointerEventData data)
        {
            //dragging = true;

            if (!isRoot)
            {
                var _temp = FindObjectToAttachTo(data);
                if (_temp != _toattachto)
                {
                    OnDragHoverOn(_temp, _toattachto);
                    _toattachto = _temp;
                }
                                
                Vector2 localPointerPosition;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(DragAreaInternal, data.position, data.pressEventCamera, out localPointerPosition))
                {
                    Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
                    DragObjectInternal.localPosition = originalPanelLocalPosition + offsetToOriginal;
                }
                
                ClampToArea();
            }
        }

        public void ActualOnEndDrag(PointerEventData data)
        {
            if (!isRoot)
            {
                dragging = false;

                var r = FindObjectToAttachTo(data);
                OnAttachTo(r);
                if (r != null)
                {
                    //transform.SetParent(r.childrenRoot);
                }
                else
                {
                    transform.SetParent(previousParent);
                    transform.SetSiblingIndex(Mathf.Min(positionInPreviousParent, previousParent.childCount - 1));
                }

                GetComponent<RectTransform>().ForceUpdateRectTransforms();
            }
        }

        public void ActualOnPointerDown(PointerEventData data)
        {
            isOn = !isOn;
            OnPressInOrOut(isOn, data);
        }

        public virtual void ActualOnPointerEnter(PointerEventData data)
        {
            if (!isOn) 
                GetAssociatedTreeViewSelectable().Highlight();
        }

        public virtual void ActualOnPointerExit(PointerEventData data)
        {
            if (!isOn) 
                GetAssociatedTreeViewSelectable().Unhighlight();
        }

        #endregion

        #region

        public virtual void OnDragHoverOn(TreeViewElement hoveredPotentialAttachElements, TreeViewElement previousHoveredPotentialAttachElements)
        {
            if (previousHoveredPotentialAttachElements != null)
                previousHoveredPotentialAttachElements.GetAssociatedTreeViewSelectable().Unhighlight();

            if (hoveredPotentialAttachElements != null)
                hoveredPotentialAttachElements.GetAssociatedTreeViewSelectable().Highlight();
        }

        public virtual void OnAttachTo(TreeViewElement newParent)
        {
            transform.SetParent(newParent.childrenRoot);
            if (newParent != null)
                newParent.GetAssociatedTreeViewSelectable().Unhighlight();
        }

        public virtual void OnPressInOrOut(bool inOrOut, PointerEventData data)
        {
            if (inOrOut)
            {
                GetAssociatedTreeViewSelectable().Select();
            }
            else
            {
                if (data != null && data.hovered.Contains(GetAssociatedTreeViewSelectable().GetGameObject()))
                    GetAssociatedTreeViewSelectable().Highlight();
                else
                    GetAssociatedTreeViewSelectable().Deselect();
            }
        }

        #endregion

        #region Private Helper Functions

        private void ClampToArea()
        {
            Vector3 pos = DragObjectInternal.localPosition;

            Vector3 minPosition = DragAreaInternal.rect.min - DragObjectInternal.rect.min;
            Vector3 maxPosition = DragAreaInternal.rect.max - DragObjectInternal.rect.max;

            pos.x = Mathf.Clamp(DragObjectInternal.localPosition.x, minPosition.x, maxPosition.x);
            pos.y = Mathf.Clamp(DragObjectInternal.localPosition.y, minPosition.y, maxPosition.y);

            DragObjectInternal.localPosition = pos;
        }

        private TreeViewElement FindParentElement()
        {
            if (isRoot == true || transform.parent.gameObject == containerOfWholeTreeView)
                return null;

            Transform res = transform.parent;
            while (true)
            {
                if (res.GetComponent<TreeViewElement>() == null)
                    res = res.parent;
                else
                    break;
            }
            return res.GetComponent<TreeViewElement>();
        }

        private void AdaptChildrenGraphicSizes()
        {
            if (isRoot || transform.parent.gameObject == containerOfWholeTreeView)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
                AdaptChildrenGraphicSizes_Aux(childrenRoot, 1);
            }
        }

        private void AdaptChildrenGraphicSizes_Aux(Transform childrenRoot, int acc)
        {
            foreach (Transform t in childrenRoot)
            {
                if (t.GetComponent<TreeViewElement>() != null)
                {
                    t.GetComponent<TreeViewElement>().GetAssociatedTreeViewSelectable().GetGameObject().GetComponent<RectTransform>().SetSizeWithCurrentAnchors(
                        RectTransform.Axis.Horizontal, DragObjectInternal.rect.width - acc*expandGraphicCutStep);
    
                    //t.GetComponent<TreeViewElement>().draggableGraphic.GetComponent<RectTransform>().ForceUpdateRectTransforms();

                    AdaptChildrenGraphicSizes_Aux(t.GetComponent<TreeViewElement>().childrenRoot, acc+1);
                }
            }
        }

        private TreeViewElement FindRootTreeViewElement()
        {
            foreach (Transform t in containerOfWholeTreeView)
            {
                if (t.GetComponent<TreeViewElement>() != null && t.GetComponent<TreeViewElement>().isRoot)
                    return t.GetComponent<TreeViewElement>();
            }
            return null;
        }

        List<RaycastResult> _raycastResults = new List<RaycastResult>();
        private TreeViewElement FindObjectToAttachTo(PointerEventData data)
        {
            _raycastResults.Clear();

            //GetMyGraphicRaycaster().Raycast(data, raycastResults);            
            EventSystem.current.RaycastAll(data,_raycastResults);

            var r = FindRootTreeViewElement();

            if (_raycastResults.ConvertAll<GameObject>(_ => _.gameObject).Contains(r.GetAssociatedTreeViewSelectable().GetGameObject()))
                return r;
            else if (r.childrenRoot.childCount > 0)
                return FindObjectToAttachTo_Aux(r.childrenRoot, data, _raycastResults);
            else
                return r;
        }

        private TreeViewElement FindObjectToAttachTo_Aux(Transform childrenRoot, PointerEventData data, List<RaycastResult> raycastResults)
        {
            TreeViewElement res = null;
            foreach (Transform t in childrenRoot)
            {
                var tve = t.GetComponent<TreeViewElement>();
                if (tve != null)
                {
                    if (raycastResults.ConvertAll<GameObject>(_ => _.gameObject).Contains(tve.GetAssociatedTreeViewSelectable().GetGameObject()))
                    {
                        res = tve;
                        break;
                    }
                    else if (tve.childrenRoot.childCount > 0)
                    {
                        res = FindObjectToAttachTo_Aux(tve.childrenRoot, data, raycastResults);
                    }
                }
            }

            return res;
        }

        #endregion

    }
}