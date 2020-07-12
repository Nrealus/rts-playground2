using UnityEngine;
using Core.Selection;
using Nrealus.Extensions;
using Core.Units;
using Core.UI;
using Gamelogic.Extensions;
using UnityEngine.EventSystems;
using Michsky.UI.ModernUIPack;
using System.Collections.Generic;
using System.Linq;
using System;
using Nrealus.Extensions.Observer;

namespace Core.Handlers
{
    /****** Author : nrealus ****** Last documentation update : 09-07-2020 ******/

    /// <summary>
    /// Singleton used as a hub for UI related things.
    /// As of now, it defines the global pause/unpause system, generic UI input stuff
    /// (e.g. involving the main pointer, what it points to, and "general in-game UI logic" like the logic that allows to select units using the mouse selection)
    /// </summary>
    public class UIHandler : MonoBehaviour
    {

        public struct PointerInfo
        {
            public PointerEventData pointerEventData;
            public Vector3 pointedPositionWorld;
            public Vector3 pointedPositionScreen;
        }
        
        public PointerInfo pointerInfo = new PointerInfo();
        
        private static Camera _cam;
        public Camera GetMyCamera()
        {
            if(_cam == null)
                _cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

            return _cam;
        }

        private static CanvasGroup uiScreenCanvasGroup;
        private CanvasGroup GetUIScreenCanvasGroup()
        {
            if (uiScreenCanvasGroup == null)
                uiScreenCanvasGroup = GameObject.Find("UI Screen Canvas").GetComponent<CanvasGroup>();//GameObject.FindObjectOfType<WindowManager3>().GetComponent<CanvasGroup>();

            return uiScreenCanvasGroup;
        }

        private static UIHandler _instance;
        public static UIHandler MyInstance
        {
            get
            {
                if(_instance == null)
                    _instance = FindObjectOfType<UIHandler>(); 
                return _instance;
            }
        }

        private EasyObserver<string,bool> onPausedOrNot = new EasyObserver<string, bool>();
        private bool paused = false;

        public static void SubscribeOnPause(string key, Action onPause)
        {
            MyInstance.onPausedOrNot.SubscribeToEvent(key, (_) => { if (_ == true) {onPause();} });
        }

        public static void UnsubscribeOnPause(string key)
        {
            MyInstance.onPausedOrNot.UnsubscribeFromEvent(key);
        }

        public static void SubscribeOnUnpause(string key, Action onUnpause)
        {
            MyInstance.onPausedOrNot.SubscribeToEvent(key, (_) => { if (_ == false) {onUnpause();} });
        }

        public static void UnsubscribeOnUnpause(string key)
        {
            MyInstance.onPausedOrNot.UnsubscribeFromEvent(key);
        }

        public static bool IsPaused()
        {
            return MyInstance.paused;
        }

        public static void PauseGame()
        {
            if (MyInstance.paused == false)
            {
                MyInstance.paused = true;
                MyInstance.onPausedOrNot?.Invoke(true);
            
                MyInstance.GetUIScreenCanvasGroup().alpha = 0.5f;
                MyInstance.GetUIScreenCanvasGroup().blocksRaycasts = false;
                SelectionHandler.GetUsedSelector().Pause();

                GameObjectExtension.FindInactiveObjectByName("UI Screen Canvas (Paused)").SetActiveRecursivelyExt(true);
            }
        }

        public static void UnpauseGame()
        {
            if (MyInstance.paused == true)
            {
                MyInstance.paused = false;
                MyInstance.onPausedOrNot?.Invoke(false);
            
                MyInstance.GetUIScreenCanvasGroup().alpha = 0.7f;
                MyInstance.GetUIScreenCanvasGroup().blocksRaycasts = true;
                SelectionHandler.GetUsedSelector().ActivateAndUnpause();

                GameObject.Find("UI Screen Canvas (Paused)").SetActiveRecursivelyExt(false);
            }
        }

        public static Vector3 GetPointedWorldPosition()
        {
            return MyInstance.pointerInfo.pointedPositionWorld;
        }

        public static Vector3 GetPointedScreenPosition()
        {
            return MyInstance.pointerInfo.pointedPositionScreen;
        }

        public static bool NotPointingUIEventSystem()
        {
            return !EventSystem.current.IsPointerOverGameObject();
        }

        public static List<GameObject> EventSystemPointedGameObjects()
        {
            var pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = UIHandler.GetPointedScreenPosition();

            var _raycastresults = new List<RaycastResult>();

            EventSystem.current.RaycastAll(pointerEventData, _raycastresults);

            return _raycastresults.Select<RaycastResult,GameObject>(_ => { return _.gameObject; }).ToList();
        }

        public static UIOrdobMenu GetUIOrdobMenu()
        {
            //return FindObjectOfType<UIOrderOfBattleMenu>();
            return MyInstance.GetUIScreenCanvasGroup().GetComponentInChildren<WindowManager3>().GetComponentInChildren<UIOrdobMenu>();
        }

        private UIOrderMenu uiOrderMenu;

        private enum UIStates1 { Neutral, Pause }

        private StateMachine<UIStates1> uiGeneralFSM;

        private void Awake()
        {
            uiOrderMenu = FindObjectOfType<UIOrderMenu>();
        }

        private void OnGUI()
        {
            GUI.TextArea(new Rect(10,10,196,32), TimeHandler.CurrentTimeToString());
        }

        private void Update()
        {
            
            UpdatePointerInfo();

            //if (!IsPaused())
            {
                ShapeSelectionControl(SelectionHandler.GetUsedSelector());
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!IsPaused())
                    PauseGame();
                else if (IsPaused())
                    UnpauseGame();
            }
            
        }

        private void UpdatePointerInfo()
        {
            pointerInfo.pointedPositionScreen = Input.mousePosition;
            pointerInfo.pointedPositionWorld = GetMyCamera().GetPointedPositionPhysRaycast(pointerInfo.pointedPositionScreen);
        }

        private bool ShapeSelectionControl(Selector selector)
        {
            selector.UpdatePointerCurrentScreenPosition(pointerInfo.pointedPositionScreen);

            if (Input.GetMouseButtonDown(0) && selector.GetLowState()==Selector.LowStates.NotSelecting
                && NotPointingUIEventSystem())
                selector.StartSelecting();

            if (Input.GetMouseButtonUp(0) && selector.GetLowState()==Selector.LowStates.Selecting)
                selector.ConfirmSelecting();

            if (Input.GetMouseButtonDown(1) && selector.GetLowState()==Selector.LowStates.Selecting)
            {
                selector.CancelSelecting();
                return true;
            }

            if (Input.GetKey(KeyCode.LeftShift))
                selector.selectionMode = Selector.SelectionModes.Additive;
            else if (Input.GetKey(KeyCode.LeftControl))
                selector.selectionMode = Selector.SelectionModes.Subtractive;
            else
                selector.selectionMode = Selector.SelectionModes.Default;

            return false;
        }

        /*
        ///<summary>
        /// mode 0 : screen ui
        /// mode 1 : world ui
        /// mode 2 : both
        ///</summary>
        private bool NoUIAtScreenPositionExceptCanvas(Vector3 screenPosition, int mode)
        {
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = screenPosition;

            _raycastresults = new List<RaycastResult>();
            int hitcount = 0;

            graphicRaycasterScreenUI.Raycast(pointerEventData, _raycastresults);
            hitcount += _raycastresults.Count;

            graphicRaycasterWorldUI.Raycast(pointerEventData, _raycastresults);
            hitcount += _raycastresults.Count;
            
            return hitcount == 0;
        }
    */

    }
}