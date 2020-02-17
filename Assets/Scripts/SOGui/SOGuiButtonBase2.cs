using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SOGui.ScriptableObjects;

namespace SOGui
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]

    public class SOGuiButtonBase2 : SOGuiButtonBase
    {

        protected Sprite[] spritesIsNotOn = new Sprite[4];
        protected Sprite[] spritesIsOn = new Sprite[4];
        protected Image image;
        protected Button button;
        protected Color color;
        protected TextMeshProUGUI text;

        protected override void Awake()
        {
            image = GetComponent<Image>();
            button = GetComponent<Button>();
            text = GetComponentInChildren<TextMeshProUGUI>();

            spritesIsNotOn = new Sprite[4];
            spritesIsNotOn[0] = ((SOGuiButtonData)mySOGuiData).buttonSprite;
            spritesIsNotOn[1] = ((SOGuiButtonData)mySOGuiData).buttonSpriteState.highlightedSprite;
            spritesIsNotOn[2] = ((SOGuiButtonData)mySOGuiData).buttonSpriteState.pressedSprite;
            spritesIsNotOn[3] = ((SOGuiButtonData)mySOGuiData).buttonSpriteState.disabledSprite;

            spritesIsOn = new Sprite[4];
            spritesIsOn[0] = ((SOGuiButtonData)mySOGuiData).buttonSpriteState.pressedSprite;
            spritesIsOn[1] = ((SOGuiButtonData)mySOGuiData).buttonPressedSpriteState.highlightedSprite;
            spritesIsOn[2] = ((SOGuiButtonData)mySOGuiData).buttonPressedSpriteState.pressedSprite;
            spritesIsOn[3] = ((SOGuiButtonData)mySOGuiData).buttonPressedSpriteState.disabledSprite;

            color = ((SOGuiButtonData)mySOGuiData).buttonColor;

            base.Awake();
        }

    }
}
