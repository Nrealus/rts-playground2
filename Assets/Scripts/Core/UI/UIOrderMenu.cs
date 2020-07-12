using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.MapMarkers;
using Core.Handlers;

namespace Core.UI
{
    public class UIOrderMenu : MonoBehaviour
    {
    
        public bool blocked = false;
        
        public int state = 0; // 0 : hidden, 1: shown, 2: in the background
    
        private static Camera _cam;
        public Camera GetMyCamera()
        {
            if(_cam == null)
                _cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

            return _cam;
        }

        public void SetScreenPosition(Vector3 screenPosition)
        {
            //GetComponent<UIView>().CustomStartAnchoredPosition = screenPosition;//GetMyCamera().ViewportToScreenPoint(GetMyCamera().ScreenToViewportPoint(screenPosition));
            //GetComponent<UIView>().UpdateStartPosition();
        }

        public void Show()
        {
            //GetComponent<UIView>().Show(true);
            state = 1;
        }

        public void Hide()
        {
            //GetComponent<UIView>().Hide(true);
            state = 0;
        }

        public void CreateMoveOrderPrefab()
        {
            //TaskMarker.CreateInstance(UIHandler.GetPointedScreenPosition(), null);
        }

        private void Awake()
        {
            //GameEventMessage.AddListener("GlobalPauseGE", Pause);
            //GameEventMessage.AddListener("GlobalUnpauseGE", Unpause);

            //GameEventMessage.AddListener("ShowOrderMenuGE", Show);
            //GameEventMessage.AddListener("HideOrderMenuGE", Hide);

            Hide();
        }

        private void Update()
        {
            if (!paused)
            {
                if (Input.GetKeyDown(KeyCode.Mouse1)
                    && UIHandler.NotPointingUIEventSystem())
                {
                    if (state == 0)
                    {
                        SetScreenPosition(Input.mousePosition);
                        //GameEventMessage.Send("ShowOrderMenuGE");
                        //Show();
                    }
                    else if (state == 1)
                    {
                        //GameEventMessage.Send("HideOrderMenuGE");
                        //Hide();
                    }
                }

                if (!blocked)
                {
                    GetComponent<CanvasGroup>().alpha = 1f;
                    GetComponent<CanvasGroup>().blocksRaycasts = true;
                }
                else
                {
                    GetComponent<CanvasGroup>().alpha = 0.5f;
                    GetComponent<CanvasGroup>().blocksRaycasts = false;
                }
            }
            else
            {
                if (!blocked)
                {
                    GetComponent<CanvasGroup>().alpha = 0.75f;
                    GetComponent<CanvasGroup>().blocksRaycasts = false;
                }
                else
                {
                    GetComponent<CanvasGroup>().alpha = 0.5f;
                    GetComponent<CanvasGroup>().blocksRaycasts = false;
                }
            }
        }

        private void DestroyMe()
        {
            //GameEventMessage.RemoveListener("GlobalPauseGE", Pause);
            //GameEventMessage.RemoveListener("GlobalUnpauseGE", Unpause);           

            //GameEventMessage.RemoveListener("ShowOrderMenuGE", Pause);
            //GameEventMessage.RemoveListener("HideOrderMenuGE", Unpause);
            
            Destroy(gameObject);
        }

        private bool paused = false;
        private void Pause()
        {
            paused = true;
            //var pointer = new PointerEventData(EventSystem.current);
            //ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, pointer, ExecuteEvents.pointerEnterHandler);
            //ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, pointer, ExecuteEvents.pointerExitHandler);
        }

        private void Unpause()
        {
            paused = false;
            //var pointer = new PointerEventData(EventSystem.current);
            //ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, pointer, ExecuteEvents.pointerEnterHandler);
            //ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, pointer, ExecuteEvents.pointerExitHandler);
        }

    }
}