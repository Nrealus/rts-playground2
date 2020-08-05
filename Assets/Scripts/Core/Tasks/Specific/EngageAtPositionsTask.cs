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

        private UnitTeam agentAsTeam { get { return GetTaskPlan().GetOwnerAgent() as UnitTeam; } }
        
        private List<UnitTeam> agentSubTeams { get { return agentAsTeam.GetSubTeams(); }}
        //private List<TargetTaskMarker> childrenTargetTaskMarkers = new List<TargetTaskMarker>();

        #endregion

        public EngageAtPositionsTask()
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

        public override bool CompatibleForParallelExecution(Task task)
        {
            bool b = false;
            foreach (var subAgent in GetSubjectAgents())
            {
                b = b || CompatibilityPerSubject(subAgent, task);
            }
            
            return b;
        }

        private bool CompatibilityPerSubject(ITaskAgent subAgent, Task task)
        {
            bool b = true;
            if (task is MoveTask2)
            {
                var mvtsk = task as MoveTask2;
                b = !task.GetSubjectAgents().Contains(subAgent);
            }
            return b;
        }

        protected override void InstanceSetTaskPlan(TaskPlan2 taskPlan)
        {
            base.InstanceSetTaskPlan(taskPlan);

            if (taskPlan != null)
            {
                foreach (var sbt in agentAsTeam.GetAllSubTeamsBFS())
                {
                    AddSubjectAgent(sbt);
                }

                SetPhase(TaskPhase.Staging);
            }
        }

        protected override bool InstanceTryStartExecution()
        {
            if (IsInPhase(TaskPhase.Staging) && GetOwnerAgent() != null)
            {
                Debug.Log("hhhhhh");
                bool b = true;

                foreach (var subAgent in new List<ITaskAgent>(GetSubjectAgents()))
                {
                    foreach (var plan in new List<TaskPlan2>(subAgent.GetOwnedPlans()))
                    {
                        if (plan.GetCurrentTaskInPlan() != this
                            && !CompatibilityPerSubject(subAgent, plan.GetCurrentTaskInPlan()))
                        {
                            //b = false;
                            plan.EndPlanExecution();
                        }                        
                    }
                    foreach (var task in new List<Task>(subAgent.GetTasksWhereIsInternalSubject()))
                    {
                        if (task != this
                            && !CompatibilityPerSubject(subAgent, task))
                        {
                            task.GetTaskPlan().EndPlanExecution();
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

        protected override void UpdateExecution()
        {
            if (IsInPhase(TaskPhase.Execution))
            {
                foreach (var subag in GetSubjectAgents())
                {
                    var ut = (subag as UnitTeam);

                    EngageTargetsInPositionsROE(ut);
                }
            }
        }

        private float s = 5;
        private void EngageTargetsInPositionsROE(UnitTeam ut)
        {
            s = Mathf.Max(s - Time.deltaTime, 0);

            if (s == 0)
            {
                Debug.Log("Hello, engaging order still active");
                s = 5;
            }
        }

        #endregion

    }
}