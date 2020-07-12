using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nrealus.Extensions;
using Core.Tasks;
using System;

namespace Core.Handlers
{
    /****** Author : nrealus ****** Last documentation update : 09-07-2020 ******/

    /// <summary>
    /// Singleton registering all Tasks (or rather, TaskWrappers) - they are added to this singleton's list when created and given a receiver, using static "factory" methods in Task.
    /// Of course, they are unregistered from the list when they are cleared.
    /// For now, its only use is to update all the Tasks (again, via TaskWrappers) in the game loop, by updating their main finite state machine.
    /// </summary>    
    public class TaskHandler : MonoBehaviour
    {
        
        private static TaskHandler _instance;
        private static TaskHandler MyInstance
        {
            get
            {
                if(_instance == null)
                    _instance = FindObjectOfType<TaskHandler>(); 
                return _instance;
            }
        }
        
        ///<summary>
        /// This list contains all orders' wrappers that currently exist in the whole scene.
        ///</summary>
        private List<TaskWrapper> taskWrappersList = new List<TaskWrapper>();
        
        private void Awake()
        {
        }

        private void Update()
        {
            // Updating all orders.
            for (int i = taskWrappersList.Count - 1; i >= 0; i--)
            {
                Task.Update(taskWrappersList[i]);
                // Add parameter for "delta time" ?
            }
        }
/*
        private EasyObserver<string,(TaskWrapper,bool)> onTaskWrapperAddOrRemove = new EasyObserver<string, (TaskWrapper, bool)>();

        public static void SubscribeOnAddedTaskWrapper(string key, Action<TaskWrapper> action)
        {
            MyInstance.onTaskWrapperAddOrRemove.SubscribeToEvent(key,(_) => { if (_.Item2) { action(_.Item1); } });
        }

        public static void UnsubscribeOnAddedTaskWrapper(string key)
        {
            MyInstance.onTaskWrapperAddOrRemove.UnsubscribeFromEvent(key);
        }

        public static void SubscribeOnRemovedTaskWrapper(string key,Action<TaskWrapper> action)
        {
            MyInstance.onTaskWrapperAddOrRemove.SubscribeToEvent(key,(_) => { if (!_.Item2) { action(_.Item1); } });
        }

        public static void UnsubscribeOnRemovedTaskWrapper(string key)
        {
            MyInstance.onTaskWrapperAddOrRemove.UnsubscribeFromEvent(key);
        }
*/
        ///<summary>
        /// This method registers a task's wrapper to the list of all tasks' wrappers.
        /// This will allow these tasks to be updated. This method is intended to be called typically right after a task's creation, as it is now. (see Task)
        ///</summary>
        public static bool AddToGlobalTaskWrapperList(TaskWrapper wrapper)
        {
            if(!MyInstance.taskWrappersList.Contains(wrapper))
            {
                wrapper.SubscribeOnClearance("removefromglobal",() => RemoveFromGlobalTaskWrapperList(wrapper));
                MyInstance.taskWrappersList.Add(wrapper);
                //MyInstance.onTaskWrapperAddOrRemove.Invoke((wrapper, true));
                return true;
            }
            else
            {
                Debug.LogError("There should be no reason for this to happen");
                return false;
            }
        }

        private static bool RemoveFromGlobalTaskWrapperList(TaskWrapper wrapper)
        {
            if(MyInstance.taskWrappersList.Contains(wrapper))
            {
                wrapper.UnsubscribeOnClearance("removefromglobal");
                MyInstance.taskWrappersList.Remove(wrapper);
                //MyInstance.onTaskWrapperAddOrRemove.Invoke((wrapper, true));
                return true;
            }
            else
            {
                Debug.LogError("There should be no reason for this to happen");
                return false;
            }
        }


    }
}