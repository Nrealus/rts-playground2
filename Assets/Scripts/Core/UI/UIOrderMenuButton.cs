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
using Core.Formations;

namespace Core.UI
{
    /****** Author : nrealus ****** Last documentation update : 12-07-2020 ******/

    /// <summary>
    /// Component for buttons in the "Orders" panel allowing to spawn and place TaskMarkers.
    /// </summary>   
    public class UIOrderMenuButton : MonoBehaviour
    {
        
        private enum TaskTypeEnum { Move, Build, EngageAt}
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
                
                List<ISelectable> selected = mySelector.GetCurrentlySelectedEntitiesOfType<Unit>();
                
                switch (taskTypeAsEnumField)
                {
                    case TaskTypeEnum.Move :
                        UITaskMarkerCreationAndTaskBuilding<MoveTaskMarker>(selected);
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

        private ITaskSubject _taskSubject;
        private MapMarkerWrapper<TaskMarker> _lastPlacedTaskMarkerWrapper;
        private void UITaskMarkerCreationAndTaskBuilding<T>(List<ISelectable> selected) where T : TaskMarker
        {
            if (selected.Count > 0)
            {

                //if (_taskSubject == null)
                //    _taskSubject = PrepareAndGetSubjectFromSelected(selected);

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
                            if (_taskSubject == null)
                                _taskSubject = UnitTeam.PrepareAndCreateTeamFromSelected(selected);

                            TaskPlan2 taskPlan = taskMarker.InsertAssociatedTaskIntoPlan(_taskSubject, _lastPlacedTaskMarkerWrapper?.Value);

                            if (!taskPlan.IsPlanBeingExecuted())
                                taskPlan.StartPlanExecution();

                            if (Input.GetKey(KeyCode.LeftShift))
                            {
                                /*if (_lastPlacedTaskMarkerWrapper == null)
                                {
                                    var tm = ITaskSubject.GetPlans().FirstOrDefault()?.GetCurrentTaskInPlan()?.GetTaskMarker();
                                    if (tm != null)
                                        _lastPlacedTaskMarkerWrapper = new MapMarkerWrapper<TaskMarker>(tm);
                                }
                                else
                                {
                                    _lastPlacedTaskMarkerWrapper = new MapMarkerWrapper<TaskMarker>(taskMarker);
                                }*/
                                _lastPlacedTaskMarkerWrapper = new MapMarkerWrapper<TaskMarker>(taskMarker);
                                OnButtonActivationOrNot(true);
                            }
                            else
                            {
                                _taskSubject = null;
                                _lastPlacedTaskMarkerWrapper = null;
                            }
                        }
                        else
                        {
                            _taskSubject = null;
                            _lastPlacedTaskMarkerWrapper = null;
                            taskMarker.OnPlacementConfirmation.UnsubscribeEventHandlerMethod("onplacementconfirmationcallback");
                            //editedTaskPlan = null;
                        }
                    });

                SelectionHandler.GetUsedSelector().SelectEntity(taskMarker);
            }
        }

        /*private ITaskSubject PrepareAndGetSubjectFromSelected(List<ISelectable> selectedEntities)
        {
            Formation res;
            if (selectedEntities.Count == 1)
            {
                (selectedEntities[0] as Unit).ResetNominalFormationStructure();
                res = (selectedEntities[0] as Unit).GetNominalFormation();
            }
            else
            {
                var units = selectedEntities.ConvertAll( _ => _ as Unit);
                Unit.FlattenUnitsToParentIfAllSiblingsContained(units);

                Unit lca = Unit.GetLowestCommonAncestor(units, false);
                units.Remove(lca);

                if (units.Count == 0)
                {
                    Debug.Log("ky");
                    lca.ResetNominalFormationStructure();
                    res = lca.GetNominalFormation();
                }
                else
                {
                    Debug.Log("kyy");
                    res = lca.AddOrGetLocalAuxiliaryFormation(units);
                }
            }
            return res;
        }*/

    }
}