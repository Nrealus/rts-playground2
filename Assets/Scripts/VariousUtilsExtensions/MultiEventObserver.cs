using System;
using System.Collections.Generic;

namespace VariousUtilsExtensions
{

    /*
    public class Source1Example
    {

        private event Action onSomethingHappened;

        private void SubscribeToMultieventObserver(MultiEventObserver obs, Action<object, EventArgs> callback)
        {
            var lstn = obs.AddEventAndSubscribeToIt(callback);
            onSomethingHappened += () => obs.InvokeEvent(lstn, this, null);
        }

    }
    */

    public class SimpleEventArgs : EventArgs
    {
        public object[] args;

        public SimpleEventArgs(params object[] args)
        {
            this.args = args;
        }
    }

    public class MultiEventObserver// : IDisposable
    {

        private List<object> senders = new List<object>();

        private int maxId; // Could be replaced by something generic without to much difficulty.
        private Dictionary<int, EventWrapper> events = new Dictionary<int, EventWrapper>();

        private class EventWrapper
        {

            public event Action<object, EventArgs> onFired;

            public void Invoke(object sender, EventArgs args)
            {
                onFired?.Invoke(sender, args);
            }

            public void Clear()
            {
                onFired = null;
            }

        }

        private bool invoking;
        
        public MultiEventObserver()
        {

        }

        /*public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            foreach(var v in events)
            {
                v.Value.Clear();
            }
            events.Clear();
        }

        ~MultiEventObserver()
        {
            Dispose(false);
        }*/
        
        public void InvokeEvent(int eventId, object sender, EventArgs args)
        {
            if (!invoking)
            {
                if (!senders.Contains(sender))
                {
                    senders.Add(sender);
                    invoking = true;
                    events[eventId].Invoke(sender, args);
                    foreach(var v in events)
                    {
                        if (v.Key != eventId)
                            v.Value.Invoke(sender, args);
                    }
                    invoking = false;                   
                    senders.Clear();
                }
            }
        }
    
        public int AddEventAndSubscribeToIt(Action<object, EventArgs> callback)
        {
            var r = new EventWrapper();

            events.Add(maxId, r);
            maxId++;

            r.onFired += callback;

            return maxId - 1;
        }
    
    }

}