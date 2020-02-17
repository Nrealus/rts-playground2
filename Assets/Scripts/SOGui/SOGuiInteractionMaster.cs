using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using GlobalManagers;

namespace SOGui
{
    public class SOGuiInteractionMaster : MonoBehaviour, IPointerClickHandler
    {

        public delegate void OnInteractSOGuiEventHandler(PointerEventData eventData);
        public event OnInteractSOGuiEventHandler onClickAnywhere;
        private PointerEventData eventData = null;

        private void Awake()
        {
            onClickAnywhere = null;
        }

        private void LateUpdate()
        {
            if (eventData != null)
            {
                if (onClickAnywhere != null)
                {
                    onClickAnywhere.Invoke(eventData);
                }
                eventData = null;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
#if false
            if (eventData.button == GameManager.Instance.IM.GetMouseButtonInputButton(GameManager.Instance.IM.LMB))
            {
                this.eventData = eventData;
            }
#endif
        }

    }
}