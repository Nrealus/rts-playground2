using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SOGui.ScriptableObjects;

namespace SOGui
{

    [RequireComponent(typeof(Image))]
    public class SOGuiImage : SOGui
    {

        protected Image image;

        protected override void Awake()
        {
            image = GetComponent<Image>();

            base.Awake();
        }

        /// <summary>
        /// Graphical update of the UI element.
        /// </summary>
        protected override void OnUICosmeticUpdate()
        {

            image.sprite = ((SOGuiImageData)mySOGuiData).backgroundSprite;
            image.color = ((SOGuiImageData)mySOGuiData).backgroundColor;
            image.type = Image.Type.Sliced;

            base.OnUICosmeticUpdate();
        }

    }

}
