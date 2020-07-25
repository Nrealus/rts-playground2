using System;
using System.Collections.Generic;

namespace Nrealus.Extensions.Observer
{

    /****** Author : nrealus ****** Last documentation update : 12-07-2020 ******/

    ///<summary>
    /// This class implements a "custom variant" of the Observer pattern.
    /// In this version, there are several "events" that are all linked together. When one of them is triggered, all of the others follow.
    /// Each one of them can be triggered by different "initial triggers" (or "senders").
    /// However, if one of the events is triggered and, in their turn, a following event triggers it again during its invocation, cyclic calls appear.
    /// To prevent this, we only allow an event to be actually invoked if it is triggered by a different sender.
    /// A typical situation where this class can be used is data binding.
    /// In fact, the initial idea behind this class was to implement data binding with a ui element.
    ///</summary>
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
                    /* Previous version : removed to support deletion / unsubscription of methods,
                    as it's not possible to modify a collection during a foreach loop. (http://msdn.microsoft.com/en-us/library/ttw7t8t6.aspx)
                    foreach(var v in events)
                    {
                        if (v.Key != eventId)
                            v.Value.Invoke(sender, args);
                    }
                    */
                    foreach(var vk in new List<int>(events.Keys))
                    {
                        if (vk != eventId)
                            events[vk].Invoke(sender, args);
                    }
                    invoking = false;                   
                    senders.Clear();
                }
            }
        }
    
        public int AddNewEventAndSubscribeMethodToIt(Action<object, EventArgs> callback)
        {
            var r = new EventWrapper();

            events.Add(maxId, r);
            maxId++;

            r.onFired += callback;

            return maxId - 1;
        }
    
    }

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


}