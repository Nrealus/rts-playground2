using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nrealus.Extensions;
using NPBehave;
using Core.MapMarkers;
using Core.Handlers;
using Core.Helpers;
using System.Linq;
using Core.Formations;

namespace Core.Tasks
{

    /****** Author : nrealus ****** Last documentation update : 25-07-2020 ******/

    /// <summary>
    /// Currently being a little bit redesigned (as lots of things)
    /// This "active" Task allows units to move on the map, given waypoints. It's one of the most basic and "important" tasks.
    /// </summary>
    public class MoveTask : Task2
    {

        #region Main declarations

        private UnitGroup actorGroupAsUG { get { return GetActorGroup<UnitGroup>(); } }
        private Unit unit { get { return actorGroupAsUG.GetActorsAsUnits()[0]; }}

        public int currentWaypointIndex;

        private bool ended = false;

        //public bool endedPath;
        
        private MoveTask parentActorMoveTask;
        //private List<MoveTaskMarker> subActorsMoveTaskMarkers = new List<MoveTaskMarker>();
        private Dictionary<UnitGroup,MoveTask> subActorsMoveTasks = new Dictionary<UnitGroup,MoveTask>();

        #endregion

        public MoveTask()
        {
            CreateAndInitFSM();
            SubscribeOnDestructionLate("clearparams", () => GetParameters().RemoveParameterActors(GetParameters().GetParameterActors()));
        }
        
        #region Instance methods and functions

        private MapMarkerWrapper<MoveTaskMarker> _moveTaskMarkerWrapper;
        protected override TaskMarker InstanceGetTaskMarker()
        {
            return _moveTaskMarkerWrapper?.Value;
        }

        protected override void InstanceSetTaskMarker(TaskMarker taskMarker)
        {
            _moveTaskMarkerWrapper = new MapMarkerWrapper<MoveTaskMarker>(taskMarker as MoveTaskMarker);
        }

        public List<WaypointMarker> GetWaypointMarkersList()
        {
            return _moveTaskMarkerWrapper.Value.waypointMarkersList;
        }

        private TaskParams _taskParams = TaskParams.DefaultParam();
        protected override TaskParams InstanceGetParameters()
        {   
            return _taskParams;
        }

        public int SolveParallelCompatibilityConflicts()
        {
            foreach (var pl in new List<TaskPlan2>(unit.GetParticipatedGroupsPlans()))
            {
                if (pl.GetCurrentTaskInPlan() != this)
                {
                    UnitGroup ug = pl.GetCurrentTaskInPlan().GetActorGroup() as UnitGroup;

                    MoveTask mvtsk = pl.GetCurrentTaskInPlan() as MoveTask;
                    if (mvtsk != null && GetActorGroup() != mvtsk.GetActorGroup())
                    {
                        if (mvtsk.unit == unit && unit.IsLeaf())
                        {
                            mvtsk.actorGroupAsUG.RemoveUnitFromGroup(unit);
                            return 1;
                        }
                    }
                }
            }

            return 0;
        }

        protected override void InstanceSetTaskPlan(TaskPlan2 taskPlan)
        {
            base.InstanceSetTaskPlan(taskPlan);

            if (taskPlan != null)
            {

                UpdateFormationFacing(unit, GetTaskMarker().GetWorldPosition());

                unit.GetFormation().FormTest();

                foreach (var subg in actorGroupAsUG.GetSubGroupsAsUG())
                {
                    Vector3 wpos;
                    wpos = subg.GetActorsAsUnits()[0].GetFormation().GetAcceptableMovementTargetPosition(GetTaskMarker().GetWorldPosition());
                    
                    MoveTaskMarker tm = TaskMarker.CreateInstanceAtWorldPosition<MoveTaskMarker>(wpos);
                    tm.AddWaypointMarker(WaypointMarker.CreateWaypointMarker(wpos));

                    MoveTask chmvt = tm.GetTask() as MoveTask;

                    chmvt.parentActorMoveTask = this;
                    subActorsMoveTasks.Add(subg, chmvt);

                    chmvt.SubscribeOnDestruction("removeparentssubactorsmovetasks", () => subActorsMoveTasks.Remove(subg));

                    subg.SubscribeOnParentGroupChange("onchangeparentgroup",
                    () =>
                    {
                        chmvt.parentActorMoveTask = null;
                        subActorsMoveTasks.Remove(subg);
                        subg.UnsubscribeOnParentGroupChange("onchangeparentgroup");
                    });
                    
                    var prevtask = GetTaskPlan().GetTaskInPlanBefore(this) as MoveTask;
                    TaskMarker chprevtm;
                    if (prevtask != null && prevtask.subActorsMoveTasks.ContainsKey(subg))
                        chprevtm = prevtask.subActorsMoveTasks[subg].GetTaskMarker();
                    else
                        chprevtm = null;

                    TaskPlan2 chtp = tm.InsertAssociatedTaskIntoPlan(subg, chprevtm);

                }

                /*SubscribeOnDestruction("parentactormovetaskchange", () =>
                {
                    foreach (var chmvt in new List<MoveTask>(subActorsMoveTasks.Values))
                    {
                        chmvt.EndExecution();
                        chmvt.parentActorMoveTask = null;
                    }
                    subActorsMoveTasks.Clear();
                });*/

                SetPhase(TaskPhase.Staging);
            }
            else
            {
                foreach (var chmvt in new List<MoveTask>(subActorsMoveTasks.Values))
                {
                    chmvt.EndExecution();
                    chmvt.parentActorMoveTask = null;
                }
                subActorsMoveTasks.Clear();
            }
        }

        protected override bool InstanceTryStartExecution()
        {
            if (IsInPhase(TaskPhase.Staging) && actorGroupAsUG != null)
            {
                SolveParallelCompatibilityConflicts();

                if(GetTaskPlan().GetCurrentTaskInPlan() == this)
                {
                    SetPhase(TaskPhase.WaitToStartExecution);

                    foreach (var chmtm in subActorsMoveTasks)
                    {
                        chmtm.Value.TryStartExecution();
                    }
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Specific behaviour logic

        protected override void UpdateExecution()
        {
            if (actorGroupAsUG != null)
            {
                /*if (unit.IsLeaf())
                {
                    ended = !(PathExists() && !PathFinished());
                    if (!ended)
                    {
                        NavigateAlongPath(unit);
                    }
                    else
                    {
                        Debug.Log("leaf end");
                        EndExecution();
                    }
                }*/
                if (actorGroupAsUG.IsLeaf() && unit.IsLeaf())
                {
                    ended = !(PathExists() && !PathFinished());
                    if (!ended)
                    {
                        NavigateAlongPath(unit);
                    }
                    else
                    {
                        Debug.Log("leaf end");
                        EndExecution();
                    }
                }
                else
                {
                    /*if (subActorsMoveTasks.Count > 0)
                    {
                        Vector3 v = Vector3.zero;// = GetTaskMarker().transform.position;//.GetWorldPosition();
                        int n = 0;
                        foreach (var s in subActorsMoveTasks)
                        {
                            v += s.Value.GetTaskMarker().GetWorldPosition();
                            n++;
                        }
                        v /= n;
                        GetTaskMarker().PlaceAtWorldPosition(v);
                    }*/

                    bool b = true;
                    foreach(var submvt in subActorsMoveTasks.Values)
                    {
                        if (!submvt.ended)
                        {
                            b = false;
                            break;
                        }
                    }

                    ended = b;

                    if (ended)
                    {
                        Debug.Log("non leaf end");
                        EndExecution();
                    }
                }
            }

        }

        private bool PathExists()
        {
            return true;
        }
        
        private bool PathFinished()
        {
            return currentWaypointIndex >= GetWaypointMarkersList().Count;
        }

        private float s = 0.05f;
        private void NavigateAlongPath(Unit u)
        {   
            var wpos = GetWaypointMarkersList()[currentWaypointIndex].transform.position;

            var targetPos = wpos;//GetUnitSubject().GetFormation().GetAcceptableMovementTargetPosition(wpos);

            u.myMover.MoveToPosition(targetPos, s);

            UpdateFormationFacing(u, targetPos);

            if (u.myMover.DistanceConditionToPosition(targetPos, 0.02f))
            {
                currentWaypointIndex++;
            }
        }
        
        private void UpdateFormationFacing(Unit u, Vector3 targetPos)
        {
            u.GetFormation().facingAngle =
                Vector3.SignedAngle(
                Vector3.right,
                targetPos - u.myMover.transform.position, Vector3.down);
        }

        #endregion
    }
}