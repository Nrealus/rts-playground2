using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;

namespace Michsky.UI.ModernUIPack
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