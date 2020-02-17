using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GlobalManagers
{
    // Central class for all things input related.
    public class InputManager : MonoBehaviour
    {
        private GameManager gm;

        #region Input keys properties

        public int Z_Key
        {
            get
            { return keysUsed.IndexOf(gm.zkey); }
            private set
            { gm.zkey = (KeyCode)value; }
        }
        public int S_Key
        {
            get
            { return keysUsed.IndexOf(gm.skey); }
            private set
            { gm.skey = (KeyCode)value; }
        }
        public int Q_Key
        {
            get
            { return keysUsed.IndexOf(gm.qkey); }
            private set
            { gm.qkey = (KeyCode)value; }
        }
        public int D_Key
        {
            get
            { return keysUsed.IndexOf(gm.dkey); }
            private set
            { gm.dkey = (KeyCode)value; }
        }
        public int A_Key
        {
            get
            { return keysUsed.IndexOf(gm.akey); }
            private set
            { gm.akey = (KeyCode)value; }
        }
        public int E_Key
        {
            get
            { return keysUsed.IndexOf(gm.ekey); }
            private set
            { gm.ekey = (KeyCode)value; }
        }
        public int F_Key
        {
            get
            { return keysUsed.IndexOf(gm.fkey); }
            private set
            { gm.fkey = (KeyCode)value; }
        }
        public int R_Key
        {
            get
            { return keysUsed.IndexOf(gm.rkey); }
            private set
            { gm.rkey = (KeyCode)value; }
        }
        public int LShift_Key
        {
            get
            { return keysUsed.IndexOf(gm.lshiftkey); }
            private set
            { gm.lshiftkey = (KeyCode)value; }
        }
        public int LAlt_Key
        {
            get
            { return keysUsed.IndexOf(gm.laltkey); }
            private set
            { gm.laltkey = (KeyCode)value; }
        }
        public int LCtrl_Key
        {
            get
            { return keysUsed.IndexOf(gm.lctrlkey); }
            private set
            { gm.lctrlkey = (KeyCode)value; }
        }
        public int Alpha0_Key
        {
            get
            { return keysUsed.IndexOf(gm.alpha0key); }
            private set
            { gm.alpha0key = (KeyCode)value; }
        }
        public int Alpha1_Key
        {
            get
            { return keysUsed.IndexOf(gm.alpha1key); }
            private set
            { gm.alpha1key = (KeyCode)value; }
        }
        public int Alpha2_Key
        {
            get
            { return keysUsed.IndexOf(gm.alpha2key); }
            private set
            { gm.alpha2key = (KeyCode)value; }
        }
        public int Alpha3_Key
        {
            get
            { return keysUsed.IndexOf(gm.alpha3key); }
            private set
            { gm.alpha3key = (KeyCode)value; }
        }
        public int Alpha4_Key
        {
            get
            { return keysUsed.IndexOf(gm.alpha4key); }
            private set
            { gm.alpha4key = (KeyCode)value; }
        }
        public int Alpha5_Key
        {
            get
            { return keysUsed.IndexOf(gm.alpha5key); }
            private set
            { gm.alpha5key = (KeyCode)value; }
        }
        public int Alpha6_Key
        {
            get
            { return keysUsed.IndexOf(gm.alpha6key); }
            private set
            { gm.alpha6key = (KeyCode)value; }
        }
        public int Alpha7_Key
        {
            get
            { return keysUsed.IndexOf(gm.alpha7key); }
            private set
            { gm.alpha7key = (KeyCode)value; }
        }
        public int Alpha8_Key
        {
            get
            { return keysUsed.IndexOf(gm.alpha8key); }
            private set
            { gm.alpha8key = (KeyCode)value; }
        }
        public int Alpha9_Key
        {
            get
            { return keysUsed.IndexOf(gm.alpha9key); }
            private set
            { gm.alpha9key = (KeyCode)value; }
        }

        #endregion

        #region Mouse button properties

        public int LMB
        {
            get
            { return gm.lmb; }
            private set
            { gm.lmb = value; }
        }
        public int RMB
        {
            get
            { return gm.rmb; }
            private set
            { gm.rmb = value; }
        }
        public int MMB
        {
            get
            { return gm.mmb; }
            private set
            { gm.mmb = value; }
        }

        #endregion

        public List<KeyCode> keysUsed; // List of the keyboard keys used.

        #region Keyboard input info
        // Read-only bool "containers" for mouse buttons states/events : held, pressed (general), pressed only once, pressed twice, released.
        [HideInInspector] public bool[] IsKeyHeld;
        [HideInInspector] public bool[] IsKeyPressed;
        [HideInInspector] public bool[] IsKeySimplePressed;
        [HideInInspector] public bool[] IsKeyDoublePressed;
        [HideInInspector] public bool[] IsKeyReleased;

        private int[] numberOfKeyPresses; // Number of key presses in short amount of time (doublePressMaxTime) per keyboard key.
        private float[] keyPressesTimer; // Key pressing reset timer for each key.
        #endregion

        #region Mouse input info
        // Read-only bool "containers" for mouse buttons states/events : held, pressed (general), pressed only once, pressed twice, released.
        // 0 : Left Mouse Button, 1 : Right Mouse Button, 2 : Middle Mouse Button
        [HideInInspector] public bool[] IsMouseButtonHeld { get; private set; }
        [HideInInspector] public bool[] IsMouseButtonPressed { get; private set; }
        [HideInInspector] public bool[] IsMouseButtonSimplePressed { get; private set; }
        [HideInInspector] public bool[] IsMouseButtonDoublePressed { get; private set; }
        [HideInInspector] public bool[] IsMouseButtonReleased { get; private set; }

        [HideInInspector] public float MouseScroll { get; private set; } // Basically Input.GetAxis("Mouse ScrollWheel").
        [HideInInspector] public Vector3 MousePosition { get; private set; } // The mouse's screen position.
        [HideInInspector] public Vector3 MouseLastClickPosition { get; private set; } // The mouse's screen position.

        private int[] numberOfClicks; // Number of clicks in short amount of time (doublePressMaxTime) per mouse button.
        private float[] clicksTimer; // Button clicking reset timer for each mouse button.

        #endregion

        private GraphicRaycaster canvasRaycaster;
        private PointerEventData pointerEventData;
        private EventSystem eventSystem;

        private List<RaycastResult> PointerRaycastResults;
        public bool PointerNotOnUIElement { get; private set; } // Indicates whether the mouse is over a UI element.
        
        public void Init()
        {
            gm = GameManager.Instance;

            canvasRaycaster = FindObjectOfType<Canvas>().GetComponent<GraphicRaycaster>();
            eventSystem = FindObjectOfType<EventSystem>();
            pointerEventData = new PointerEventData(eventSystem);
            PointerRaycastResults = new List<RaycastResult>();
            PointerNotOnUIElement = true;

            // Initialize used keyboard keys

            keysUsed = new List<KeyCode>
            {
                gm.zkey,
                gm.skey,
                gm.qkey,
                gm.dkey,
                gm.akey,
                gm.ekey,
                gm.fkey,
                gm.rkey,
                gm.laltkey,
                gm.lshiftkey,
                gm.lctrlkey,
                gm.alpha0key,
                gm.alpha1key,
                gm.alpha2key,
                gm.alpha3key,
                gm.alpha4key,
                gm.alpha5key,
                gm.alpha6key,
                gm.alpha7key,
                gm.alpha8key,
                gm.alpha9key
            };

            #region Initialize keyboard input states.

            IsKeyHeld = new bool[keysUsed.Count];
            IsKeyPressed = new bool[keysUsed.Count];
            IsKeySimplePressed = new bool[keysUsed.Count];
            IsKeyDoublePressed = new bool[keysUsed.Count];
            IsKeyReleased = new bool[keysUsed.Count];
            numberOfKeyPresses = new int[keysUsed.Count];
            keyPressesTimer = new float[keysUsed.Count];

            for (int i = 0; i < keysUsed.Count; i++)
            {
                IsKeyHeld[i] = false;
                IsKeyPressed[i] = false;
                IsKeySimplePressed[i] = false;
                IsKeyDoublePressed[i] = false;
                IsKeyReleased[i] = false;
                numberOfKeyPresses[i] = 0;
                keyPressesTimer[i] = 0;
            }

            #endregion

            #region Initialize mouse input states.

            IsMouseButtonHeld = new bool[3];
            IsMouseButtonPressed = new bool[3];
            IsMouseButtonSimplePressed = new bool[3];
            IsMouseButtonDoublePressed = new bool[3];
            IsMouseButtonReleased = new bool[3];
            numberOfClicks = new int[3];
            clicksTimer = new float[3];

            for (int i = 0; i < 3; i++)
            {
                IsMouseButtonHeld[i] = false;
                IsMouseButtonPressed[i] = false;
                IsMouseButtonSimplePressed[i] = false;
                IsMouseButtonDoublePressed[i] = false;
                IsMouseButtonReleased[i] = false;
                numberOfClicks[i] = 0;
                clicksTimer[i] = 0;
            }

            MousePosition = Input.mousePosition;
            MouseScroll = 0;

            #endregion
        }

        // Update input states
        private void Update()
        {

            for (int i = 0; i < keysUsed.Count; i++)
            {
                IsKeyHeld[i] = Input.GetKey(keysUsed[i]);
                IsKeyPressed[i] = Input.GetKeyDown(keysUsed[i]);
                IsKeyReleased[i] = Input.GetKeyUp(keysUsed[i]);
                IsKeySimplePressed[i] = false;
                IsKeyDoublePressed[i] = false;
            }

            MousePosition = Input.mousePosition;
            MouseScroll = Input.GetAxis("Mouse ScrollWheel");

            for (int k = 0; k < 3; k++)
            {
                IsMouseButtonHeld[k] = Input.GetMouseButton(k);
                IsMouseButtonPressed[k] = Input.GetMouseButtonDown(k);
                IsMouseButtonReleased[k] = Input.GetMouseButtonUp(k);
                IsMouseButtonSimplePressed[k] = false;
                IsMouseButtonDoublePressed[k] = false;
            }

            DoubleAndSimplePressesUpdate();

        }

        private void OnGUI() // TODO : performance ????
        {
            #region Is pointer over UI or not ?

            PointerNotOnUIElement = true;

            pointerEventData = new PointerEventData(eventSystem)
            {
                position = MousePosition
            };

            PointerRaycastResults = new List<RaycastResult>();

            if (canvasRaycaster != null)
                canvasRaycaster.Raycast(pointerEventData, PointerRaycastResults);

            if (PointerRaycastResults.Count >= 2)
            {
                PointerNotOnUIElement = false;
            }

            #endregion
        }

        // Handles double & simple presses input states.
        private void DoubleAndSimplePressesUpdate()
        {

            for (int i = 0; i < keysUsed.Count; i++)
            {
                if (keyPressesTimer[i] > 0 && keyPressesTimer[i] - Time.unscaledDeltaTime <= 0)
                {
                    if (numberOfKeyPresses[i] == 1)
                    {
                        IsKeySimplePressed[i] = true;
                    }
                    if (numberOfKeyPresses[i] == 2)
                    {
                        IsKeyDoublePressed[i] = true;
                    }
                }

                keyPressesTimer[i] = Mathf.Max(0, keyPressesTimer[i] - Time.unscaledDeltaTime);

                if (keyPressesTimer[i] == 0)
                {
                    numberOfKeyPresses[i] = 0;
                }

                if (Input.GetKeyDown(keysUsed[i]))
                {
                    if (numberOfKeyPresses[i] == 1 && keyPressesTimer[i] > 0)
                    {
                        numberOfKeyPresses[i] = 2;
                    }
                    if (numberOfKeyPresses[i] == 0)
                    {
                        keyPressesTimer[i] = gm.doublePressMaxTime;
                        numberOfKeyPresses[i] = 1;
                    }
                }

            }

            for (int k = 0; k < 3; k++)
            {
                if (clicksTimer[k] > 0 && clicksTimer[k] - Time.unscaledDeltaTime <= 0)
                {
                    if (numberOfClicks[k] == 1)
                    {
                        IsMouseButtonSimplePressed[k] = true;
                    }
                    if (numberOfClicks[k] == 2)
                    {
                        IsMouseButtonDoublePressed[k] = true;
                    }
                }

                clicksTimer[k] = Mathf.Max(0, clicksTimer[k] - Time.unscaledDeltaTime);

                if (clicksTimer[k] == 0)
                {
                    numberOfClicks[k] = 0;
                }

                if (Input.GetMouseButtonDown(k))
                {
                    if (numberOfClicks[k] == 1 && clicksTimer[k] > 0)
                    {
                        numberOfClicks[k] = 2;
                        MouseLastClickPosition = MousePosition;
                    }
                    if (numberOfClicks[k] == 0)
                    {
                        clicksTimer[k] = gm.doubleClickMaxTime;
                        numberOfClicks[k] = 1;
                        MouseLastClickPosition = MousePosition;
                    }
                }

            }

        }

        /// <summary>
        /// Maps mouse button identified by MB (0 : LMB, 1 : RMB, 2 : MMB) to PointerEventData's input buttons for the mouse.
        /// </summary>
        /// <param name="MB"></param>
        /// <returns></returns>
        public PointerEventData.InputButton GetMouseButtonInputButton(int MB)
        {
            PointerEventData.InputButton inputButton;
            switch (MB)
            {
                case 0:
                    inputButton = PointerEventData.InputButton.Left;
                    break;
                case 1:
                    inputButton = PointerEventData.InputButton.Right;
                    break;
                case 2:
                    inputButton = PointerEventData.InputButton.Middle;
                    break;
                default:
                    inputButton = PointerEventData.InputButton.Left;
                    break;
            }
            return inputButton;
        }
    }
}