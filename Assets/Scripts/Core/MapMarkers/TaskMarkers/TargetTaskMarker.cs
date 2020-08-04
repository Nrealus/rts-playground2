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
    /// A subclass of TaskMarker (NEEDS MORE DETAILS)
    /// </summary>   
    public class TargetTaskMarker : TaskMarker
    {

        private TaskWrapper<Task> _associatedTaskWrapper;
        public override Task GetTask() { return _associatedTaskWrapper.Value; }

        #region Main declarations

        private string onPauseEventKey;
        private string onUnpauseEventKey;
        private string onClearanceSubjectsKey;
        
        public override TaskMarker GetPreviousTaskMarker() { return GetTask().GetTaskPlan()?.GetTaskInPlanBefore(GetTask())?.GetTaskMarker(); }

        public List<FirePositionMarker> firePositionMarkers = new List<FirePositionMarker>();

        private Color initialColor; 

        #endregion

        #region Initialisation

        private static int _instcount = 0;
        private void InitPreparation()
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

            factionAffiliation = GetComponent<FactionAffiliation>();
            uiGraphic = GetComponentInChildren<UnityEngine.UI.Image>();
            initialColor = uiGraphic.color;
        }

        protected override void Init(Vector3 position, bool screenPosTrue)
        {
            base.Init(position, screenPosTrue);

            InitPreparation();

            if (screenPosTrue)
                PlaceAtScreenPosition(position);
            else
                PlaceAtWorldPosition(position);

            _associatedTaskWrapper = new TaskWrapper(Task.CreateTask<EngageAtPositionsTask>());
            //Task.CreateTaskWrapperAndSetReceiver<MoveTask>(myTaskSubjectsList[0]);

            GetTask().SetTaskMarker(this);
            GetTask().SubscribeOnDestruction("taskmarkerclear", DestroyThis);
            //GetRefWrapper().SubscribeOnClearance(DestroyMe);

        }

        #endregion

        public override void DestroyThis()
        {
            UIHandler.UnsubscribeOnPause(onPauseEventKey);
            UIHandler.UnsubscribeOnUnpause(onUnpauseEventKey);
            base.DestroyThis();
            _associatedTaskWrapper = null;
        }

        #region Behaviour methods

        protected override void UpdateMe()
        {
            DrawUpdate(initialColor);

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
                        AddTargetMarker(FirePositionMarker.CreateFirePositionMarker(GetWorldPosition(), 10f));

                        if (!Input.GetKey(KeyCode.LeftShift))
                        {
                            ExitPlacementUIMode();
                            ConfirmPositioning(true);
                        }
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

        #endregion

        public void AddTargetMarker(FirePositionMarker wp)
        {
            if (!firePositionMarkers.Contains(wp))
            {
                SubscribeOnDestruction(wp.GetInstanceID().ToString(), () => RemoveClearedTargetMarker(wp));
                this.firePositionMarkers.Add(wp);
            }
        }

        private void RemoveClearedTargetMarker(FirePositionMarker wp)
        {
            if (firePositionMarkers.Contains(wp))
            {
                firePositionMarkers.Remove(wp);
                UnsubscribeOnDestruction(wp.GetInstanceID().ToString());
                wp.DestroyThis();
            }
        }

    }
}
