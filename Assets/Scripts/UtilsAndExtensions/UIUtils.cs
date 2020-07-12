using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nrealus.Extensions
{
    public static class UIUtils
    {
        /// <summary>
        /// (De)activates a UI element with an attached CanvasGroup, by setting the CanvasGroup's alpha to (0), interactable and blocksRaycast fields to (false) true.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="val"></param>
        public static void SetUIActive(Transform obj, bool val)
        {
            CanvasGroup canvasgroup = obj.GetComponent<CanvasGroup>();
            if (canvasgroup == null)
            {
                Debug.LogError("No canvas group !");
                return;
            }

            if (val)
            {
                if (canvasgroup.alpha == 0)
                {
                    canvasgroup.alpha = 1;
                }
                canvasgroup.interactable = true;
                canvasgroup.blocksRaycasts = true;
            }
            if (!val)
            {
                if (canvasgroup.alpha != 0)
                {
                    canvasgroup.alpha = 0;
                }
                canvasgroup.interactable = false;
                canvasgroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// Returns whether a UI element with an attached CanvasGroup is "active" or not. See SetUIActive for information on UI "activation/deactivation" approach.
        /// </summary>
        public static bool GetUIActive(Transform obj)
        {
            CanvasGroup canvasgroup = obj.GetComponent<CanvasGroup>();
            if (canvasgroup == null)
            {
                Debug.LogError("No canvas group !");
                return false;
            }

            return (canvasgroup.alpha != 0);

        }

        /// <summary>
        /// If a UI element is already "inactive", this will keep the attached CanvasGroup's blockRaycasts field at false. If not, it will be set to val. This allows to "block raycasts" on an active UI element without deactivating it.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="val"></param>
        public static void SetUIRaycastable(Transform obj, bool val)
        {
            CanvasGroup canvasgroup = obj.GetComponent<CanvasGroup>();
            if (canvasgroup == null)
            {
                Debug.LogError("No canvas group !");
                return;
            }
            bool b = GetUIActive(obj);
            SetUIActive(obj, b);
            if (!b)
            {
                canvasgroup.blocksRaycasts = false;
            }
            else
            {
                canvasgroup.blocksRaycasts = val;
            }
        }

        /// <summary>
        /// Applies SetUIRaycastable (with parameter val) to all CanvasGroups attached to any child of rootObj, if the CanvasGroup's ignoreParentGroups is false.
        /// </summary>
        /// <param name="rootObj"></param>
        /// <param name="val"></param>
        public static void SetAllUIRaycastable(Transform rootObj, bool val)
        {
            CanvasGroup[] canvasgroups = rootObj.GetComponentsInChildren<CanvasGroup>();
            for (int i = 0; i < canvasgroups.Length; i++)
            {
                if (canvasgroups[i] == null)
                {
                    Debug.LogError("No canvas group !");
                    return;
                }
                if (canvasgroups[i].ignoreParentGroups == false)
                {
                    SetUIRaycastable(canvasgroups[i].transform, val);
                }
            }
        }

        /// <summary>
        /// Returns box bounds in viewport coordinates based on 2 screen positions and the Camera we're looking through.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="screenPosition1"></param>
        /// <param name="screenPosition2"></param>
        /// <returns></returns>
        public static Bounds GetViewportBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2)
        {
            var v1 = camera.ScreenToViewportPoint(screenPosition1);
            var v2 = camera.ScreenToViewportPoint(screenPosition2);
            var min = Vector3.Min(v1, v2);
            var max = Vector3.Max(v1, v2);
            min.z = camera.nearClipPlane;
            max.z = camera.farClipPlane;

            var bounds = new Bounds();
            bounds.SetMinMax(min, max);
            return bounds;
        }

    }
}