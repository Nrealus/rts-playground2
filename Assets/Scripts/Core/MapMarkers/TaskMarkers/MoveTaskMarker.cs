using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Tasks;
using Core.MapMarkers;
using Core.Units;
using Core.Selection;
using Core.Handlers;
using Nrealus.Extensions;
using UnityEngine.EventSystems;
using System.Text;
using Core.Helpers;
using Core.Faction;

namespace Core.MapMarkers
{

    /****** Author : nrealus ****** Last documentation update : 25-07-2020 ******/

    /// <summary>
    /// A subclass of TaskMarker, specific to MoveTask tasks. (NEEDS MORE DETAILS)
    /// </summary>   
    public class MoveTaskMarker : TaskMarker
    {

        private TaskWrapper<MoveTask> _associatedTaskWrapper;
        public override Task GetTask() { return _associatedTaskWrapper.Value; }
        public MoveTask GetTaskAsMoveTask() { return _associatedTaskWrapper.Value; }

        private string onPauseEventKey;
        private string onUnpauseEventKey;
        private string onClearanceSubjectsKey;
        
        private MapMarkerWrapper<TaskMarker> _previousTaskMarkerWrapper;
        public TaskMarker GetPreviousTaskMarker() { return _previousTaskMarkerWrapper?.Value; }

        public List<WaypointMarker> waypointMarkersList = new List<WaypointMarker>();

        private Color _initialColor; 
        private void InitFirstHalf()
        {
            ready = true;

            _instcount++;

            Contract();

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { OnPointerClickDelegate((PointerEventData)data); entry.callback.RemoveAllListeners(); });
            GetComponentInChildren<EventTrigger>().triggers.Add(entry);

            onPauseEventKey = (new StringBuilder("tmw")).Append(_instcount).ToString();
            onUnpauseEventKey = (new StringBuilder("tmw")).Append(-_instcount).ToString();            
            UIHandler.SubscribeOnPause(onPauseEventKey,() => paused = true);
            UIHandler.SubscribeOnUnpause(onUnpauseEventKey,() => paused = false);
        }

        protected override void Init(Vector3 position, TaskMarker _previousTaskMarker, bool screenPosTrue)
        {
            base.Init(position, _previousTaskMarker, screenPosTrue);

            InitFirstHalf();

            factionAffiliation = GetComponent<FactionAffiliation>();
            uiGraphic = GetComponentInChildren<UnityEngine.UI.Image>();
            _initialColor = uiGraphic.color;

            if (screenPosTrue)
                PlaceAtScreenPosition(position);
            else
                PlaceAtWorldPosition(position);

            _associatedTaskWrapper = new TaskWrapper<MoveTask>(Task.CreateTask<MoveTask>());
            //Task.CreateTaskWrapperAndSetReceiver<MoveTask>(myTaskSubjectsList[0]);

            if (_previousTaskMarker != null)
            {
                _previousTaskMarkerWrapper = new MapMarkerWrapper<TaskMarker>(_previousTaskMarker);
                //GetPreviousTaskMarker().GetTask().GetTaskPlan().AddTaskToPlan(GetTask());
            }

            GetTaskAsMoveTask().SetMoveTaskMarker(this);

            GetTask().SubscribeOnDestruction("taskmarkerclear", DestroyThis);
            //GetRefWrapper().SubscribeOnClearance(DestroyMe);

        }

        public override void DestroyThis()
        {
            UIHandler.UnsubscribeOnPause(onPauseEventKey);
            UIHandler.UnsubscribeOnUnpause(onUnpauseEventKey);
            base.DestroyThis();
            _previousTaskMarkerWrapper = null;
            _associatedTaskWrapper = null;
        }

        protected override void UpdateMe()
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
                            ConfirmPositioning(false);
                            DestroyThis();
                        }
                        //PlaceAtWorldPosition(UIHandler.GetPointedWorldPosition()); same result confirmed
                        PlaceAtScreenPosition(UIHandler.GetPointedScreenPosition());
                    }
                }
            }

            DrawUpdate(_initialColor);
        }

        protected override void DrawUpdate(Color _initialColor)
        {
            base.DrawUpdate(_initialColor);

            if (GetPreviousTaskMarker() != null)
            {
                Debug.DrawLine(GetPreviousTaskMarker().transform.position, transform.position);
            }

        }

        private void OnPointerClickDelegate(PointerEventData data)
        {
            if (!paused)
            {
                if (isEditing)
                {
                    if (UIHandler.EventSystemPointedGameObjects().Count > 1)
                    {
                        ExitPlacementUIMode();
                        ConfirmPositioning(false);
                        DestroyThis();
                    }
                    else// if (myTaskSubjectsList.Count > 0)
                    {            
                        //GetTaskAsMoveTask().AddWaypoints(waypointMarkersList);
                        AddWaypoint(WaypointMarker.CreateInstance(GetWorldPosition()));

                        ExitPlacementUIMode();
                        ConfirmPositioning(true);
                    }
                }
                else
                {
                    if (expanded)
                    {
                        SelectionHandler.GetUsedSelector().DeselectEntity(this);
                        expanded = false;
                        //Contract();
                    }
                    else
                    {
                        SelectionHandler.GetUsedSelector().SelectEntity(this);
                        expanded = true;
                        //Expand();
                    }
                }
            }
        }

        public void AddWaypoint(WaypointMarker wp)
        {
            if (!waypointMarkersList.Contains(wp))
            {
                SubscribeOnDestruction(wp.GetInstanceID().ToString(), () => RemoveClearedWaypointWrapper(wp));
                this.waypointMarkersList.Add(wp);
            }
        }

        private void RemoveClearedWaypointWrapper(WaypointMarker wp)
        {
            if (waypointMarkersList.Contains(wp))
            {
                waypointMarkersList.Remove(wp);
                UnsubscribeOnDestruction(wp.GetInstanceID().ToString());
                wp.DestroyThis();
            }
        }

    }
}
