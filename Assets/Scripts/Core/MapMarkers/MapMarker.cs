using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Helpers;
using Nrealus.Extensions;
using Nrealus.Extensions.Observer;
using Core.Selection;
using System;
using Nrealus.Extensions.ReferenceWrapper;

namespace Core.MapMarkers
{

    public class MapMarkerWrapper : MapMarkerWrapper<MapMarker>
    {
        public MapMarkerWrapper(MapMarker obj) : base(obj)
        { 
            obj.SubscribeOnDestructionAtEnd("destroywrapper", DestroyRef, true);
        }
    }

    public class MapMarkerWrapper<T> : RefWrapper<T> where T : MapMarker
    {
        public MapMarkerWrapper(T obj) : base(obj)
        { 
            obj.SubscribeOnDestructionAtEnd("destroywrapper", DestroyRef, true);
        }
    }
    
    
    /****** Author : nrealus ****** Last documentation update : 25-07-2020 ******/

    /// <summary>
    /// A class whose subclasses are intended to be "concrete" (MonoBehaviour) "markers" on the map.
    /// These markers may fill different roles : simple indicators of an in-game situation, or "parameter" indicators for Tasks, like an area to target,
    /// and/or elements of interactive UI (as opposed to simple information display) which can be selected, and moved around for example.
    /// (-> see TaskMarker and an implementation : MoveTaskMarker)
    /// </summary>
    public abstract class MapMarker : MonoBehaviour, ISelectable // IHasRefWrapper<MapMarkerWrapper>
    {

        #region ISelectable implementation

        private EasyObserver<string, (Selector,bool)> onSelectionStateChange = new EasyObserver<string, (Selector, bool)>();

        public EasyObserver<string, (Selector,bool)> GetOnSelectionStateChangeObserver()
        {
            return onSelectionStateChange;
        }
        
        void ISelectable.InvokeOnSelectionStateChange(Selector selector, bool b)
        {
            onSelectionStateChange.Invoke((selector, b));
        }

        #endregion
        
        #region IDestroyable implementation
        
        private EasyObserver<string> onDestroyed = new EasyObserver<string>();

        public void SubscribeOnDestruction(string key, Action action)
        {
            onDestroyed.SubscribeEventHandlerMethod(key, action);
        }

        public void SubscribeOnDestructionAtEnd(string key, Action action)
        {
            onDestroyed.SubscribeEventHandlerMethodAtEnd(key, action);
        }

        public void SubscribeOnDestruction(string key, Action action, bool combineActionsIfKeyAlreadyExists)
        {
            onDestroyed.SubscribeEventHandlerMethod(key, action, combineActionsIfKeyAlreadyExists);
        }

        public void SubscribeOnDestructionAtEnd(string key, Action action, bool combineActionsIfKeyAlreadyExists)
        {
            onDestroyed.SubscribeEventHandlerMethodAtEnd(key, action, combineActionsIfKeyAlreadyExists);
        }

        public void UnsubscribeOnDestruction(string key)
        {
            onDestroyed.UnsubscribeEventHandlerMethod(key);
        }

        public virtual void DestroyThis()
        {
            Destroy(gameObject);

            onDestroyed.Invoke();

            GetOnSelectionStateChangeObserver().UnsubscribeAllEventHandlerMethods();
        }

        #endregion

    }
}