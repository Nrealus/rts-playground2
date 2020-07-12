using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Michsky.UI.ModernUIPack
{
    public class WindowManager2 : MonoBehaviour
    {
        // Content
        public List<WindowItem> windows = new List<WindowItem>();

        // Settings
        public int currentWindowIndex = 0;
        private int newWindowIndex;
        public string windowFadeIn = "Panel In";
        public string windowFadeOut = "Panel Out";

        private GameObject currentWindow;
        private GameObject nextWindow;

        private Animator currentWindowAnimator;
        private Animator nextWindowAnimator;

        [System.Serializable]
        public class WindowItem
        {
            public string windowName = "My Window";
            public GameObject windowObject;
        }

        void Start()
        {
            currentWindow = windows[currentWindowIndex].windowObject;
            currentWindowAnimator = currentWindow.GetComponent<Animator>();
            currentWindowAnimator.Play(windowFadeIn);
        }

        public void OpenFirstTab()
        {
            if (currentWindowIndex != 0)
            {
                currentWindow = windows[currentWindowIndex].windowObject;
                currentWindowAnimator = currentWindow.GetComponent<Animator>();
                currentWindowAnimator.Play(windowFadeOut);

                currentWindowIndex = 0;

                currentWindow = windows[currentWindowIndex].windowObject;
                currentWindowAnimator = currentWindow.GetComponent<Animator>();
                currentWindowAnimator.Play(windowFadeIn);
            }

            else if (currentWindowIndex == 0)
            {
                currentWindow = windows[currentWindowIndex].windowObject;
                currentWindowAnimator = currentWindow.GetComponent<Animator>();
                currentWindowAnimator.Play(windowFadeIn);
            }
        }

        public void OpenPanel(string newPanel)
        {
            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i].windowName == newPanel)
                    newWindowIndex = i;
            }

            if (newWindowIndex != currentWindowIndex)
            {
                currentWindow = windows[currentWindowIndex].windowObject;
                currentWindowIndex = newWindowIndex;
                nextWindow = windows[currentWindowIndex].windowObject;

                currentWindowAnimator = currentWindow.GetComponent<Animator>();
                nextWindowAnimator = nextWindow.GetComponent<Animator>();

                currentWindowAnimator.Play(windowFadeOut);
                nextWindowAnimator.Play(windowFadeIn);
            }
        }

        public void NextPage()
        {
            if (currentWindowIndex <= windows.Count - 2)
            {
                currentWindow = windows[currentWindowIndex].windowObject;

                currentWindowAnimator = currentWindow.GetComponent<Animator>();
                
                currentWindowAnimator.Play(windowFadeOut);

                currentWindowIndex += 1;
                nextWindow = windows[currentWindowIndex].windowObject;

                nextWindowAnimator = nextWindow.GetComponent<Animator>();
                nextWindowAnimator.Play(windowFadeIn);
            }
        }

        public void PrevPage()
        {
            if (currentWindowIndex >= 1)
            {
                currentWindow = windows[currentWindowIndex].windowObject;

                currentWindowAnimator = currentWindow.GetComponent<Animator>();

                currentWindowAnimator.Play(windowFadeOut);

                currentWindowIndex -= 1;
                nextWindow = windows[currentWindowIndex].windowObject;

                nextWindowAnimator = nextWindow.GetComponent<Animator>();
                nextWindowAnimator.Play(windowFadeIn);
            }
        }

        public void AddNewItem()
        {
            WindowItem window = new WindowItem();
            windows.Add(window);
        }
    }
}