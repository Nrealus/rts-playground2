using System;
using System.Collections.Generic;
using Core.Handlers;
using Core.MapMarkers;
using Core.Selection;
using Core.Tasks;
using Core.Units;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Nrealus.Extensions;
using Nrealus.Extensions.Observer;

namespace Core.UI
{
    /****** Author : nrealus ****** Last documentation update : 12-07-2020 ******/

    /// <summary>
    /// Component for buttons in the "Orders" panel allowing to spawn and place TaskMarkers.
    /// </summary>   
    public class UIOrderMenuButton : MonoBehaviour
    {
        
        private enum TaskTypeEnum {Move, Build, EngageAt}
        [SerializeField]
        private TaskTypeEnum taskTypeAsEnumField;

        public GameObject associatedTaskEditMenu;

        private Selector mySelector;

        public bool on = false;

        private void Start()
        {
            mySelector = SelectionHandler.GetUsedSelector();

            if (associatedTaskEditMenu != null)
                associatedTaskEditMenu.SetActiveRecursivelyExt(false);
        }

        public void OnButtonClick()
        {
            OnButtonActivationOrNot(!on);
        }

        private void OnButtonActivationOrNot(bool b)
        {
            foreach(Transform t in transform.parent.transform)
            {
                var temp = t.GetComponent<UIOrderMenuButton>().associatedTaskEditMenu;

                if (temp != null && temp != this)
                    temp.SetActiveRecursivelyExt(false);
            }
            if (!on && b)
            {
                associatedTaskEditMenu.SetActiveRecursivelyExt(true);
                
                List<ISelectable> l = mySelector.GetCurrentlySelectedEntitiesOfType<UnitWrapper>();
                
                switch (taskTypeAsEnumField)
                {
                    case TaskTypeEnum.Move :
                        CreateTaskMarker<MoveTaskMarker>(mySelector.GetCurrentlySelectedEntitiesOfType<UnitWrapper>());
                        break;
                    case TaskTypeEnum.Build :
                        CreateTaskMarker<MoveTaskMarker>(mySelector.GetCurrentlySelectedEntitiesOfType<UnitWrapper>());
                        break;
                    case TaskTypeEnum.EngageAt :
                        CreateTaskMarker<MoveTaskMarker>(mySelector.GetCurrentlySelectedEntitiesOfType<UnitWrapper>());
                        break;
                }
            }
            on = b;
        }

        private void CreateTaskMarker<T>(List<ISelectable> list) where T : TaskMarker
        {
            if (list.Count > 0)
            {
                var v = TaskMarker.CreateInstance<T>(UIHandler.GetPointedScreenPosition(), list);

                InitBinderForTaskWrapper(v);
                
                v.GetWrappedReference().EnterEditMode();

                v.GetWrappedReference().OnExitEditMode.SubscribeToEvent("spawnerbuttondeactivate",
                    () =>
                    {
                        on = false;
                        v.GetWrappedReference().OnExitEditMode.UnsubscribeFromEvent("spawnerbuttondeactivate");
                    });

                SelectionHandler.GetUsedSelector().SelectEntity(v);
            }
        }

        private void BindTaskWrapperSelectionEvent(MultiEventObserver binder, Action<object, EventArgs> action, TaskMarkerWrapper tmw)
        {
            var id = binder.AddNewEventAndSubscribeToIt(action);
            tmw.GetOnSelectionStateChangeObserver().SubscribeToEvent("key",
                (_) => binder.InvokeEvent(id,tmw, new SimpleEventArgs(_.Item2)));
        }

        private void InitBinderForTaskWrapper(TaskMarkerWrapper tmw)
        {
            var binder = new MultiEventObserver();
            
            BindTaskWrapperSelectionEvent(binder,
                (sender, args) => {
                    //Debug.Log("Triggered by direct selection status change.");
                    if (args is SimpleEventArgs)
                        associatedTaskEditMenu.SetActiveRecursivelyExt(((bool)(args as SimpleEventArgs).args[0]));
                }, tmw);
        }

    }
}