using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Tasks;
using Core.Units;
using Core.Selection;
using Core.Handlers;
using Nrealus.Extensions;
using UnityEngine.EventSystems;
using System.Text;
using Core.Helpers;
using Core.MapMarkers;
using Nrealus.Extensions.Observer;
using Core.Faction;
using System;

namespace Core.MapMarkers
{
    
    /****** Author : nrealus ****** Last documentation update : 25-07-2020 ******/

    /// <summary>
    /// A MapMarker subclass, that serve as a means of communication with an associated Task. It has both a pure game logic and UI role.
    /// For now, a Task is spawned from a TaskMarker, and the settings of a TaskMarker can be used to influence the parameters or behaviour of a Task.
    /// That includes the position of the marker on the map.
    /// There also is a TaskPlan associated to this marker, and the associated Task is expected to be part of the TaskPlan.
    /// </summary>   
    public abstract class TaskMarker : MapMarker
    {

        private static Camera _cam;
        public Camera GetMyCamera()
        {
            if(_cam == null)
                _cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

            return _cam;
        }

        private static RectTransform _uiScreenCanvasRT;
        protected RectTransform GetScreenCanvasRT()
        {
            if(_uiScreenCanvasRT == null)
                _uiScreenCanvasRT = GameObject.Find("UI Screen Canvas").GetComponent<RectTransform>();

            return _uiScreenCanvasRT;
        }

        protected Selector GetUsedSelector()
        {
            return SelectionHandler.GetUsedSelector();
        }

        #region Static "factory" functions

        public static T CreateInstanceAtWorldPosition<T>(Vector3 worldPosition)
            where T : TaskMarker
        {
            T res = Instantiate(
                GameObject.Find("ResourcesList").GetComponent<ResourcesListComponent>().taskMarkerPrefab,
                GameObject.Find("UI Screen Canvas").GetComponent<RectTransform>())
                .gameObject.AddComponent<T>();

            res.Init(worldPosition, false);

            return res;
        }

        public static T CreateInstanceAtScreenPosition<T>(Vector3 screenPosition)
            where T : TaskMarker
        {
            T res = Instantiate(
                GameObject.Find("ResourcesList").GetComponent<ResourcesListComponent>().taskMarkerPrefab,
                GameObject.Find("UI Screen Canvas").GetComponent<RectTransform>())
                .gameObject.AddComponent<T>();

            res.Init(screenPosition, true);

            return res;
        }

        #endregion

        #region Main declarations

        protected bool isEditing = false;

        protected FactionAffiliation factionAffiliation;
        protected UnityEngine.UI.Graphic uiGraphic;

        public EasyObserver<string> OnExitPlacementUIMode = new EasyObserver<string>();
        //public event Action OnExitPlacementUIMode;
        public EasyObserver<string, bool> OnPlacementConfirmation = new EasyObserver<string, bool>();

        protected bool ready = false;
        protected bool expanded = false;
        protected bool paused = false;

        #endregion

        #region Initialisation

        protected virtual void Init(Vector3 position, bool screenPosTrue)
        {
        }

        public void InitBinderForTask(GameObject associatedTaskEditMenu)
        {
            var binder = new MultiEventObserver();
            
            BindTaskSelectionEvent(binder,
                (sender, args) => {
                    //Debug.Log("Triggered by direct selection status change.");
                    if (args is SimpleEventArgs)
                        associatedTaskEditMenu.SetActiveRecursivelyExt(((bool)(args as SimpleEventArgs).args[0]));
                });
        }
 
        private void BindTaskSelectionEvent(MultiEventObserver binder, Action<object, EventArgs> action)
        {
            var id = binder.AddNewEventAndSubscribeMethodToIt(action);
            GetOnSelectionStateChangeObserver().SubscribeEventHandlerMethod("whateverkey", 
                (_) => { if (_.Item3 == 0) binder.InvokeEvent(id,this, new SimpleEventArgs(_.Item2)); }, true);
        }

        #endregion

        #region Public functions
        
        public Vector3 GetScreenPosition()
        {
            return GetMyCamera().WorldToScreenPoint(transform.position);
        }

        public Vector3 GetWorldPosition()
        {
            return GetMyCamera().GetPointedPositionPhysRaycast(GetScreenPosition());
        }

        public abstract TaskMarker GetPreviousTaskMarker();

        public abstract Task GetTask();

        public void EnterPlacementUIMode()
        {
            isEditing = true;
        }

        public void ExitPlacementUIMode()
        {
            OnExitPlacementUIMode.Invoke();
            isEditing = false;
        }

        public void ConfirmPositioning(bool confirmationStatus)
        {
            OnPlacementConfirmation.Invoke(confirmationStatus);            
        }

        #endregion

        #region Protected behaviour methods

        private void Update()
        {
            UpdateMe();
        }

        protected virtual void UpdateMe()
        {
            if (!paused)
            {
                if (ready)
                {
                    if (isEditing)
                    {
                        if (Input.GetKeyDown(KeyCode.Mouse1))
                        {
                            ExitPlacementUIMode();
                            DestroyThis();
                        }
                        PlaceAtScreenPosition(UIHandler.GetPointedScreenPosition());                
                    }
                }
            }
        }

        protected virtual void DrawUpdate(Color initialColor)
        {
            if (uiGraphic != null)
            {
                uiGraphic.color = initialColor;
                
                if (GetUsedSelector().IsSelected(this))
                {
                    uiGraphic.color = factionAffiliation.GetFaction().baseColor;
                }
                /*else if (GetUsedSelector().IsHighlighted(GetRefWrapper()))
                {
                    graphic.color = 
                        new Color(factionAffiliation.MyFaction.baseColor.r,
                                factionAffiliation.MyFaction.baseColor.g,
                                factionAffiliation.MyFaction.baseColor.b,
                                factionAffiliation.MyFaction.baseColor.a/2);
                }*/
            }
        }

        protected void Expand()
        {
            GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 80);
            GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 80);
            expanded = true;
        }    

        protected void Contract()
        {
            GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 24);
            GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 24);
            expanded = false;
        }

        protected void PlaceAtScreenPosition(Vector3 screenPosition)
        {
            GetComponent<RectTransform>().SetPositionOfPivotFromViewportPosition(GetScreenCanvasRT(),
                GetMyCamera().ScreenToViewportPoint(screenPosition)); 
        }
        
        protected void PlaceAtWorldPosition(Vector3 worldPosition)
        {
            GetComponent<RectTransform>().SetPositionOfPivotFromViewportPosition(GetScreenCanvasRT(),
                GetMyCamera().WorldToViewportPoint(worldPosition));
            //PlaceAtScreenPosition(GetMyCamera().WorldToScreenPoint(worldPosition));
        }

        #endregion
    
    
        public virtual TaskPlan2 InsertAssociatedTaskIntoPlan(ITaskAgent agent, TaskMarker previousTaskMarker)
        {
            TaskPlan2 taskPlan;
            if (previousTaskMarker == null)
            {
                taskPlan = agent.CreateAndRegisterNewOwnedPlan();
                taskPlan.AddTaskToPlan(GetTask());
            }
            else
            {
                GetTask().GetParameters().AddExecutionMode(TaskParams.TaskExecutionMode.Chain);

                taskPlan = previousTaskMarker.GetTask().GetTaskPlan();
                taskPlan.AddTaskToPlan(GetTask());
            }
            return taskPlan;
        }

    }
}
