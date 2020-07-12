using Core.Handlers;
using Core.Helpers;
using Core.Units;
using Gamelogic.Extensions;
using GlobalManagers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Tasks
{

    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/
    
    /// <summary>
    /// The base class for all orders. It works with IOrderable "receivers" of orders, to which the order is given.
    /// It provides the structure for orders, mostly in the form of protected abstract or virtual methods to be implemented by "concrete" subclasses.
    /// It also provides static methods and functions which call the appropriate instance methods or functions, given a IOrderable as a parameter.
    /// This allows to limit accessing .WrappedObject for common things, and has shown to be a nice approach improving clarity and encapsulation aswell as decoupling.   
    /// An Order subclass should be instanciated from the static class OrderFactory.
    /// With time, some things that may become very common subclasses may be bundled into an "intermediate" abstract subclass, or even into this one.
    /// </summary>      
    public abstract class Task :
        IHasRefWrapper<TaskWrapper>
    {
        
        #region Static "factory" functions

        private static TaskWrapper<T> CreateTaskWrapper<T>() where T : Task
        {
            switch (typeof(T))
            {
                case Type taskType when taskType == typeof(MoveTask):
                {
                    MoveTask t = new MoveTask();
                    TaskWrapper<MoveTask> wrapper = t.GetRefWrapper();
                    return wrapper as TaskWrapper<T>;
                }
                case Type taskType when taskType == typeof(BuildTask):
                {
                    BuildTask t = new BuildTask();
                    TaskWrapper<BuildTask> wrapper = t.GetRefWrapper();
                    return wrapper as TaskWrapper<T>;
                }
                case Type taskType when taskType == typeof(EngageAtPositionsTask):
                {
                    EngageAtPositionsTask t = new EngageAtPositionsTask();
                    TaskWrapper<EngageAtPositionsTask> wrapper = t.GetRefWrapper();
                    return wrapper as TaskWrapper<T>;
                }
                /*case Type MoveOrderType 
                when MoveOrderType == typeof(MoveOrder) :
                    bool j = true;
                    break;*/
                default:
                    throw new ArgumentException(
                    message: "not a recognized type of order");
                    //return null;
            }

        } 

        public static TaskWrapper<T> CreateTaskWrapperWithoutSubject<T>() where T : Task
        {
            TaskWrapper<T> res = CreateTaskWrapper<T>();
            
            TaskHandler.AddToGlobalTaskWrapperList(res);
            
            return res;
        }

        public static TaskWrapper<T> CreateTaskWrapperAndSetReceiver<T>(ITaskSubject subject) where T : Task
        {
            TaskWrapper<T> res = CreateTaskWrapper<T>();

            TaskHandler.AddToGlobalTaskWrapperList(res);

            Task.SetSubject(res, null, null, subject);
            return res;
        }

        public static TaskWrapper<T> CreatePredecessorTaskWrapperAndSetReceiver<T>(ITaskSubject subject, TaskWrapper successor) where T : Task
        {
            TaskWrapper<T> res = CreateTaskWrapper<T>();
            
            TaskHandler.AddToGlobalTaskWrapperList(res);

            Task.SetSubject(res, null, successor, subject);
            return res;
        }

        #endregion

        #region Static functions

        public static bool TryStartExecution(TaskWrapper taskWrapper)
        {
            return taskWrapper.GetWrappedReference().InstanceTryStartExecution();
        }

        public static bool PauseExecution(TaskWrapper taskWrapper)
        {
            if (IsInPhase(taskWrapper, Task.OrderPhase.Execution))
            {
                SetPhase(taskWrapper, Task.OrderPhase.Pause);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool UnpauseExecution(TaskWrapper taskWrapper)
        {
            if (IsInPhase(taskWrapper, Task.OrderPhase.Pause))
            {
                SetPhase(taskWrapper, Task.OrderPhase.Execution);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool EndExecution(TaskWrapper taskWrapper)
        {
            SetPhase(taskWrapper, Task.OrderPhase.End);
            return true;
        }

        public static bool EndExecution(IEnumerable<TaskWrapper> taskWrapper)
        {
            foreach (var tw in taskWrapper)
                EndExecution(tw);
            return true;
        }

        public static ITaskSubject GetSubject(TaskWrapper taskWrapper)
        {
            return taskWrapper.GetWrappedReference().InstanceGetSubject();
        }

        public static Vector3 GetTaskLocation(TaskWrapper taskWrapper)
        {
            if (GetSubject(taskWrapper) is UnitWrapper)
                return Unit.GetPosition((UnitWrapper)GetSubject(taskWrapper));
            else
                return Vector3.zero;
        }

        public static object GetTaskGiver(TaskWrapper taskWrapper)
        {
            throw new System.NotImplementedException();
        }

        public static void SetSubject(TaskWrapper taskWrapper, TaskWrapper predecessor, TaskWrapper successor, ITaskSubject subject)
        {
            // unsubscribe this if SetOrderReceiver for another orderable afterwards, potentially ?
            taskWrapper.GetWrappedReference().InstanceSetSubject(subject, predecessor, successor);
        }

        public static void SetTaskGiver(TaskWrapper taskWrapper)
        {
            throw new System.NotImplementedException();
        }

        public static TaskParams GetParameters(TaskWrapper taskWrapper)
        {
            return taskWrapper.GetWrappedReference().InstanceGetParameters();
        }

        /*public static void SetParameters(taskWrapper taskWrapper, OrderParams orderParams)
        {
            taskWrapper.WrappedObject.InstanceSetOrderParams(orderParams);
        }*/

        public static bool SubjectExists(TaskWrapper taskWrapper)
        {
            return GetSubject(taskWrapper) != null && GetSubject(taskWrapper).IsWrappedObjectNotNull();
        }

        public static void SetPhase(TaskWrapper taskWrapper, OrderPhase phase)
        {
            taskWrapper.GetWrappedReference().InstanceSetPhase(phase);
        }

        public static bool IsInPhase(TaskWrapper taskWrapper, OrderPhase phase)
        {
            return taskWrapper.GetWrappedReference().InstanceIsInPhase(phase);
        }
        
        public static void Update(TaskWrapper taskWrapper)
        {
            if (taskWrapper != null && taskWrapper.GetWrappedReference() != null)
                taskWrapper.GetWrappedReference().InstanceUpdate();
        }

        #endregion

        #region Variables, properties etc.

        public enum OrderPhase
        {   Initial,
            /*Registration,*/ Staging,// ReadyForExecution, NotReadyForExecution,
            ExecutionWaitingTimeToStart, Execution, Pause, Cancelled, End, End2, Disposed,  
        }
        protected StateMachine<OrderPhase> orderPhasesFSM;

        /*--------*/

        protected TaskWrapper _myWrapper;

        public TaskWrapper GetRefWrapper() { return _myWrapper; }
        //public TaskWrapper<T> GetRefWrapper<T>() where T : Task { return ReturnUniqueRefWrapperField<T>(); }

        /*--------*/

        #endregion

        public Task()
        {
            //BaseConstructor(); <-- NO : BECAUSE C# CALLS CONSTRUCTORS "FROM TOP TO BOTTOM" (base then derived)
        }

        #region Protected/Private abstract instance methods

        protected abstract TaskParams InstanceGetParameters();

        //protected abstract void InstanceSetOrderParams(OrderParams orderParams);

        protected abstract ITaskSubject InstanceGetSubject();
        
        //public abstract T GetOrderReceiverAsT<T>() where T : IOrderable;

        protected abstract void InstanceSetSubject(ITaskSubject orderable, TaskWrapper predecessor, TaskWrapper successor);

        protected abstract void InitPhasesFSM();

        //protected abstract bool InstanceIsReadyToStartExecution(OrderExecutionMode mode);

        protected abstract bool InstanceTryStartExecution();
        
        #endregion

        #region Protected/Private instance methods

        protected void InstanceSetPhase(OrderPhase phase)
        {
            orderPhasesFSM.CurrentState = phase;
        }

        protected bool InstanceIsInPhase(OrderPhase phase)
        {
            return orderPhasesFSM.CurrentState == phase;
        }

        private void InstanceUpdate()
        {
            orderPhasesFSM.Update();
        }

        protected void CreateAndInitFSM()
        {
            orderPhasesFSM = new StateMachine<OrderPhase>();
            InitPhasesFSM();
        }

        //public abstract void SetOptions(OrderOptions options);

        #endregion
        
    }
}