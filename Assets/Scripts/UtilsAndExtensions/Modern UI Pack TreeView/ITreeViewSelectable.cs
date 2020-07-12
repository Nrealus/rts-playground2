using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;
using Michsky.UI.ModernUIPack;

namespace Nrealus.Extensions
{
    public interface ITreeViewSelectable
    {
        GameObject GetGameObject();

        void SetAssociatedTreeViewElement(TreeViewElement tve);

        void Highlight();

        void Unhighlight();

        void Select();

        void Deselect();
    }

}