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

    /****** Author : nrealus ****** Last documentation update : 12-07-2020 ******/

    /// <summary>
    /// A subclass of TaskMarker, specific to MoveTask tasks. (NEEDS MORE DETAILS)
    /// </summary>   
    public class MoveTaskMarker : TaskMarker, IHasRefWrapper<TaskMarkerWrapper<MoveTaskMarker>>
    {

        public new TaskMarkerWrapper<MoveTaskMarker> GetRefWrapper()
        {
            return _myWrapper as TaskMarkerWrapper<MoveTaskMarker>;
        }

        private TaskPlan _myTaskPlan;
        public override TaskPlan GetTaskPlan() { return _myTaskPlan; }

        private TaskWrapper<MoveTask> _myTaskWrapper;
        protected override TaskWrapper GetTaskWrapper() { return _myTaskWrapper; }

        private List<UnitWrapper> myTaskSubjectsList = new List<UnitWrapper>();
        public override IEnumerable<ITaskSubject> GetTaskSubjects() { return myTaskSubjectsList; }

        private string onPauseEventKey;
        private string onUnpauseEventKey;
        private string onClearanceSubjectsKey;
        
        private List<MapMarkerWrapper<WaypointMarker>> waypointMarkerWrappersList = new List<MapMarkerWrapper<WaypointMarker>>();

        private Color _initialColor;
        protected override void Init(Vector3 screenPosition, IEnumerable<ISelectable> subjects, TaskMarkerWrapper previousTaskMarker)
        {
            base.Init(screenPosition, subjects, previousTaskMarker);

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

            factionAffiliation = GetComponent<FactionAffiliation>();
            uiGraphic = GetComponentInChildren<UnityEngine.UI.Image>();
            _initialColor = uiGraphic.color;

            PlaceAtScreenPosition(screenPosition);

            onClearanceSubjectsKey += (new StringBuilder("rmvsubj")).Append(_instcount).ToString();
            foreach(var v in subjects)
            {
                var uw = v.GetSelectableAsReferenceWrapperSpecific<UnitWrapper>();
                uw.SubscribeOnClearance(onClearanceSubjectsKey,() => _RemoveSubjectAndUnsubscribe(uw));
                myTaskSubjectsList.Add(uw);
            }

            _myWrapper = new TaskMarkerWrapper<MoveTaskMarker>(this, () => {_myWrapper = null;});

            ready = true;

            if (previousTaskMarker == null)
            {
                _myTaskPlan = new TaskPlan();
            }
            else
            {
                _myTaskPlan = previousTaskMarker.GetWrappedReference().GetTaskPlan();//.GetTaskPlan();
            }
            //_myTaskWrapper = Task.CreateTaskWrapperWithoutSubject<MoveTask>();
            //Task.SetSubject(GetTaskWrapper(), null, null, this.subjects[0].GetSelectableAsReferenceWrapperSpecific<UnitWrapper>());
            _myTaskWrapper = Task.CreateTaskWrapperAndSetTaskMarker<MoveTask>(
                //this.subjects[0].GetSelectableAsReferenceWrapperSpecific<UnitWrapper>(),
                GetRefWrapper());

            GetTaskWrapper().SubscribeOnClearance("taskmarkerclear",() => { GetRefWrapper().DestroyWrappedReference(); _myTaskWrapper = null; });

            GetRefWrapper().SubscribeOnClearance(DestroyMe);

        }

        private void _RemoveSubjectAndUnsubscribe(UnitWrapper uw)
        {
            myTaskSubjectsList.Remove(uw);
            uw.UnsubscribeOnClearance(onClearanceSubjectsKey);
        }

        protected override void DestroyMe()
        {
            UIHandler.UnsubscribeOnPause(onPauseEventKey);
            UIHandler.UnsubscribeOnUnpause(onUnpauseEventKey);
            _myTaskPlan = null;
            Destroy(gameObject);
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
                            ExitEditMode();
                            GetRefWrapper().DestroyWrappedReference();
                        }
                        PlaceAtScreenPosition(UIHandler.GetPointedScreenPosition());                
                    }
                }
            }

            DrawUpdate(_initialColor);
        }

        private void OnPointerClickDelegate(PointerEventData data)
        {
            if (!paused)
            {
                if (isEditing)
                {
                    if (UIHandler.EventSystemPointedGameObjects().Count > 1)
                    {
                        ExitEditMode();
                        GetRefWrapper().DestroyWrappedReference();
                    }
                    else if (myTaskSubjectsList.Count > 0)
                    {
                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            var v = MoveTaskMarker.CreateInstance<MoveTaskMarker>(UIHandler.GetPointedScreenPosition(), myTaskSubjectsList, GetRefWrapper());
                            
                            v.GetWrappedReference().ConfirmAndExecute();
                            
                            Task.GetParameters(v.GetWrappedReference().GetTaskWrapper()).AddExecutionMode(TaskParams.TaskExecutionMode.Chain);
                            
                            v.GetWrappedReference().GetTaskPlan().QueueActiveOrderToPlan(v.GetWrappedReference().GetTaskWrapper(), null, null);
                        }
                        else
                        {
                            ConfirmAndExecute();
                           
                            Task.GetParameters(GetTaskWrapper()).AddExecutionMode(TaskParams.TaskExecutionMode.Chain);

                            GetTaskPlan().QueueActiveOrderToPlan(GetTaskWrapper(), null, null);
                            
                            Task.GetSubject(GetTaskWrapper()).GetTaskPlan().StopActiveExecution();

                            Task.GetSubject(GetTaskWrapper()).SetTaskPlan(GetTaskPlan());
                            GetTaskPlan().StartActiveExecution();
                            //Task.TryStartExecution(GetTaskPlan().GetFirstInlineActiveTaskInPlan());
                        }
                    }
                }
                else
                {
                    if (expanded)
                    {
                        SelectionHandler.GetUsedSelector().DeselectEntity(GetRefWrapper());
                        expanded = false;
                        //Contract();
                    }
                    else
                    {
                        SelectionHandler.GetUsedSelector().SelectEntity(GetRefWrapper());
                        expanded = true;
                        //Expand();
                    }
                }
            }
        }

        private void ConfirmAndExecute()
        {
            SelectionHandler.GetUsedSelector().DeselectEntity(GetRefWrapper());
            /*foreach (var v in subjects)
            {
                TaskFactory.CreateTaskWrapperAndSetReceiver<MoveTask>((ITaskSubject<Unit>)v.GetSelectableAsReferenceWrapperSpecific<UnitWrapper>());
            }*/            
            ExitEditMode();
            
            AddWaypoint(WaypointMarker.CreateInstance(UIHandler.GetPointedWorldPosition()).GetRefWrapper());

            GetTaskWrapper().GetCastReference<MoveTask>().AddWaypoints(waypointMarkerWrappersList);

            //Task.GetSubject(myTaskWrapper).SetTaskPlan(myTaskPlan);
            //Task.TryStartExecution(myTaskWrapper);
        }

        private void AddWaypoint(MapMarkerWrapper<WaypointMarker> waypointWrapper)
        {
            if (!waypointMarkerWrappersList.Contains(waypointWrapper))
            {
                GetRefWrapper().SubscribeOnClearance(() => RemoveClearedWaypointWrapper(waypointWrapper));
                this.waypointMarkerWrappersList.Add(waypointWrapper);
            }
        }

        private void RemoveClearedWaypointWrapper(MapMarkerWrapper<WaypointMarker> waypointWrapper)
        {
            if (waypointMarkerWrappersList.Contains(waypointWrapper))
            {
                waypointMarkerWrappersList.Remove(waypointWrapper);
            }
        }

    }
}
