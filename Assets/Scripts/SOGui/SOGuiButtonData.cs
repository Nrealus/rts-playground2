using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SOGui
{
    namespace ScriptableObjects
    {
        [CreateAssetMenu(fileName = "SOGuiButtonData", menuName = "GUI/Scriptable Object GUI Button Data", order = 0)]
        public class SOGuiButtonData : SOGuiData
        {

            public Sprite buttonSprite;
            public SpriteState buttonSpriteState;

            public Sprite buttonSpritePressedHighlighted;

            [HideInInspector] public SpriteState buttonPressedSpriteState;

            public Color buttonColor;

            public Color CTNormalColor = Color.white;
            public Color CTHighlightedColor = Color.white;
            public Color CTPressedColor = Color.white;
            public Color CTDisabledColor = Color.white;

            private void OnEnable()
            {
                buttonPressedSpriteState = new SpriteState();
                buttonPressedSpriteState.pressedSprite = buttonSprite;
                buttonPressedSpriteState.highlightedSprite = buttonSpritePressedHighlighted;
                buttonPressedSpriteState.disabledSprite = buttonSpriteState.disabledSprite;
            }
        }
    }
}