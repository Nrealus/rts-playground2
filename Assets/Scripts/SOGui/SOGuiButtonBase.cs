using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VariousUtilsExtensions;
using GlobalManagers;

namespace SOGui
{
    public class SOGuiButtonBase : SOGui,
        IPointerClickHandler,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Tooltip("Whether this button stays On/'pushed' when it gets clicked.")]
        public bool togglable = false;

        [SerializeField]
        private bool _isOn = false;
        public bool IsOn
        {
            get
            {
                return _isOn;
            }

            set
            {
                bool p = _isOn;
                _isOn = value;
                if (p != _isOn)
                {
                    OnUICosmeticUpdate();
                }
            }
        }

        [HideInInspector]
        public bool Interactable
        {
            get
            {
                return GetComponent<Selectable>().interactable;
            }

            set
            {
                bool p = GetComponent<Selectable>().interactable;
                GetComponent<Selectable>().interactable = value;
                if (p != value)
                {
                    if (value == false)
                    {
                        _isOn = false;
                        _isHighlighted = false; // HACK : encapsulated field used here, NOT the property !!!
                        _isPressed = false;
                    }
                    OnUICosmeticUpdate();
                }
            }
        }

        [SerializeField]
        private bool _isHighlighted = false;
        public bool IsHighlighted
        {
            get
            {
                return _isHighlighted;
            }

            set
            {
                bool p = _isHighlighted;
                _isHighlighted = value;

                if (_isHighlighted == true)
                {
                    if (OnHighlight != null)
                        OnHighlight.Invoke();
                }
                else
                {
                    if (OnUnHighlight != null)
                        OnUnHighlight.Invoke();
                }
                if (p != _isHighlighted)
                {
                    OnUICosmeticUpdate();
                }
            }
        }

        [SerializeField]
        private bool _isPressed = false;
        public bool IsPressed
        {
            get
            {
                return _isPressed;
            }

            set
            {
                bool p = _isPressed;
                _isPressed = value;
                if (p != _isPressed)
                {
                    OnUICosmeticUpdate();
                }
            }
        }

        private bool _isDragged = false;
        public bool IsDragged { get { return _isDragged; } private set { _isDragged = value; } }

        [Tooltip("If this button is part of the toggle group 'rooted' on this button's parent.\nAll hierarchically equal elements with this tag can only be On one at a time.")]
        public bool isInParentToggroup;

        public delegate void VoidVoidDelegate();

        protected event VoidVoidDelegate OnButtonClick;
        protected event VoidVoidDelegate OnSimpleButtonClick;
        protected event VoidVoidDelegate OnDoubleButtonClick;

        protected event VoidVoidDelegate OnHighlight;
        protected event VoidVoidDelegate OnUnHighlight;

        protected event VoidVoidDelegate OnMyBeginDrag;
        protected event VoidVoidDelegate OnMyDrag;
        protected event VoidVoidDelegate OnMyEndDrag;

        protected override void Awake()
        {
            UnsubscribeAllOnInteraction();

            base.Awake();
        }

        protected float doubleclicktimer = 0f;
        protected override void Update()
        {
            if (Interactable && doubleclicktimer > 0 && doubleclicktimer - Time.deltaTime < 0)
            {
                if (OnSimpleButtonClick != null)
                    OnSimpleButtonClick.Invoke();
                OnUICosmeticUpdate();
            }
            doubleclicktimer = Mathf.Max(0, doubleclicktimer - Time.deltaTime);

            base.Update();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
#if false
            if (Interactable && eventData.button == GameManager.Instance.IM.GetMouseButtonInputButton(GameManager.Instance.IM.LMB))
            {
                GameManager.Instance.currentMainCanvas.GetComponent<SOGuiInteractionMaster>().OnPointerClick(eventData);
                ButtonClicked();
            }
#endif
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Interactable)
                IsHighlighted = true;

            //RethrowRaycast(eventData, this.gameObject, 0);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (Interactable)
                IsHighlighted = false;
            //RethrowRaycast(eventData, this.gameObject, 1);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
#if false
            if (Interactable && eventData.button == GameManager.Instance.IM.GetMouseButtonInputButton(GameManager.Instance.IM.LMB))
            {
                IsPressed = true;
                if (!IsHighlighted)
                {
                    IsHighlighted = true;
                }
            }
#endif
        }

        public void OnPointerUp(PointerEventData eventData)
        {
#if false
            if (Interactable && eventData.button == GameManager.Instance.IM.GetMouseButtonInputButton(GameManager.Instance.IM.LMB))
            {
                IsPressed = false;
                if (isInParentToggroup) // HACK : keep in mind ; DO NOT put this block in IsPressed getter, it belongs here !!
                {
                    foreach (Transform t in transform.parent.transform)
                    {
                        SOGuiButtonBase tb = t.GetComponent<SOGuiButtonBase>();
                        if (t != transform && tb != null && tb.isInParentToggroup)
                        {
                            tb.IsOn = false;
                        }
                    }
                }
            }
#endif
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (OnMyBeginDrag != null)
            {
                IsHighlighted = true;
                IsDragged = true;
                OnMyBeginDrag.Invoke();
                UIUtils.SetAllUIRaycastable(GameManager.Instance.currentMainCanvas.transform,false);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (OnMyDrag != null)
            {
                if (!IsHighlighted)
                {
                    IsHighlighted = true;
                }
                IsDragged = true;
                OnMyDrag.Invoke();
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (OnMyEndDrag != null)
            {
                IsDragged = false;
                OnMyEndDrag.Invoke();
                UIUtils.SetAllUIRaycastable(GameManager.Instance.currentMainCanvas.transform,true);
            }
        }

        protected void OnTogglableButtonValueChanged(bool v)
        {
            IsOn = v;
        }

        /// <summary>
        /// Encapsulates what happens when a button get effectively clicked. It is for example called from OnPointerClick.
        /// </summary>
        protected virtual void ButtonClicked()
        {
            if (doubleclicktimer > 0)
            {
                if (OnDoubleButtonClick != null)
                    OnDoubleButtonClick.Invoke();
            }
            else
            {
                if (togglable)
                {
                    OnTogglableButtonValueChanged(!IsOn);
                }
                if (OnButtonClick != null)
                    OnButtonClick.Invoke();
            }
            doubleclicktimer = GameManager.Instance.doubleClickMaxTime;

        }

        public void SubscribeOnClick(VoidVoidDelegate call)
        {
            OnButtonClick += call;
        }

        public void SubscribeOnSimpleClick(VoidVoidDelegate call)
        {
            OnSimpleButtonClick += call;
        }

        public void SubscribeOnClickAnywhere(SOGuiInteractionMaster.OnInteractSOGuiEventHandler call)
        {
            //GameManager.Instance.currentMainHandler.GetComponent<SOGuiInteractionMaster>().onClickAnywhere += call;
        }

        public void SubscribeOnDoubleClick(VoidVoidDelegate call)
        {
            OnDoubleButtonClick += call;
        }

        public void SubscribeOnHighlight(VoidVoidDelegate call)
        {
            OnHighlight += call;
        }

        public void SubscribeOnUnHighlight(VoidVoidDelegate call)
        {
            OnUnHighlight += call;
        }

        public void SubscribeOnBeginDrag(VoidVoidDelegate call)
        {
            OnMyBeginDrag += call;
        }

        public void SubscribeOnDrag(VoidVoidDelegate call)
        {
            OnMyDrag += call;
        }

        public void SubscribeOnEndDrag(VoidVoidDelegate call)
        {
            OnMyEndDrag += call;
        }

        /// <summary>
        /// Resets all of the OnSomething events to null.
        /// </summary>
        public void UnsubscribeAllOnInteraction()
        {
            OnButtonClick = null;
            OnSimpleButtonClick = null;
            OnDoubleButtonClick = null;

            OnHighlight = null;
            OnUnHighlight = null;

            OnMyBeginDrag = null;
            OnMyDrag = null;
            OnMyEndDrag = null;
        }

        public void OnUICosmeticUpdateForced()
        {
            OnUICosmeticUpdate();
        }
        /*
        private void RethrowRaycast(PointerEventData eventData, GameObject excludeGameObject)
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                //pointerEventData.position = eventData.pressPosition;
                position = eventData.position
            };

            //Where to store Raycast Result
            List<RaycastResult> raycastResult = new List<RaycastResult>();

            //Rethrow the raycast to include everything regardless of their Z position
            EventSystem.current.RaycastAll(pointerEventData, raycastResult);

            //Debug.Log("Other GameObject hit");
            for (int i = 0; i < raycastResult.Count; i++)
            {
                //Debug.Log(raycastResult[i].gameObject.name);

                //Don't Rethrow Raycayst for the first GameObject that is hit
                if (excludeGameObject != null && raycastResult[i].gameObject != excludeGameObject)
                {
                    SimulatePointerEnterFunction(raycastResult[i].gameObject);
                }
            }
        }
        
        void SimulatePointerEnterFunction(GameObject target)
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            //pointerEventData.ra
            RaycastResult res = new RaycastResult
            {
                gameObject = target
            };
            pointerEventData.pointerCurrentRaycast = res;
            ExecuteEvents.Execute(target, pointerEventData, ExecuteEvents.pointerEnterHandler);
        }
        */
    }
}
