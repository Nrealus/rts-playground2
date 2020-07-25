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

        //private List<UnitWrapper> myTaskSubjectsList = new List<UnitWrapper>();
        //public override IEnumerable<ITaskSubject> GetTaskSubjects() { return myTaskSubjectsList; }

        private string onPauseEventKey;
        private string onUnpauseEventKey;
        private string onClearanceSubjectsKey;
        
        private List<WaypointMarker> waypointMarkersList = new List<WaypointMarker>();

        private Color _initialColor; 
        private void InitFirstHalf()
        {
            ready = true;

            _instcount++;

            Contract();

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { OnPointerClickDelegate((PointerEventData)data); });
            GetComponentInChildren<EventTrigger>().triggers.Add(entry);

            onPauseEventKey = (new StringBuilder("tmw")).Append(_instcount).ToString();
            onUnpauseEventKey = (new StringBuilder("tmw")).Append(-_instcount).ToString();            
            UIHandler.SubscribeOnPause(onPauseEventKey,() => paused = true);
            UIHandler.SubscribeOnUnpause(onUnpauseEventKey,() => paused = false);
        }

        protected override void Init(Vector3 screenPosition/*, IEnumerable<ISelectable> subjects*/, TaskMarker _previousTaskMarker)
        {
            base.Init(screenPosition/*, subjects*/, _previousTaskMarker);

            InitFirstHalf();

            factionAffiliation = GetComponent<FactionAffiliation>();
            uiGraphic = GetComponentInChildren<UnityEngine.UI.Image>();
            _initialColor = uiGraphic.color;

            PlaceAtScreenPosition(screenPosition);

            /*onClearanceSubjectsKey += (new StringBuilder("rmvsubj")).Append(_instcount).ToString();

            foreach(var v in subjects)
            {
                var uw = v.GetSelectableAsReferenceWrapperSpecific<UnitWrapper>();
                uw.SubscribeOnClearance(onClearanceSubjectsKey,() => RemoveAndUnsubSubject(uw));
                myTaskSubjectsList.Add(uw);
            }*/

            /*if (_previousTaskMarkerWrapper == null)
            {
                //_myTaskPlan = new TaskPlan2();
            }
            else
            {
                previousTaskMarkerWrapper = _previousTaskMarkerWrapper;
                previousTaskMarkerWrapper.SubscribeOnClearance(() => previousTaskMarkerWrapper = null);
                //_myTaskPlan = _previousTaskMarkerWrapper.GetWrappedReference().GetTaskPlan();//.GetTaskPlan();
            }*/

            _associatedTaskWrapper = new TaskWrapper<MoveTask>(Task.CreateTask<MoveTask>());//Task.CreateTaskWrapperAndSetReceiver<MoveTask>(myTaskSubjectsList[0]);

            GetTask().SubscribeOnDestruction("taskmarkerclear", DestroyThis);
            //GetRefWrapper().SubscribeOnClearance(DestroyMe);

        }

        /*private void RemoveAndUnsubSubject(UnitWrapper uw)
        {
            myTaskSubjectsList.Remove(uw);
            uw.UnsubscribeOnClearance(onClearanceSubjectsKey);
        }*/

        public override void DestroyThis()
        {
            UIHandler.UnsubscribeOnPause(onPauseEventKey);
            UIHandler.UnsubscribeOnUnpause(onUnpauseEventKey);
            //successorTaskMarker = null;
            //_myTaskPlan = null;
            base.DestroyThis();
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
                        PlaceAtScreenPosition(UIHandler.GetPointedScreenPosition());                
                    }
                }
            }

            DrawUpdate(_initialColor);
        }

        protected override void DrawUpdate(Color _initialColor)
        {
            base.DrawUpdate(_initialColor);

            /*if (successorTaskMarkerWrapper != null)
            {
                Debug.DrawLine(transform.position, successorTaskMarkerWrapper.GetWrappedReference().transform.position);
            }*/

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
                        AddWaypoint(WaypointMarker.CreateInstance(UIHandler.GetPointedWorldPosition()));
                        (GetTask() as MoveTask).AddWaypoints(waypointMarkersList);

                        /*var taskPlan = new TaskPlan2();
                        taskPlan.AddTaskToPlan(GetTaskWrapper());
                        
                        Task.GetSubject(GetTaskWrapper()).GetTaskPlan().StopPlanExecution();

                        Task.GetSubject(GetTaskWrapper()).SetTaskPlan(GetTaskPlan());
                        GetTaskPlan().StartPlanExecution();*/

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

        private void AddWaypoint(WaypointMarker wp)
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
                //UnsubscribeOnDestruction(wp.GetInstanceID().ToString());
            }
        }

    }
}
