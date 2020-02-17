using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SOGui
{
    namespace ScriptableObjects
    {
        [CreateAssetMenu(fileName = "SOGuiImageData", menuName = "GUI/Scriptable Object GUI Image Data", order = 0)]
        public class SOGuiImageData : SOGuiData
        {
            public Sprite backgroundSprite;
            public Color backgroundColor;

        }
    }
}