using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nrealus.Extensions
{
    public static class RectTransformExtension
    {
        /// <summary>
        /// Sets the local scale to (1,1,1).
        /// </summary>
        /// <param name="trans"></param>
        public static void SetDefaultScale(this RectTransform trans)
        {
            trans.localScale = new Vector3(1, 1, 1);
        }
        /// <summary>
        /// Sets the pivot, anchorMin and anchorMax to aVec.
        /// </summary>
        public static void SetPivotAndAnchors(this RectTransform trans, Vector2 aVec)
        {
            trans.pivot = aVec;
            trans.anchorMin = aVec;
            trans.anchorMax = aVec;
        }
        /// <summary>
        /// rect.size
        /// </summary>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static Vector2 GetSize(this RectTransform trans)
        {
            return trans.rect.size;
        }
        /// <summary>
        /// rect.width
        /// </summary>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static float GetWidth(this RectTransform trans)
        {
            return trans.rect.width;
        }
        /// <summary>
        /// rect.height
        /// </summary>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static float GetHeight(this RectTransform trans)
        {
            return trans.rect.height;
        }
        /// <summary>
        /// Sets localPosition to (newPos.x, newPos.y, localPosition.z).
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="newPos"></param>
        public static void SetPositionOfPivot(this RectTransform trans, Vector2 newPos)
        {
            trans.localPosition = new Vector3(newPos.x, newPos.y, trans.localPosition.z);
        }
        /// <summary>
        /// Sets localPosition corresponding to viewport coordinates position.
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="newPos"></param>
        public static void SetPositionOfPivotFromViewportPosition(this RectTransform trans, RectTransform canvas, Vector2 viewportPosition)
        {
            viewportPosition.x -= 0.5f;
            viewportPosition.y -= 0.5f;
            viewportPosition.Scale(canvas.rect.size);
            trans.localPosition = viewportPosition;
        }
        public static void SetLeftBottomPosition(this RectTransform trans, Vector2 newPos)
        {
            trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
        }
        public static void SetLeftTopPosition(this RectTransform trans, Vector2 newPos)
        {
            trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
        }
        public static void SetRightBottomPosition(this RectTransform trans, Vector2 newPos)
        {
            trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width), newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
        }
        public static void SetRightTopPosition(this RectTransform trans, Vector2 newPos)
        {
            trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width), newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
        }
        /// <summary>
        /// Correctly sets to a new size.
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="newSize"></param>
        public static void SetSize(this RectTransform trans, Vector2 newSize)
        {
            Vector2 oldSize = trans.rect.size;
            Vector2 deltaSize = newSize - oldSize;
            trans.offsetMin -= new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
            trans.offsetMax += new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
        }
        /// <summary>
        /// Correctly sets to new size for width
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="newSize"></param>
        public static void SetWidth(this RectTransform trans, float newSize)
        {
            SetSize(trans, new Vector2(newSize, trans.rect.size.y));
        }
        /// <summary>
        /// Correctly sets to new size for height
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="newSize"></param>
        public static void SetHeight(this RectTransform trans, float newSize)
        {
            SetSize(trans, new Vector2(trans.rect.size.x, newSize));
        }
    }
}