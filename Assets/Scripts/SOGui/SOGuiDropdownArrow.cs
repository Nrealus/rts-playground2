using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Nrealus.Extensions;

namespace SOGui
{
    public class SOGuiDropdownArrow : SOGuiButtonBase2
    {

        protected RectTransform myRectTransf;
        public Transform toOpenOrClose;

        public RectTransform contentPrefabRect;
        private float contentNormalHeight;

        protected override void ButtonClicked()
        {
            contentNormalHeight = contentPrefabRect.rect.height;

            base.ButtonClicked();
            
            UIUtils.SetUIActive(toOpenOrClose.transform, IsOn);
            if (IsOn)
            {
                foreach (Transform rt in toOpenOrClose.transform)
                {
                    rt.GetComponent<RectTransform>().SetHeight(contentNormalHeight);
                    rt.GetComponentInChildren<TextMeshProUGUI>().alpha = 1;
                    rt.GetComponent<SOGuiButtonBase>().OnUICosmeticUpdateForced();
                }
            }
            else
            {
                foreach (Transform rt in toOpenOrClose.transform)
                {
                    rt.GetComponent<RectTransform>().SetHeight(0f);
                    rt.GetComponentInChildren<TextMeshProUGUI>().alpha = 0;
                    rt.GetComponent<SOGuiButtonBase>().OnUICosmeticUpdateForced();
                }
            }

        }

        /// <summary>
        /// Graphical update of the UI element.
        /// </summary>
        protected override void OnUICosmeticUpdate()
        {
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = image;
            image.type = Image.Type.Sliced;
            image.color = color;

            if (IsOn)
            {
                if (IsPressed == true)
                {
                    image.sprite = spritesIsOn[2];
                }
                else if (IsHighlighted == true)
                {
                    image.sprite = spritesIsOn[1];
                }
                else
                {
                    image.sprite = spritesIsOn[0];
                }
                if (!button.IsInteractable())
                {
                    image.sprite = spritesIsOn[3];
                }
            }
            else
            {
                if (IsPressed == true)
                {
                    image.sprite = spritesIsNotOn[2];
                }
                else if (IsHighlighted == true)
                {
                    image.sprite = spritesIsNotOn[1];
                }
                else
                {
                    image.sprite = spritesIsNotOn[0];
                }
                if (!button.IsInteractable())
                {
                    image.sprite = spritesIsNotOn[3];
                }
            }

            if (IsOn)
            {
                transform.localRotation = Quaternion.Euler(0, 0, -90);
                //UnityEngine.Extensions.TransformExtensions.SetLocalEulerAngles(myRectTransf, 0, 0, -90);
            }
            else
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                //UnityEngine.Extensions.TransformExtensions.SetLocalEulerAngles(myRectTransf, 0, 0, 0);
            }
        }
    }
}