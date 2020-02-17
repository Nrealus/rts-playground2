using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SOGui.ScriptableObjects;

namespace SOGui
{
    [ExecuteInEditMode]
    public abstract class SOGui : MonoBehaviour
    {
        [Tooltip("The Scriptable Object asset with graphical/style data to apply to this UI element.")]
        public SOGuiData mySOGuiData;

        /// <inheritdoc>
        /// Graphical update of the UI element.
        /// </inheritdoc>
        protected virtual void OnUICosmeticUpdate()
        {

        }

        protected virtual void Awake()
        {
            OnUICosmeticUpdate();
        }

        protected virtual void Update()
        {
            if (!Application.isPlaying)
            {
                OnUICosmeticUpdate();
            }
        }

    }
}
