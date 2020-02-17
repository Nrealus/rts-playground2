using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SOGui.ScriptableObjects;

namespace SOGui
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]

    public class SOGuiButtonStdTransition : SOGuiButtonBase2
    {
        protected Color CTNormalColor;
        protected Color CTHighlightedColor;
        protected Color CTPressedColor;
        protected Color CTDisabledColor;

        protected override void Awake()
        {
            base.Awake();
            CTNormalColor = ((SOGuiButtonData)mySOGuiData).CTNormalColor;
            CTHighlightedColor = ((SOGuiButtonData)mySOGuiData).CTHighlightedColor;
            CTPressedColor = ((SOGuiButtonData)mySOGuiData).CTPressedColor;
            CTDisabledColor = ((SOGuiButtonData)mySOGuiData).CTDisabledColor;
            OnUICosmeticUpdate();
        }

        /// <summary>
        /// Graphical update of the UI element.
        /// </summary>
        protected override void OnUICosmeticUpdate()
        {

            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = image;

            ColorBlock colorblock = button.colors;
            colorblock.normalColor = CTNormalColor;
            colorblock.highlightedColor = CTHighlightedColor;
            colorblock.pressedColor = CTPressedColor;
            colorblock.disabledColor = CTDisabledColor;
            button.colors = colorblock;

            image.type = Image.Type.Sliced;
            image.color = color;

            if (IsOn)
            {
                if (IsPressed == true)
                {
                    image.sprite = spritesIsOn[2];
                    if (IsHighlighted == true)
                    {
                        image.sprite = spritesIsNotOn[1];
                    }
                }
                else if(IsHighlighted == true)
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
                    if (IsHighlighted == true)
                    {
                        image.sprite = spritesIsOn[1];
                    }
                }
                else if (IsHighlighted == true)
                {
                    image.sprite = spritesIsNotOn[1];
                }
                else
                {
                    image.sprite = spritesIsNotOn[0];
                }
                if (!Interactable)
                {
                    image.sprite = spritesIsNotOn[3];
                }
            }


            if (text != null)
            {
                if (image.color.r * 0.3 + image.color.g * 0.59 + image.color.b * 0.11 <= 0.6)
                {
                    text.color = Color.white;
                }
                else
                {
                    text.color = Color.black;
                }
            }


            base.OnUICosmeticUpdate();
        }
    }

}
