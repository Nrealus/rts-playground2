using Core.Handlers;
using Core.Helpers;
using Core.MapMarkers;
using Core.Units;
using Gamelogic.Extensions;
using GlobalManagers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.BuiltStructures
{

    /// <summary>
    /// ---- General Description, by nrealus, last update : 23-04-2020 ----
    ///
    /// Main class for buildings and constructibles.
    /// Extend this class to implement building specific functionality.
    ///
    /// Instances of BuiltStructure are wrapped in BuiltStructureWrapper.
    /// And the encouraged use of interacting with a BuiltStructure instance is to use static functions with a reference to its wrapper as a parameter.
    ///
    /// For now, this class isn't even abstract, because of testing purposes.
    /// Most of the code is inspired by the Order class, that's why there's a lot of its code that is commented below.
    ///    
    /// In other words, this is still being figured out.
    /// </summary>
    
    public /*abstract*/ class BuiltStructure :
        IHasRefWrapper<BuiltStructureWrapper>
    {
        
        #region Static functions

        /*protected static bool RegisterIfAppropriate(BuiltStructureWrapper orderWrapper)
        {
            if (!OrderHandler.IsOrderWrapperRegistered(orderWrapper))
            {
                Order.GetReceiver(orderWrapper).AddOrderToList(orderWrapper, null);
                OrderHandler.AddToOrderWrapperList(orderWrapper);
                orderWrapper.SubscribeOnClearance(() => Order.Unregister(orderWrapper));
                return true;
            }
            else
            {
                Debug.LogError("order was already registered");
                return false;
            }

        }

        private static bool Unregister(OrderWrapper orderWrapper)
        {
            if (OrderHandler.IsOrderWrapperRegistered(orderWrapper))
            {
                GetReceiver(orderWrapper).RemoveOrderFromList(orderWrapper);
                OrderHandler.RemoveFromOrderWrapperList(orderWrapper);
                //.RemoveFromOrderWrapperList(this);
                orderWrapper.UnsubscribeOnClearance(() => Order.Unregister(orderWrapper)); 
                return true;
            }
            else
            {
                Debug.LogError("order is not registered");
                return false;
            }
        }

        private static bool GetConfirmationFromReceiver(OrderWrapper orderWrapper)
        {
            if (orderWrapper.WrappedObject != null
                && (Order.IsInPhase(orderWrapper, Order.OrderPhase.Registration)
                || Order.IsInPhase(orderWrapper, Order.OrderPhase.NotReadyForExecution)))
            {
                Order.SetPhase(orderWrapper, Order.OrderPhase.RequestConfirmation);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool TryStartExecution(OrderWrapper orderWrapper)
        {
            Order.GetConfirmationFromReceiver(orderWrapper);
            if(Order.IsInPhase(orderWrapper, Order.OrderPhase.ReadyForExecution))
            {
                Order.SetPhase(orderWrapper, Order.OrderPhase.ExecutionWaitingToStart);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool PauseExecution(OrderWrapper orderWrapper)
        {
            if (Order.IsInPhase(orderWrapper, Order.OrderPhase.Execution))
            {
                Order.SetPhase(orderWrapper, Order.OrderPhase.Pause);
                return true;
            }
            else
            {
                return false;private List<MapMarkerWrapper<WaypointMarker>> waypointMarkerWrappersList = new List<MapMarkerWrapper<WaypointMarker>>();
        
            }
        }

        public static bool UnpauseExecution(OrderWrapper orderWrapper)
        {
            if (Order.IsInPhase(orderWrapper, Order.OrderPhase.Pause))
            {
                Order.SetPhase(orderWrapper, Order.OrderPhase.Execution);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool EndExecution(OrderWrapper orderWrapper)
        {
            Order.SetPhase(orderWrapper, Order.OrderPhase.End);
            return true;
        }

        public static bool IsApplicable(OrderWrapper orderWrapper)
        {
            return orderWrapper.WrappedObject.InstanceIsOrderApplicable();
        }

        public static IOrderable GetReceiver(OrderWrapper orderWrapper)
        {
            return orderWrapper.WrappedObject.InstanceGetOrderReceiver();
        }

        public static void SetReceiver(OrderWrapper orderWrapper, IOrderable orderable)
        {
            // unsubscribe this if SetOrderReceiver for another orderable afterwards, potentially ?
            orderable.GetOrderableAsReferenceWrapperNonGeneric().SubscribeOnClearance(() => orderWrapper.DestroyWrappedReference());
            orderWrapper.WrappedObject.InstanceSetOrderReceiver(orderable);
        }

        public static OrderParams GetParameters(OrderWrapper orderWrapper)
        {
            return orderWrapper.WrappedObject.InstanceGetOrderParams();
        }

        public static void SetParameters(OrderWrapper orderWrapper, OrderParams orderParams)
        {
            orderWrapper.WrappedObject.InstanceSetOrderParams(orderParams);
        }

        public static bool ReceiverExists(OrderWrapper orderWrapper)
        {
            return GetReceiver(orderWrapper) != null && GetReceiver(orderWrapper).AmIStillUsed();
        }

        public static void SetPhase(OrderWrapper orderWrapper, OrderPhase phase)
        {
            orderWrapper.WrappedObject.InstanceSetPhase(phase);
        }

        public static bool IsInPhase(OrderWrapper orderWrapper, OrderPhase phase)
        {
            return orderWrapper.WrappedObject.InstanceIsInPhase(phase);
        }

        public static void UpdateFSM(OrderWrapper orderWrapper)
        {
            if (orderWrapper != null && orderWrapper.WrappedObject != null)
                orderWrapper.WrappedObject.InstanceUpdateFSM();
        }

        #endregion

        #region Variables, properties etc.

        public enum OrderPhase
        {   InitialState,
            Registration, RequestConfirmation, ReadyForExecution, NotReadyForExecution,
            ExecutionWaitingToStart, Execution, Pause, Cancelled, End, Disposed }
        protected StateMachine<OrderPhase> orderPhasesFSM;

        */

        public static float GetHP(BuiltStructureWrapper builtStructureWrapper)
        {
            return builtStructureWrapper.WrappedObject.InstanceGetHP();
        }

        public static void AddHP(BuiltStructureWrapper builtStructureWrapper, float amount)
        {
            builtStructureWrapper.WrappedObject.InstanceSetHP(builtStructureWrapper.WrappedObject.InstanceGetHP() + amount);
        }

        /*--------*/

        protected BuiltStructureWrapper _myWrapper;
        public BuiltStructureWrapper GetMyWrapper() { return _myWrapper; }
        public BuiltStructureWrapper<T> GetMyWrapper<T>() where T : BuiltStructure { return _myWrapper as BuiltStructureWrapper<T>; }

        /*--------*/

        #endregion

        public BuiltStructure()
        {
            //BaseConstructor(); <-- NO : BECAUSE C# CALLS CONSTRUCTORS "FROM TOP TO BOTTOM" (base then derived)
            // TEMPORARY FOR TESTING (as long as this is not an abstract class)
            _myWrapper = new BuiltStructureWrapper<BuiltStructure>(this, () => {_myWrapper = null;});
        }
        
        #region Protected/Private abstract instance methods

        private float _hp;
        protected virtual float InstanceGetHP()
        {
            return _hp;
        }
        protected virtual void InstanceSetHP(float hp)
        {
            _hp = hp;
        }
        
        //protected abstract float InstanceCompletionState();

        /*protected abstract OrderParams InstanceGetOrderParams();

        protected abstract void InstanceSetOrderParams(OrderParams orderParams);

        protected abstract IOrderable InstanceGetOrderReceiver();
        
        //public abstract T GetOrderReceiverAsT<T>() where T : IOrderable;

        protected abstract void InstanceSetOrderReceiver(IOrderable orderable);

        protected abstract void OrderPhasesFSMInit();

        protected abstract bool InstanceIsOrderApplicable();

        #endregion

        #region Protected/Private instance methods

        protected void InstanceSetPhase(OrderPhase phase)
        {
            orderPhasesFSM.CurrentState = phase;
        }
mo
        protected bool InstanceIsInPhase(OrderPhase phase)
        {
            return orderPhasesFSM.CurrentState == phase;
        }

        private void InstanceUpdateFSM()
        {
            orderPhasesFSM.Update();
        }

        protected void CreateAndInitFSM()
        {
            orderPhasesFSM = new StateMachine<OrderPhase>();
            OrderPhasesFSMInit();
        }*/

        //public abstract void SetOptions(OrderOptions options);

        #endregion*/
        
    }
}