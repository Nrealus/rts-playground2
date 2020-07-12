using System;
using System.Collections.Generic;

namespace Nrealus.Extensions.ReferenceWrapper
{

    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    ///<summary>
    /// Base class for "Reference Wrappers". See RefWrapper<T> for much more details.
    ///</summary>
    public abstract class RefWrapper// : IRefWrapperInterf1
    {        
        public abstract void SubscribeOnClearance(Action action);

        public abstract void SubscribeOnClearance(string key, Action action);

        public abstract void UnsubscribeOnClearance(string key);

        public abstract void UnsubscribeOnClearanceAll();

        public abstract void DestroyWrappedReference();

    }

    /// <summary>
    /// A wrapper class whose initial purpose is to remove any direct references to the wrapped object.
    /// In fact, a multitude of direct references to an object makes it hard to keep track of all of them, and can result in unwanted scenarios,
    /// such as an object outliving its supposed lifespan instead of being collected after it's no longer used (or "destroyed"), because of some remaining non-null references to the object.
    /// To prevent that, we use references to a wrapper that contains one single reference to this object.
    /// Once the object is not needed anymore, we set the reference to it in the wrapper to null, and "clear" the rest of the wrapper.
    /// "Used" wrappers do not contain any useful information anymore, and will then eventually be collected, or may be used again with a pooling system.
    /// Finally this wrapper publishes(?) an event whose subscribed methods are invoked when the wrapper is cleaned, i.e. when the wrapped object isn't needed anymore.
    /// For example, this allows to automatically remove the wrapped object from a list, when it is remotely "destroyed".
    /// </summary>
    /// <typeparam name="T">Type of the wrapped object</typeparam>
    public class RefWrapper<T> : RefWrapper
    {

		private bool destroyed = false;

        private event Action onClearance;
        private Dictionary<string,Action> storedHandlers = new Dictionary<string, Action>();
        private Action nullifyPrivateRefToWrapper;
        
        public RefWrapper(T wrappedObject, Action nullifyPrivateRefToWrapper)
        {
            Constructor1(wrappedObject, nullifyPrivateRefToWrapper);
        }

        protected virtual void Constructor1(T wrappedObject, Action nullifyPrivateRefToWrapper)
        {
            Special_SetWrappedObject(wrappedObject);
            Constructor2(nullifyPrivateRefToWrapper);
        }

        protected virtual void Constructor2(Action nullifyPrivateRefToWrapper)
        {
            this.nullifyPrivateRefToWrapper = nullifyPrivateRefToWrapper;
        }

        private T _wrappedObject;

        protected virtual void Special_SetWrappedObject(T value)
        {
            _wrappedObject = value;
        }

        public virtual T GetWrappedReference()
        {
            return _wrappedObject;
        }

        public override void SubscribeOnClearance(Action action)
        {
            onClearance += action;
        }

        /*public override void UnsubscribeOnClearance(Action action)
        {
            OnClearance -= action;
        }*/

        public override void SubscribeOnClearance(string key, Action action)
        {
            if (storedHandlers.ContainsKey(key))
                throw new SystemException("Already known key...");
            else
            {
                storedHandlers.Add(key,action);
                onClearance += storedHandlers[key];
            }
        }

        public override void UnsubscribeOnClearance(string key)
        {
            if (!storedHandlers.ContainsKey(key))
                throw new SystemException("Unknown key...");
            else
            {
                onClearance -= storedHandlers[key];
                storedHandlers.Remove(key);
            }
        }
        
        public override void UnsubscribeOnClearanceAll()
        {
            onClearance = null;
        }        

        public override void DestroyWrappedReference()
        {
            Clear(true,true);
        }
        
        private void Clear(bool destroyReference, bool unsubAllOnClearance)
        {
            if (destroyReference && !unsubAllOnClearance)
                throw new SystemException("Impossible case : destroy Reference without unsubbing from OnClearance.");
            
            if (!destroyed)
            {
                if (destroyReference)
                    onClearance += nullifyPrivateRefToWrapper;

                // The idea behind is to allow the (very possibly private) reference to the wrapper 
                // from the wrapped object is set to null
                // after all other handlers have been executed.

                // if(GetWrappedReference() != default(T)) ???
                onClearance.Invoke();

                if (unsubAllOnClearance)
                {
                    onClearance = null; // Automatically unsubscribes everything
                    storedHandlers.Clear();
                }

                if (destroyReference)
                {
                    Special_SetWrappedObject(default(T));
                    destroyed = true;
                }
          
            }
        }

    }

    public class RefWrapper2<T> : RefWrapper<T>
    {
        protected T _wrappedObject;
        
        public override T GetWrappedReference()
        {
            return _wrappedObject;
        }

        protected override void Special_SetWrappedObject(T value)
        {
            _wrappedObject = value;
        }

        public U GetCastReference<U>() where U : T
        {
            return (U) GetWrappedReference();
        }

        public RefWrapper2(T wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
        }

    }

    
}
