using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nrealus.Extensions;
using Core.Tasks;
using System;

namespace Core.Handlers
{
    /****** Author : nrealus ****** Last documentation update : 25-07-2020 ******/

    /// <summary>
    /// Singleton registering all Tasks - they are added to this singleton's list when created, using static "factory" methods in Task.
    /// Of course, they are unregistered from the list when they are destroyed.
    /// For now, its only use is to update all the Tasks in the game loop, by updating their main finite state machine.
    /// This is needed because Tasks are not MonoBehaviours and their updates must be called from another MonoBehaviour.
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
        /// This list contains all tasks that currently exist.
        ///</summary>
        private List<Task> tasks = new List<Task>();
        
        private void Awake()
        {
        }

        private void Update()
        {
            // Updating all tasks.
            for (int i = tasks.Count - 1; i >= 0; i--)
            {
                tasks[i].Update();
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
        /// This method registers a task to the "global" tasksList.
        /// This will allow these tasks to be updated. This method is intended to be called typically right after a task's creation, as it is now. (see Task)
        ///</summary>
        public static bool AddToGlobalTasksList(Task task)
        {
            if(!MyInstance.tasks.Contains(task))
            {
                task.SubscribeOnDestruction("removefromglobal",() => RemoveFromGlobalTasksList(task));
                MyInstance.tasks.Add(task);
                //MyInstance.onTaskWrapperAddOrRemove.Invoke((wrapper, true));
                return true;
            }
            else
            {
                Debug.LogError("There should be no reason for this to happen");
                return false;
            }
        }

        private static bool RemoveFromGlobalTasksList(Task task)
        {
            if(MyInstance.tasks.Contains(task))
            {
                task.UnsubscribeOnDestruction("removefromglobal");
                MyInstance.tasks.Remove(task);
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