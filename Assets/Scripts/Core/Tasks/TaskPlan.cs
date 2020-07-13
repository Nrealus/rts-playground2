using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.MapMarkers;
using UnityEngine;
using Nrealus.Extensions;
using Nrealus.Extensions.Observer;

namespace Core.Tasks
{
    
    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    /// <summary>
    /// OUTDATED
    /// Instances of this class store a "plan" of Task.
    /// Two types of them are maintained here : active and passive ones.
    /// Passive orders are supposed not to have a "distinct end", and multiple passive orders could be active executed in this plan at the same time.
    /// Active orders in this plan can only be executed one at a time.
    /// Each Unit actually has an OrderPlan attached to it.
    /// This encapsulation of "plan logic" actually allows for some Orders to use "elementary" orders as "building blocks" in them, like the BuildOrder.
    /// The BuildOrder has a small private class which keeps track of all the IOrderables (i.e. basically units) in its receiver OrderGroup.
    /// And this small class creates a temporary auxiliary Unit for each one of them. Using the OrderPlan of this auxiliary Unit, the BuildOrder
    /// can create and run a MoveOrder "inside of it", to get closer to the building marker. (See BuildOrderNew)
    /// OUTDATED
    /// </summary>
    public class TaskPlan
    {
            
        private EasyObserver<TaskWrapper> onClearance = new EasyObserver<TaskWrapper>();

        public void Clear()
        {
            onClearance.Invoke();
            onClearance.UnsubscribeFromAllEvents();
        }

        /*private void AddOnClearance(Action action)
        {
            onClearance += action;
        }*/

        private void AddOnClearance(TaskWrapper key, Action action)
        {
            onClearance.SubscribeToEvent(key,action);
        }

        private void RemoveOnClearance(TaskWrapper key)
        {
            onClearance.UnsubscribeFromEvent(key);
        }

        /*private Dictionary<TaskWrapper,TaskMarkerWrapper> associatedTaskMarkerDict = new Dictionary<TaskWrapper, TaskMarkerWrapper>();
        public TaskMarkerWrapper GetAssociatedTaskMarker(TaskWrapper tw)
        {
            TaskMarkerWrapper res;
            associatedTaskMarkerDict.TryGetValue(tw, out res);
            return res;
        }*/

        private bool lastAddedActiveOrPassive = true;
        private LinkedList<TaskWrapper> activeTaskPlan = new LinkedList<TaskWrapper>();
        private LinkedList<TaskWrapper> passiveTaskPlan = new LinkedList<TaskWrapper>();

        public void StartActiveExecution()
        {
            Task.TryStartExecution(GetFirstInlineActiveTaskInPlan());
        }

        public void StopActiveExecution()
        {
            Task.EndExecution(activeTaskPlan);
        }

        #region Active Tasks In Plan Functions

        public TaskWrapper GetFirstInlineActiveTaskInPlan()
        {
            var v = activeTaskPlan.First;
            if (v != null)
                return v.Value;
            else
                return null;
        }

        public TaskWrapper GetPreviousInlineActiveTaskInPlan(TaskWrapper orderWrapper)
        {
            var v = activeTaskPlan.Find(orderWrapper);
            if (v != null && v.Previous != null)
                return v.Previous.Value;
            else
                return null;
        }

        public TaskWrapper GetNextInlineActiveTaskInPlan(TaskWrapper orderWrapper)
        {
            var v = activeTaskPlan.Find(orderWrapper);
            if (v != null && v.Next != null)
                return v.Next.Value;
            else
                return null;
        }

        public TaskWrapper GetLastInlineActiveTaskInPlan()
        {
            var v = activeTaskPlan.Last;
            if (v != null)
                return v.Value;
            else
                return null;
        }

        public bool IsFirstInlineActiveTaskInPlan(TaskWrapper wrapper)
        {
            return GetFirstInlineActiveTaskInPlan() == wrapper;
        }

        public IEnumerable<TaskWrapper> GetAllActiveTasksFromPlan()
        {
            return activeTaskPlan;
        }

        public bool IsActiveTaskInPlanBeforeOther(TaskWrapper wrapper, TaskWrapper beforeWhich)
        {
            var node = activeTaskPlan.Find(beforeWhich);

            while (node.Previous != null)
            {
                if (node.Previous.Value == wrapper)
                    return true;
                node = node.Previous;
            }

            return false;
        }

        public bool IsActiveTaskInPlanAfterOther(TaskWrapper wrapper, TaskWrapper afterWhich)
        {
            var node = activeTaskPlan.Find(afterWhich);

            while (node.Next != null)
            {
                if (node.Next.Value == wrapper)
                    return true;
                node = node.Next;
            }

            return false;
        }

        #endregion

        #region Queue And Remove Active Orders To/From Plan

        //private List<MapMarkerWrapper<TaskMarker>> orderMarkersList = new List<MapMarkerWrapper<TaskMarker>>();

        public bool QueueActiveOrderToPlan(TaskWrapper wrapper, TaskWrapper previous, TaskWrapper next)
        {
            if (previous != null && next != null)
                throw new SystemException("Cannot specify both a predecessor and a successor at the same time for an order when queueing it !");

            if (wrapper != null && activeTaskPlan.Find(wrapper) == null)
            {
                if (Task.GetParameters(wrapper).isPassive)
                    throw new SystemException("Only active (non passive) orders allowed");

                wrapper.SubscribeOnClearance("removeactiveorder", () => RemoveActiveOrderFromPlan(wrapper));
                AddOnClearance(wrapper, () => wrapper.DestroyWrappedReference());
                                
                if (previous == null && next == null)
                {
                    lastAddedActiveOrPassive = true;

                    activeTaskPlan.AddLast(wrapper);
                }
                else if (previous != null && activeTaskPlan.Find(previous) != null)
                {
                    if (Task.GetParameters(wrapper).isPassive || Task.GetParameters(previous).isPassive)
                        throw new SystemException("Only active (non passive) orders allowed");
            
                    activeTaskPlan.AddAfter(activeTaskPlan.Find(previous), wrapper);
                }
                else if (next != null && activeTaskPlan.Find(next) != null)
                {
                    if (Task.GetParameters(wrapper).isPassive || Task.GetParameters(next).isPassive)
                        throw new SystemException("Only active (non passive) orders allowed");
            
                    activeTaskPlan.AddBefore(activeTaskPlan.Find(next), wrapper);
                }

                //var om = (TaskMarker.CreateInstance(wrapper)).GetMyWrapper();
                //orderMarkersList.Add(om); // TO BE CHANGED OF COURSE, SO ORDER MARKERS APPEAR FROM BOTTOM TO TOP CORRECT SUCCESSION

                return true;
            }
            else
            {
                return false;    
            }
        }

        private bool RemoveActiveOrderFromPlan(TaskWrapper wrapper)
        {
            if(activeTaskPlan.Find(wrapper) != null)
            {
                activeTaskPlan.Remove(activeTaskPlan.Find(wrapper));

                wrapper.UnsubscribeOnClearance("removeactiveorder");
                RemoveOnClearance(wrapper);
                
                return true;
            }
            else
            {
                return false;    
            }
        }

        #endregion

        /*#region Passive Orders In Plan Functions

        public TaskWrapper GetFirstInlinePassiveOrderInPlan()
        {
            var v = passiveTaskPlan.First;
            if (v != null)
                return v.Value;
            else
                return null;
        }

        public TaskWrapper GetPreviousInlinePassiveOrderInPlan(TaskWrapper orderWrapper)
        {
            var v = passiveTaskPlan.Find(orderWrapper);
            if (v != null && v.Previous != null)
                return v.Previous.Value;
            else
                return null;
        }

        public TaskWrapper GetNextInlinePassiveOrderInPlan(TaskWrapper orderWrapper)
        {
            var v = passiveTaskPlan.Find(orderWrapper);
            if (v != null && v.Next != null)
                return v.Next.Value;
            else
                return null;
        }

        public TaskWrapper GetLastInlinePassiveOrderInPlan()
        {
            var v = passiveTaskPlan.Last;
            if (v != null)
                return v.Value;
            else
                return null;
        }

        public bool IsFirstInlinePassiveOrderInPlan(TaskWrapper wrapper)
        {
            return GetFirstInlinePassiveOrderInPlan() == wrapper;
        }

        public IEnumerable<TaskWrapper> GetAllPassiveOrdersFromPlan()
        {
            return passiveTaskPlan.ToList();
        }

        public TaskWrapper GetLastInlineAnyOrderInPlan()
        {
            LinkedListNode<TaskWrapper> v;
            if (lastAddedActiveOrPassive)
                v = activeTaskPlan.Last;
            else
                v = passiveTaskPlan.Last;
            if (v != null)
                return v.Value;
            else
                return null;
        }

        #endregion

        #region Queue And Remove Passive Orders To/From Plan

        public bool QueuePassiveOrderToPlan(TaskWrapper wrapper, TaskWrapper previous, TaskWrapper next)
        {
            if (previous != null && next != null)
                throw new SystemException("Cannot specify both a predecessor and a successor at the same time for an order when queueing it !");

            if(wrapper != null && passiveTaskPlan.Find(wrapper) == null)
            {
                if (!Task.GetParameters(wrapper).isPassive)
                    throw new SystemException("Only passive orders allowed");

                wrapper.SubscribeOnClearance("removepassiveorder",() => RemovePassiveOrderFromPlan(wrapper));
                AddOnClearance(wrapper,() => wrapper.DestroyWrappedReference());
                                
                if (previous == null && next == null)
                {
                    lastAddedActiveOrPassive = false;

                    passiveTaskPlan.AddLast(wrapper);
                }
                else if (previous != null && passiveTaskPlan.Find(previous) != null)
                {
                    if (!Task.GetParameters(wrapper).isPassive || !Task.GetParameters(previous).isPassive)
                        throw new SystemException("Only passive orders allowed");
            
                    passiveTaskPlan.AddAfter(passiveTaskPlan.Find(previous), wrapper);
                }
                else if (next != null && activeTaskPlan.Find(next) != null)
                {
                    if (!Task.GetParameters(wrapper).isPassive || !Task.GetParameters(next).isPassive)
                        throw new SystemException("Only passive orders allowed");
        
                    passiveTaskPlan.AddBefore(passiveTaskPlan.Find(next), wrapper);
                }

                //var om = (TaskMarker.CreateInstance(wrapper)).GetMyWrapper();
                //orderMarkersList.Add(om); // TO BE CHANGED OF COURSE, SO ORDER MARKERS APPEAR FROM BOTTOM TO TOP CORRECT SUCCESSION

                return true;
            }
            else
            {
                return false;    
            }
        }

        private bool RemovePassiveOrderFromPlan(TaskWrapper wrapper)
        {
            if(passiveTaskPlan.Find(wrapper) != null)
            {
                passiveTaskPlan.Remove(passiveTaskPlan.Find(wrapper));

                wrapper.UnsubscribeOnClearance("removepassiveorder");
                RemoveOnClearance(wrapper);
                
                return true;
            }
            else
            {
                return false;    
            }
        }

        #endregion*/


    }

}