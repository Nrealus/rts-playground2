using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Michsky.UI.ModernUIPack
{
    public class WindowManager3 : MonoBehaviour
    {
        // Content
        public List<WindowItem> windows = new List<WindowItem>();

        // Settings
        public int lastInteractedWindowIndex = 0;

        public string windowFadeIn = "Panel In";
        public string windowFadeOut = "Panel Out";

        private GameObject lastInteractedWindow;

        private Animator lastInteractedWindowAnimator;

        [System.Serializable]
        public class WindowItem
        {
            public string windowName = "My Window";
            public GameObject windowObject;
        }

        void Start()
        {
            lastInteractedWindow = windows[lastInteractedWindowIndex].windowObject;
            //lastInteractedWindowAnimator = lastInteractedWindow.GetComponent<Animator>();
            //lastInteractedWindowAnimator.Play(windowFadeIn);
        }

        public void SelectWindow(GameObject window)
        {
            int newWindowIndex = 0;

            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i].windowObject == window)
                    newWindowIndex = i;
            }

            if (window != lastInteractedWindow)
            {
                lastInteractedWindow = window;

                lastInteractedWindow.transform.SetAsLastSibling();
            }
        }

        public void AddNewItem()
        {
            WindowItem window = new WindowItem();
            windows.Add(window);
        }
    }
}