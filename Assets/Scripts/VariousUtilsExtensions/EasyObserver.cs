using System;
using System.Collections.Generic;

namespace VariousUtilsExtensions
{

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