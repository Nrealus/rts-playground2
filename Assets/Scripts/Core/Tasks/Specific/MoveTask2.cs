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
    public class MoveTask2 : Task2
    {

        #region Main declarations

        private UnitGroup actorGroupAsUG { get { return GetTaskPlan().GetActorGroup() as UnitGroup; } }
        private List<Unit> units { get { return actorGroupAsUG.GetActorsAsUnits(); }}

        public int currentWaypointIndex;
        public bool endedPath;

        private List<MoveTaskMarker> subActorsMoveTaskMarkers = new List<MoveTaskMarker>();

        #endregion

        public MoveTask2()
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

        public bool CompatibleForParallelExecution(Task task)
        {
            bool b = false;
            /*if (task is MoveTask2)
            {
                MoveTask2 mvtsk = task as MoveTask2;

                var v = units

                var chus = subActorsMoveTaskMarkers.Select(_ => _.GetTask().GetActorGroup<UnitGroup>().GetActors()[0]);
                if (mvtsk.unit == unit
                || mvtsk.actorGroupAsUG.GetSubGroups().Select(_ => _.GetActors()[0]).Intersect(chus).Count() > 0)
                    return false;
            }
            bool b = true;
            foreach (var v in subActorsMoveTaskMarkers)
            {
                b = b && v.GetTask().CompatibleForParallelExecution(task);
            }
            return true && b;*/
            /*foreach (var subAgent in GetSubjectAgents())
            {
                b = b || CompatibilityPerSubject(subAgent, task);
            }
            */
            return b;
        }

        public bool SolveParallelCompatibilityConflicts()
        {
            foreach (var pl in new List<TaskPlan2>(actorGroupAsUG.GetActorsGroupsAndSubGroupsPlans(true, true)))
            {
                if (pl.GetCurrentTaskInPlan() != this)
                {
                    MoveTask2 mvtsk = pl.GetCurrentTaskInPlan() as MoveTask2;
                    UnitGroup ug = mvtsk.GetActorGroup() as UnitGroup;
                    if (mvtsk != null/* && ug != null*/)
                    {
                        foreach (var u in new List<Unit>(units))
                        {
                            if (mvtsk.units.Contains(u))
                                ug.RemoveUnitFromGroup(u);
                        }
                        /*var intsect = mvtsk.units.Intersect(units);
                        foreach (var u in intsect)
                        {
                            ug.RemoveUnitFromGroup(u);
                        }*/
                    }

                    if (ug != null && ug.GetActors().Count == 0)
                    {
                        foreach (var subg in new List<UnitGroup>(ug.GetSubGroupsAsUG()))
                            subg.ChangeParentTo(null);
                        
                        pl.EndPlanExecution();
                    }
                }
            }

            return false;
        }

        /*private bool CompatibilityPerActor(IActor actor, Task task)
        {
            bool b = true;
            if (task is MoveTask2)
            {
                var mvtsk = task as MoveTask2;
                var subut = (actor as UnitGroup);

                //b = !((task.GetAgent() as UnitTeam).GetUnit() == subut.GetUnit()
                //    || mvtsk.agentAsTeam.GetUnits().Where(_ => (_ as UnitTeam).GetUnit() == subut.GetUnit()).Count() > 0);
            }
            return b;
        }*/

        protected override void InstanceSetTaskPlan(TaskPlan2 taskPlan)
        {
            base.InstanceSetTaskPlan(taskPlan);

            if (taskPlan != null)
            {
                Debug.Log("setting task plan to task");

                SubscribeOnDestruction("clearchildrenmovetaskmarkers", () => subActorsMoveTaskMarkers.Clear());
    
                foreach (var u in units)
                {
                    /*actorGroupAsUG.SubscribeOnUnitRemovalFromGroup(u,
                        _ => 
                        {
                            subActorsMoveTaskMarkers.Remove(u);
                            actorGroupAsUG.UnsubscribeOnUnitRemovalFromGroup(_);
                        });*/

                    //UpdateFormationFacing(u, GetTaskMarker().GetWorldPosition());
                    //u.GetFormation().FormTest();
                }

                foreach (var subg in actorGroupAsUG.GetSubGroupsAsUG())
                {
                    Vector3 wpos;
                    wpos = subg.GetActorsAsUnits()[0].GetFormation().GetParentFormation().GetAcceptableMovementTargetPosition(GetTaskMarker().GetWorldPosition());

                    var prevtm = GetTaskMarker().GetPreviousTaskMarker()?.GetTask() as MoveTask2;
                    
                    TaskMarker chprevtm;
                    if (prevtm != null)
                        chprevtm = prevtm.subActorsMoveTaskMarkers.FirstOrDefault(_ => (UnitGroup)_.GetTask().GetActorGroup() == subg);
                    else
                        chprevtm = null;

                    MoveTaskMarker tm = TaskMarker.CreateInstanceAtWorldPosition<MoveTaskMarker>(wpos);
                    tm.AddWaypointMarker(WaypointMarker.CreateWaypointMarker(wpos));

                    TaskPlan2 chtp = tm.InsertAssociatedTaskIntoPlan(subg, chprevtm);

                    tm.SubscribeOnDestruction("removefromparentmovetaskmarkerslist",() => subActorsMoveTaskMarkers.Remove(tm));                    
                    subActorsMoveTaskMarkers.Add(tm);
                }

                SetPhase(TaskPhase.Staging);
            }
            else
            {
                subActorsMoveTaskMarkers.Clear();
            }

            if (taskPlan != null)
            {
                //AddSubjectAgent(GetAgent());

                /*foreach (var sbt in agentAsTeam.GetAllSubTeamsBFS())
                {
                    AddSubjectAgent(sbt);
                }*/

                /*foreach (var subAgent in GetSubjectAgents())
                {
                    var sbt = subAgent as UnitTeam;
                    if (!sbt.IsVirtualTeam())
                        sbt.GetUnit().GetFormation().FormTest();
                }
                foreach (var subAgent in GetSubjectAgents())
                {
                    Vector3 wpos;
                    wpos = (subAgent as UnitTeam).GetUnit().GetFormation().GetAcceptableMovementTargetPosition(GetTaskMarker().GetWorldPosition());
                }*/

                //SetPhase(TaskPhase.Staging);
            }
        }

        protected override bool InstanceTryStartExecution()
        {
            
            if (IsInPhase(TaskPhase.Staging) && actorGroupAsUG != null)
            {
                SolveParallelCompatibilityConflicts();

                if(GetTaskPlan().GetCurrentTaskInPlan() == this)
                {
                    // for testing purposes
                    //GetParameters().plannedStartingTime = TimeHandler.CurrentTime() + new TimeStruct(0,0,10);

                    SetPhase(TaskPhase.WaitToStartExecution);

                    foreach (var chmtm in subActorsMoveTaskMarkers)
                        chmtm.GetTask().TryStartExecution();

                    return true;
                }
            }

            return false;

            /*if (IsInPhase(TaskPhase.Staging) && GetActorGroup() != null)
            {
                bool b = true;

                foreach (var subAgent in new List<ITaskAgent>(GetSubjectAgents()))
                {
                    foreach (var plan in new List<TaskPlan2>(subAgent.GetOwnedPlans()))
                    {
                        if (plan.GetCurrentTaskInPlan() != this
                            && !CompatibilityPerSubjectAgent(subAgent, plan.GetCurrentTaskInPlan()))
                        {
                            //b = false;
                            plan.EndPlanExecution();
                        }                        
                    }
                    
                    foreach (var task in new List<Task>(subAgent.GetTasksWhereIsInternalSubject()))
                    {
                        if (task != this
                            && !CompatibilityPerSubjectAgent(subAgent, task))
                        {
                            Debug.Log(task.GetSubjectAgents().Count);
                            task.RemoveSubjectAgent(task.GetSubjectAgents().First(_ => (_ as UnitTeam).GetUnit() == (subAgent as UnitTeam).GetUnit()));
                            Debug.Log(task.GetSubjectAgents().Count);
                            //task.GetTaskPlan().EndPlanExecution();
                            //b = false;
                        }
                    }
                }

                if(GetTaskPlan().GetCurrentTaskInPlan() == this && b)
                {
                    SetPhase(TaskPhase.WaitToStartExecution);
                    return true;
                }
            }*/
        }

        #endregion

        #region Specific behaviour logic

        private bool _ended = false;
        protected override void UpdateExecution()
        {
            if (actorGroupAsUG.IsLeaf())
            {
                if (IsInPhase(TaskPhase.Execution))
                {
                    foreach (var u in units)
                    {
                        if (PathExists(u) && !PathFinished(u))
                        {
                            Vector3 wpos;
                            //wpos = u.GetFormation().GetAcceptableMovementTargetPosition(GetWaypointMarkersList()[currentWaypointIndex].transform.position);
                            wpos = GetWaypointMarkersList()[currentWaypointIndex].transform.position;

                            NavigateAlongPath(u, wpos);
                            
                            _ended = false;
                        }
                        else
                        {
                            _ended = true;
                        }
                    }
                }
            }
            else
            {
                _ended = false;
                foreach (var v in subActorsMoveTaskMarkers)
                {
                    _ended = true;
                    if (!(v.GetTask() as MoveTask2)._ended)
                    {
                        _ended = false;
                        break;//&& v.GetTask().IsInPhase(Task.TaskPhase.Disposed);
                    }
                }
            }

            if (_ended && actorGroupAsUG.GetParentGroup() == null)
            {
                Debug.Log("hhhhhh");
                EndExecution();
            }

        }

        private bool PathExists(Unit u)
        {
            return true;
        }
        
        private bool PathFinished(Unit u)
        {
            return currentWaypointIndex >= GetWaypointMarkersList().Count;
        }

        private float s = 0.05f;
        private void NavigateAlongPath(Unit u, Vector3 wpos)
        {   
            //var wpos = GetWaypointMarkersList()[currentWaypointIndex].transform.position;

            var targetPos = wpos;//GetUnitSubject().GetFormation().GetAcceptableMovementTargetPosition(wpos);

            u.myMover.MoveToPosition(targetPos, s);

            //UpdateFormationFacing(u, targetPos);

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