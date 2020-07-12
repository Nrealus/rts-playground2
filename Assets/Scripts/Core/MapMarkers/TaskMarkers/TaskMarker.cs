using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Tasks;
using Core.Units;
using Core.Selection;
using Core.Handlers;
using VariousUtilsExtensions;
using UnityEngine.EventSystems;
using System.Text;
using Core.Helpers;
using Core.MapMarkers;

namespace Core.MapMarkers
{
    /****** Author : nrealus ****** Last documentation update : 14-05-2020 ******/

    /// <summary>
    /// A MapMarker subclass, used as "widgets" for Orders, or information windows about them.
    /// They are linked to an OrderWrapper. By default, they follow the position of the order's receiver.
    /// For a UnitGroup receiver, the default position is the mean position of all the units in the group.
    /// </summary>   
    public abstract class TaskMarker : MapMarker, IHasRefWrapper<TaskMarkerWrapper>
    {

        public new TaskMarkerWrapper GetRefWrapper()
        {
            return _myWrapper as TaskMarkerWrapper;
        }

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

        public static TaskMarkerWrapper<T> CreateInstance<T>(Vector3 screenPosition, List<ISelectable> subjects) where T : TaskMarker
        {
            return CreateInstance<T>(screenPosition, subjects, null);
        }
        
        public static TaskMarkerWrapper<T> CreateInstance<T>(Vector3 screenPosition, List<ISelectable> subjects, TaskMarkerWrapper previousTaskMarker)
            where T : TaskMarker
        {
            T res = Instantiate(
                GameObject.Find("ResourcesList").GetComponent<ResourcesListComponent>().taskMarkerPrefab,
                GameObject.Find("UI Screen Canvas").GetComponent<RectTransform>())
                .gameObject.AddComponent<T>();

            res.Init(screenPosition, subjects, previousTaskMarker);

            return res.GetRefWrapper().CastWrapper<T>();
        }

        protected abstract void Init(Vector3 screenPosition, List<ISelectable> subjects, TaskMarkerWrapper previousTaskMarker);

        protected static int _instcount = 0;

        public abstract TaskPlan GetTaskPlan();

        protected abstract TaskWrapper GetTaskWrapper();

        protected bool isEditing = false;

        public void EnterEditMode()
        {
            isEditing = true;
        }

        public void ExitEditMode()
        {
            isEditing = false;
        }

        protected bool ready = false;
        protected bool expanded = false;

        protected List<ISelectable> subjects;

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
                            ExitEditMode();
                            GetRefWrapper().DestroyWrappedReference();
                        }
                        PlaceAtScreenPosition(UIHandler.GetPointedScreenPosition());                
                    }
                }
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
            GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 36);
            GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 36);
            expanded = false;
        }

        protected void PlaceAtScreenPosition(Vector3 screenPosition)
        {
            GetComponent<RectTransform>().SetPositionOfPivotFromViewportPosition(GetScreenCanvasRT(),
                GetMyCamera().ScreenToViewportPoint(screenPosition)); 
        }
    }
}
