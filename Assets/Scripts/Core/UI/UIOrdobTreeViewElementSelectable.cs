using System;
using Michsky.UI.ModernUIPack;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Nrealus.Extensions;

namespace Core.UI
{

    /****** Author : nrealus ****** Last documentation update : 12-07-2020 ******/

    /// <summary>
    /// A Helper component for UIOrdobTreeViewElement. Used to link the Unity UI aspect (callbacks, graphics) with the UI logic.
    /// </summary>   
    public class UIOrdobTreeViewElementSelectable : UIBehaviour, ITreeViewSelectable,
            IBeginDragHandler, IDragHandler, IEndDragHandler,
            IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public float horInset = 2f;
        public float verInset = 1f;

        [HideInInspector]
        private TreeViewElement myAssociatedTreeViewElement;
        // Automatically set when found by TreeViewElement.
        
        public void OnBeginDrag(PointerEventData data)
        {
            myAssociatedTreeViewElement.ActualOnBeginDrag(data);
        }

        public void OnDrag(PointerEventData data)
        {
            myAssociatedTreeViewElement.ActualOnDrag(data);
        }

        public void OnEndDrag(PointerEventData data)
        {
            myAssociatedTreeViewElement.ActualOnEndDrag(data);
        }

        public void OnPointerDown(PointerEventData data)
        {
            myAssociatedTreeViewElement.ActualOnPointerDown(data);
        }

        public void OnPointerEnter(PointerEventData data)
        {
            myAssociatedTreeViewElement.ActualOnPointerEnter(data);
        }

        public void OnPointerExit(PointerEventData data)
        {
            myAssociatedTreeViewElement.ActualOnPointerExit(data);
        }

        private Image image;
        private Color initialColor;
        protected override void Start()
        {
            image = GetComponent<Image>();
            initialColor = image.color;
        }

        protected override void OnRectTransformDimensionsChange()
        {
            transform.GetChild(0).GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, horInset, GetComponent<RectTransform>().rect.width - 2 * horInset);
            transform.GetChild(0).GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, verInset, GetComponent<RectTransform>().rect.height - 2 * verInset);
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public void SetAssociatedTreeViewElement(TreeViewElement tve)
        {
            myAssociatedTreeViewElement = tve;
        }

        public void Highlight()
        {
            image.color = new Color(1-initialColor.r/2, 1-initialColor.g/2, 1-initialColor.b/2, initialColor.a);
        }

        public void Unhighlight()
        {
            image.color = new Color(initialColor.r, initialColor.g, initialColor.b, initialColor.a);
        }

        public void Select()
        {
            image.color = new Color(1-initialColor.r, 1-initialColor.g, 1-initialColor.b, initialColor.a);
        }

        public void Deselect()
        {
            image.color = new Color(initialColor.r, initialColor.g, initialColor.b, initialColor.a);
        }

        public void SetText(string text)
        {
            GetComponentInChildren<TextMeshProUGUI>().text = text;
        }

    }
}