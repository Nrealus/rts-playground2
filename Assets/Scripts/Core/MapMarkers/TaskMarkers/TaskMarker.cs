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

        public static T CreateInstance<T>(Vector3 screenPosition/*, IEnumerable<ISelectable> subjects*/) where T : TaskMarker
        {
            return CreateInstance<T>(screenPosition/*, subjects*/, null);
        }
        
        public static T CreateInstance<T>(Vector3 screenPosition/*, IEnumerable<ISelectable> subjects*/, TaskMarker previousTaskMarker)
            where T : TaskMarker
        {
            T res = Instantiate(
                GameObject.Find("ResourcesList").GetComponent<ResourcesListComponent>().taskMarkerPrefab,
                GameObject.Find("UI Screen Canvas").GetComponent<RectTransform>())
                .gameObject.AddComponent<T>();

            res.Init(screenPosition/*, subjects*/, previousTaskMarker);

            return res;//.GetRefWrapper().CastWrapper<T>();
        }

        protected static int _instcount = 0;

        protected bool isEditing = false;

        protected FactionAffiliation factionAffiliation;
        protected UnityEngine.UI.Graphic uiGraphic;

        public EasyObserver<string> OnExitPlacementUIMode = new EasyObserver<string>();
        //public event Action OnExitPlacementUIMode;
        public EasyObserver<string, bool> OnPlacementConfirmation = new EasyObserver<string, bool>();

        protected virtual void Init(Vector3 screenPosition/*, IEnumerable<ISelectable> subjects*/, TaskMarker previousTaskMarker)
        {
            /*OnExitEditMode.SubscribeToEvent("oneditexitdeselectall",
                () => 
                {
                    foreach (var v in GetUsedSelector().GetCurrentlySelectedEntitiesOfType<TaskMarkerWrapper>())
                        GetUsedSelector().DeselectEntity(v);
                });*/
        }

        //public abstract TaskPlan2 GetTaskPlan();

        public abstract Task GetTask();

        //public abstract IEnumerable<ITaskSubject> GetTaskSubjects();

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

        protected bool ready = false;
        protected bool expanded = false;

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

        protected virtual void DrawUpdate(Color _initialColor)
        {
            if (uiGraphic != null)
            {
                uiGraphic.color = _initialColor;
                
                if (GetUsedSelector().IsSelected(this))
                {
                    uiGraphic.color = factionAffiliation.MyFaction.baseColor;
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

        protected bool paused = false;   

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
    }
}
