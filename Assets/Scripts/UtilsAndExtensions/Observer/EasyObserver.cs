using System;
using System.Collections.Generic;

namespace Nrealus.Extensions.Observer
{

    /****** Author : nrealus ****** Last documentation update : 12-07-2020 ******/

    ///<summary>
    /// This class is a simple implementation of the observer pattern. It can be seen as an encapsulated c# event.
    /// Event handlers subscribe to this observer (="event") with a key, which can then be used to unsubscribe.
    /// Indeed, if an anonymous function subscribes, it can only be unsubscribed if we keep a reference to it and then use it.
    /// This version of the class only allows event handlers with no arguments.
    /// The EasyObserver<TKey,TArgs> version allows event handlers with arguments.
    ///</summary>
    public class EasyObserver<TKey>
    {
        private Dictionary<TKey, Action> dict = new Dictionary<TKey, Action>();

        public void SubscribeToEvent(TKey key, Action action)
        {
            if (!dict.ContainsKey(key))
                dict.Add(key, action);
            else
                throw new ArgumentException("Already known key");
        }

        public void UnsubscribeFromEvent(TKey key)
        {
            if (dict.ContainsKey(key))
                dict.Remove(key);
            else
                throw new ArgumentException("Unknown key");            
        }

        public void UnsubscribeFromAllEvents()
        {
            dict.Clear();
        }

        public void Invoke()
        {
            foreach (var elt in dict)
                elt.Value.Invoke();
        }
    }


    ///<summary>
    /// Another implementation of the observer pattern (see EasyObserver<TKey>) which allows to specify arguments for event handlers.
    ///</summary>
    public class EasyObserver<TKey,TArgs>
    {
        private Dictionary<TKey, Action<TArgs>> dict = new Dictionary<TKey, Action<TArgs>>();

        public void SubscribeToEvent(TKey key, Action<TArgs> action)
        {
            if (!dict.ContainsKey(key))
                dict.Add(key, action);
            else
                throw new ArgumentException("Already known key");
        }

        public void UnsubscribeFromEvent(TKey key)
        {
            if (dict.ContainsKey(key))
                dict.Remove(key);
            else
                throw new ArgumentException("Unknown key");            
        }

        public void UnsubscribeFromAllEvents()
        {
            dict.Clear();
        }

        public void Invoke(TArgs args)
        {
            foreach (var elt in dict)
                elt.Value.Invoke(args);
        }

    }

}