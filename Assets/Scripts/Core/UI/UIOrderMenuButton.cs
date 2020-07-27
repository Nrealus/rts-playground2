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
using System.Linq;

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

        private void OnButtonActivationOrNot(bool desiredButtonState)
        {
            foreach(Transform t in transform.parent.transform)
            {
                var temp = t.GetComponent<UIOrderMenuButton>().associatedTaskEditMenu;

                if (temp != null && temp != this)
                    temp.SetActiveRecursivelyExt(false);
            }
            if (!on && desiredButtonState)
            {
                associatedTaskEditMenu.SetActiveRecursivelyExt(true);
                
                List<ISelectable> l = mySelector.GetCurrentlySelectedEntitiesOfType<Unit>();
                
                switch (taskTypeAsEnumField)
                {
                    case TaskTypeEnum.Move :
                        CreateTaskMarkerEtc<MoveTaskMarker>(l);
                        break;
                    /*case TaskTypeEnum.Build :
                        CreateTaskMarker<MoveTaskMarker>(l);
                        break;
                    case TaskTypeEnum.EngageAt :
                        CreateTaskMarker<MoveTaskMarker>(l);
                        break;*/
                }
            }
            on = desiredButtonState;
        }

        private void CreateTaskMarkerEtc<T>(List<ISelectable> list) where T : TaskMarker
        {
            if (list.Count > 0)
            {
                T taskMarker = TaskMarker.CreateInstanceAtScreenPosition<T>(UIHandler.GetPointedScreenPosition());

                taskMarker.InitBinderForTask(associatedTaskEditMenu);
                
                taskMarker.EnterPlacementUIMode();

                taskMarker.OnExitPlacementUIMode.SubscribeEventHandlerMethod("spawnerbuttondeactivate",// += SpawnerButtonDeactivate;
                    () =>
                    {
                        on = false;
                        taskMarker.OnExitPlacementUIMode.UnsubscribeEventHandlerMethod("spawnerbuttondeactivate");
                    });

                taskMarker.OnPlacementConfirmation.SubscribeEventHandlerMethod("onplacementconfirmationcallback",
                    (b) =>
                    {
                        if (b)
                        {
                            EditedPlanAddTask(taskMarker.GetTask(), list[0] as Unit);

                            if (Input.GetKey(KeyCode.LeftShift))
                            {
                                _lastPlacedTaskMarkerWrapper = new MapMarkerWrapper<TaskMarker>(taskMarker);
                                OnButtonActivationOrNot(true);
                            }
                            else
                                _lastPlacedTaskMarkerWrapper = null;
                        }
                        else
                        {
                            _lastPlacedTaskMarkerWrapper = null;
                            //editedTaskPlan = null;
                        }
                    });

                SelectionHandler.GetUsedSelector().SelectEntity(taskMarker);
            }
        }

        private MapMarkerWrapper<TaskMarker> _lastPlacedTaskMarkerWrapper;
        //private TaskPlan2 editedTaskPlan;

        private void EditedPlanAddTask(Task t, ITaskSubject ts)
        {
            t.GetParameters().AddExecutionMode(TaskParams.TaskExecutionMode.Chain);
            if (_lastPlacedTaskMarkerWrapper?.Value == null)
            {
                var tp = new TaskPlan2(ts);
                tp.AddTaskToPlan(t);
                tp.StartPlanExecution();                
            }
            else
            {
                _lastPlacedTaskMarkerWrapper?.Value.GetTask().GetTaskPlan().AddTaskToPlan(t);
            }

            /*if (editedTaskPlan == null)
            {
                editedTaskPlan = new TaskPlan2(ts);
                editedTaskPlan.AddTaskToPlan(t);
                editedTaskPlan.StartPlanExecution();
            }
            else
            {
                editedTaskPlan.AddTaskToPlan(t);
            }

            t.GetParameters().AddExecutionMode(TaskParams.TaskExecutionMode.Chain);

            if (Input.GetKey(KeyCode.LeftShift))
            {
                OnButtonActivationOrNot(true);
            }
            else
            {
                editedTaskPlan = null;
            }*/

        }

    }
}