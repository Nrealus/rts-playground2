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

        private UnitTeam agentAsTeam { get { return GetTaskPlan().GetOwnerAgent() as UnitTeam; } }

        public int currentWaypointIndex;
        public bool endedPath;
        
        //private List<MoveTaskMarker> childrenMoveTaskMarkers = new List<MoveTaskMarker>();

        #endregion

        public MoveTask2()
        {
            CreateAndInitFSM();
            SubscribeOnDestructionLate("clearparams", () => GetParameters().RemoveParameterAgents(GetParameters().GetParameterAgents()));
        }
        
        #region Instance methods and functions

        private List<ITaskAgent> _subjectAgents = new List<ITaskAgent>();
        public override List<ITaskAgent> GetSubjectAgents()
        {
            return _subjectAgents;
        }

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

        public override bool CompatibleForParallelExecution(Task task)
        {
            bool b = false;
            /*foreach (var subAgent in GetSubjectAgents())
            {
                b = b || CompatibilityPerSubject(subAgent, task);
            }
            */
            return b;
        }

        private bool CompatibilityPerSubjectAgent(ITaskAgent subAgent, Task task)
        {
            bool b = true;
            if (task is MoveTask2)
            {
                var mvtsk = task as MoveTask2;
                var subut = (subAgent as UnitTeam);

                b = !((task.GetOwnerAgent() as UnitTeam).GetUnit() == subut.GetUnit()
                    || task.GetSubjectAgents().Where((_) => (_ as UnitTeam).GetUnit() == subut.GetUnit()).Count() > 0);
            }
            return b;
        }

        protected override void InstanceSetTaskPlan(TaskPlan2 taskPlan)
        {
            base.InstanceSetTaskPlan(taskPlan);

            if (taskPlan != null)
            {
                /*if (!agentAsTeam.IsVirtualTeam())
                    agentAsTeam.GetUnit().GetFormation().FormTest();*/
                AddSubjectAgent(GetOwnerAgent());

                foreach (var sbt in agentAsTeam.GetAllSubTeamsBFS())
                {
                    AddSubjectAgent(sbt);
                }

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

                SetPhase(TaskPhase.Staging);
            }
        }

        protected override bool InstanceTryStartExecution()
        {
            if (IsInPhase(TaskPhase.Staging) && GetOwnerAgent() != null)
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
                            task.RemoveSubjectAgent(task.GetSubjectAgents().First((_) => (_ as UnitTeam).GetUnit() == (subAgent as UnitTeam).GetUnit())/*subAgent*/);
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
            }

            return false;
        }

        #endregion

        #region Specific behaviour logic

        protected override void EnterExecution()
        {
            //GetParameters().plannedStartingTime = TimeHandler.CurrentTime() + new TimeStruct(0,0,15);
        }
        /*protected override void EnterExecution()
        {
            base.EnterExecution();
            foreach (var chmtm in childrenMoveTaskMarkers)
            {
                //chmtm.GetTaskAsMoveTask().waypointMarkersList = waypointMarkersList;
                //change screen positions of children task markers accordingly
            }
        }*/

        private bool _endedPathForAll;
        protected override void UpdateExecution()
        {
            _endedPathForAll = true;
            if (IsInPhase(TaskPhase.Execution))
            {                
                foreach (var subag in GetSubjectAgents())
                {
                    var ut = (subag as UnitTeam);

                    if (!ut.IsVirtualTeam())
                        ut.GetUnit().GetFormation().FormTest();
                
                    if (/*GetOwnerAgent() != null && */PathExists(ut) && !PathFinished(ut))
                    {
                        Vector3 wpos;
                        wpos = ut.GetUnit().GetFormation().GetAcceptableMovementTargetPosition(GetWaypointMarkersList()[currentWaypointIndex].transform.position);
                
                        NavigateAlongPath(ut, wpos);
                        _endedPathForAll = false;
                    }
                }

                if (GetSubjectAgents().Count == 1 && !(GetOwnerAgent() as UnitTeam).IsLeaf())
                {
                    _endedPathForAll = true;
                }
            }

            if (_endedPathForAll)
            {
                EndExecution();
            }
        }

        private bool PathExists(UnitTeam ut)
        {
            return true;
        }
        
        private bool PathFinished(UnitTeam ut)
        {
            return currentWaypointIndex >= GetWaypointMarkersList().Count;
        }

        private float s = 0.05f;
        private void NavigateAlongPath(UnitTeam ut, Vector3 wpos)
        {   
            //var wpos = GetWaypointMarkersList()[currentWaypointIndex].transform.position;

            var targetPos = wpos;//GetUnitSubject().GetFormation().GetAcceptableMovementTargetPosition(wpos);

            ut.GetUnit().myMover.MoveToPosition(targetPos, s);

            UpdateFormationFacing(ut, targetPos);

            if (ut.GetUnit().myMover.DistanceConditionToPosition(targetPos, 0.02f))
            {
                currentWaypointIndex++;
            }
        }
        
        private void UpdateFormationFacing(UnitTeam ut, Vector3 targetPos)
        {
            if (!ut.IsVirtualTeam())
            {
                ut.GetUnit().GetFormation().facingAngle =
                    Vector3.SignedAngle(
                    Vector3.right,
                    targetPos - agentAsTeam.GetUnit().myMover.transform.position, Vector3.down);
            }
        }

        #endregion

    }
}