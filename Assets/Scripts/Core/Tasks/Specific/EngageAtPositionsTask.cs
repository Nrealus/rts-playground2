using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nrealus.Extensions;
using NPBehave;
using Core.MapMarkers;
using Core.Handlers;
using Core.Helpers;

namespace Core.Tasks
{
    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    /// <summary>
    /// OUTDATED
    /// This "passive" order (See OrderPlan) can specify targets for units. WIP (as most things but here especially - UnitROE stuff on the horizon..?)
    /// </summary>
    public class EngageAtPositionsTask : Task2
    {

        #region Main declarations

        private UnitGroup actorGroupAsUG { get { return GetTaskPlan().GetActorGroup() as UnitGroup; } }
        
        #endregion

        public EngageAtPositionsTask()
        {
            CreateAndInitFSM();
            SubscribeOnDestructionLate("clearparams", () => GetParameters().RemoveParameterActors(GetParameters().GetParameterActors()));
        }
        
        #region Instance methods and functions

        /*private List<ITaskAgent> _subjectAgents = new List<ITaskAgent>();
        public override List<ITaskAgent> GetSubjectAgents()
        {
            return _subjectAgents;
        }*/
        
        private MapMarkerWrapper<TargetTaskMarker> _targetTaskMarkerWrapper;
        protected override TaskMarker InstanceGetTaskMarker()
        {
            return _targetTaskMarkerWrapper?.Value;
        }

        protected override void InstanceSetTaskMarker(TaskMarker taskMarker)
        {
            _targetTaskMarkerWrapper = new MapMarkerWrapper<TargetTaskMarker>(taskMarker as TargetTaskMarker);
        }

        public List<FirePositionMarker> GetFirePositionMarkersList()
        {
            return _targetTaskMarkerWrapper.Value.firePositionMarkers;
        }

        private TaskParams _taskParams = TaskParams.DefaultParam();
        protected override TaskParams InstanceGetParameters()
        {   
            return _taskParams;
        }

        public int SolveParallelCompatibilityConflicts()
        {
            foreach (var pl in new List<TaskPlan2>(actorGroupAsUG.GetThisGroupPlans()))
            {
                if (pl.GetCurrentTaskInPlan() != this)
                {
                    UnitGroup ug = pl.GetCurrentTaskInPlan().GetActorGroup() as UnitGroup;

                }
            }

            return 0;
        }

        public bool CompatibleForParallelExecution(Task task)
        {
            bool b = false;
            /*foreach (var subAgent in GetSubjectAgents())
            {
                b = b || CompatibilityPerSubject(subAgent, task);
            }*/
            
            return b;
        }

        private bool CompatibilityPerActor(IActor actor, Task task)
        {
            bool b = true;
            if (task is MoveTask2)
            {
                var mvtsk = task as MoveTask2;
                //b = !task.GetSubjectAgents().Contains(subAgent);
            }
            return b;
        }

        protected override void InstanceSetTaskPlan(TaskPlan2 taskPlan)
        {
            base.InstanceSetTaskPlan(taskPlan);

            if (taskPlan != null)
            {
                /*foreach (var sbt in agentAsTeam.GetAllSubTeamsBFS())
                {
                    AddSubjectAgent(sbt);
                }*/

                SetPhase(TaskPhase.Staging);
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

                    /*foreach (var chmtm in subActorsMoveTasks)
                    {
                        chmtm.Value.TryStartExecution();
                    }*/
                    return true;
                }
            }

            return false;
        }

        #endregion
        

        #region Specific behaviour logic

        protected override void UpdateExecution()
        {
            foreach (var u in actorGroupAsUG.GetActorsAsUnits())
            {
                //var ut = (subag as UnitTeam);

                EngageTargetsInPositionsROE(u);
            }
        }

        private float s = 2;
        private void EngageTargetsInPositionsROE(Unit u)
        {
            s = Mathf.Max(s - Time.deltaTime, 0);

            if (s == 0)
            {
                Debug.Log("Hello, engaging order still active");
                s = 2;
            }
        }

        #endregion

    }
}