using Core.Handlers;
using Core.Helpers;
using Core.MapMarkers;
using Core.Units;
using Gamelogic.Extensions;
using GlobalManagers;
using Nrealus.Extensions.Observer;
using Nrealus.Extensions.ReferenceWrapper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Tasks
{

    /****** Author : nrealus ****** Last documentation update : 25-07-2020 ******/

    public class TaskWrapper : TaskWrapper<Task>
    {
        public TaskWrapper(Task obj) : base(obj)
        { 
            obj.SubscribeOnDestructionLate("destroywrapper", DestroyRef, true);
        }
    }

    public class TaskWrapper<T> : RefWrapper<T> where T : Task
    {
        public TaskWrapper(T obj) : base(obj)
        { 
            obj.SubscribeOnDestructionLate("destroywrapper", DestroyRef, true);
        }
    }
    
    /// <summary>
    /// The base class for all tasks. They are part of a TaskPlan, and their agent - at least for now - is always the agent of the plan.
    /// It provides the structure for tasks, notably in the form of protected abstract or virtual methods to be implemented by "concrete" subclasses.
    /// A Task subclass should be instaciated from the "factory" generic function "CreateTask".
    /// With time, some things that may become very common subclasses may be bundled into an "intermediate" abstract subclass (like Task2), or even into this one.
    /// </summary>      
    public abstract class Task : IDestroyable
    {
        
        #region IDestroyable implementation
        
        private EasyObserver<string> onDestroyed = new EasyObserver<string>();

        public void SubscribeOnDestruction(string key, Action action)
        {
            onDestroyed.SubscribeEventHandlerMethod(key, action);
        }

        public void SubscribeOnDestructionLate(string key, Action action)
        {
            onDestroyed.SubscribeEventHandlerMethodAtEnd(key, action);
        }

        public void SubscribeOnDestruction(string key, Action action, bool combineActionsIfKeyAlreadyExists)
        {
            onDestroyed.SubscribeEventHandlerMethod(key, action, combineActionsIfKeyAlreadyExists);
        }

        public void SubscribeOnDestructionLate(string key, Action action, bool combineActionsIfKeyAlreadyExists)
        {
            onDestroyed.SubscribeEventHandlerMethodAtEnd(key, action, combineActionsIfKeyAlreadyExists);
        }

        public void UnsubscribeOnDestruction(string key)
        {
            onDestroyed.UnsubscribeEventHandlerMethod(key);
        }

        public void DestroyThis()
        {
            /*foreach (var v in new List<ITaskAgent>(GetSubjectAgents()))
            {
                RemoveSubjectAgent(v);
            }*/

            onDestroyed.Invoke();
        }

        #endregion

        #region Static "factory" functions

        private static T Internal_CreateTask<T>() where T : Task
        {
            switch (typeof(T))
            {
                case Type taskType when taskType == typeof(MoveTask):
                {
                    MoveTask t = new MoveTask();
                    return t as T;
                }
                case Type taskType when taskType == typeof(MoveTask2):
                {
                    MoveTask2 t = new MoveTask2();
                    return t as T;
                }
                case Type taskType when taskType == typeof(EngageAtPositionsTask):
                {
                    EngageAtPositionsTask t = new EngageAtPositionsTask();
                    return t as T;
                }
                default:
                    throw new ArgumentException(
                    message: "Not a recognized type of task");
                    //return null;
            }

        } 

        public static T CreateTask<T>() where T : Task
        {
            T res = Internal_CreateTask<T>();
            
            TaskHandler.RegisterToGlobalTasksList(res);
            
            return res;
        }

        #endregion

        #region FSM control methods

        public bool TryStartExecution()
        {
            return InstanceTryStartExecution();
        }

        public bool PauseExecution()
        {
            if (IsInPhase(Task.TaskPhase.Execution))
            {
                SetPhase(Task.TaskPhase.Paused);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UnpauseExecution()
        {
            if (IsInPhase(Task.TaskPhase.Paused))
            {
                SetPhase(Task.TaskPhase.Execution);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool EndExecution()
        {
            SetPhase(Task.TaskPhase.End);
            return true;
        }

        public static bool EndExecution(IEnumerable<Task> tasks)
        {
            foreach (var tw in tasks)
                tw.EndExecution();
            return true;
        }

        public void SetPhase(TaskPhase phase)
        {
            InstanceSetPhase(phase);
        }

        public bool IsInPhase(TaskPhase phase)
        {
            return InstanceIsInPhase(phase);
        }

        #endregion

        public void SetTaskMarker(TaskMarker taskMarker)
        {
            InstanceSetTaskMarker(taskMarker);
        }

        public void SetTaskPlan(TaskPlan2 taskPlan)
        {
            InstanceSetTaskPlan(taskPlan);
        }
        
        public TaskMarker GetTaskMarker()
        {
            return InstanceGetTaskMarker();
        }

        public TaskPlan2 GetTaskPlan()
        {
            return InstanceGetTaskPlan();
        }

        public IActorGroup GetActorGroup()
        {
            return GetTaskPlan().GetActorGroup();
        }

        public T GetActorGroup<T>() where T : IActorGroup
        {
            return (T) GetTaskPlan().GetActorGroup();
        }

        public TaskParams GetParameters()
        {
            return InstanceGetParameters();
        }
        
        public enum TaskPhase
        {   Initial, Staging,
            WaitToStartExecution, Execution,
            Paused, Cancelled, End,
            End2, Disposed,  
        }
        protected StateMachine<TaskPhase> orderPhasesFSM;

        public Task()
        {
            //BaseConstructor(); <-- NO : BECAUSE C# CALLS CONSTRUCTORS "FROM TOP TO BOTTOM" (base then derived)
        }

        protected void CreateAndInitFSM()
        {
            orderPhasesFSM = new StateMachine<TaskPhase>();
            InitPhasesFSM();
        }

        public void Update()
        {
            InstanceUpdate();
        }

        //public abstract bool CompatibleForParallelExecution(Task task);

        #region Abstract instance methods

        protected abstract void InstanceSetTaskMarker(TaskMarker taskMarker);

        protected abstract void InstanceSetTaskPlan(TaskPlan2 taskPlan);

        protected abstract TaskMarker InstanceGetTaskMarker();

        protected abstract TaskPlan2 InstanceGetTaskPlan();

        protected abstract TaskParams InstanceGetParameters();

        protected abstract void InitPhasesFSM();

        protected abstract bool InstanceTryStartExecution();
        
        #endregion

        #region Protected/Private instance methods

        protected void InstanceSetPhase(TaskPhase phase)
        {
            orderPhasesFSM.CurrentState = phase;
        }

        protected bool InstanceIsInPhase(TaskPhase phase)
        {
            return orderPhasesFSM.CurrentState == phase;
        }

        private void InstanceUpdate()
        {
            orderPhasesFSM.Update();
        }

        #endregion
        
    }
}