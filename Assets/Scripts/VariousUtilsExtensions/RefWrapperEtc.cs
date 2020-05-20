using Gamelogic.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace VariousUtilsExtensions
{

    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/
    
    ///<summary>
    /// This interface provides the ability to add (and remove) event handlers to (from) an object's event, supposed to be invoked when the object is "cleared" or "disposed".
    /// This can actually mean whatever we want. The reason why this interface is intended for this specific use case situation is for overall clarity. 
    /// An implementing class has to define the "OnClearance" event itself and taking care of invoking it when it's appropriate (i.e when it is "cleared" or "disposed" etc.)
    ///</summary>
    public interface IClearable
    {
        void SubscribeOnClearance(Action action);

        void UnsubscribeOnClearance(Action action);

        void UnsubscribeOnClearanceAll();

    }
    
    ///<summary>
    /// Base class for "Reference Wrappers". See RefWrapper<T> for much more details.
    ///</summary>
    public abstract class RefWrapper : IClearable
    {
        
        public abstract void DestroyWrappedReference();

        public abstract void SubscribeOnClearance(Action action);

        public abstract void UnsubscribeOnClearance(Action action);

        public abstract void UnsubscribeOnClearanceAll();

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

        private T _wrappedObject;
        public T WrappedObject { get { return _wrappedObject; } }

		private bool _cleared = false;
        private bool Cleared
		{ 
			get { return _cleared; } 
			set 
			{
				if (_cleared == false && value == true)
					Clear();
				
				_cleared = value;				
			} 
		}

        private event Action OnClearance;

        private Action nullifyPrivateRefToWrapper;
        
        public RefWrapper(T wrappedObject, Action nullifyPrivateRefToWrapper)
        {
            _wrappedObject = wrappedObject;

			Cleared = false;
			
            this.nullifyPrivateRefToWrapper = nullifyPrivateRefToWrapper;

        }

        public override void SubscribeOnClearance(Action action)
        {
            OnClearance += action;
        }

        public override void UnsubscribeOnClearance(Action action)
        {
            OnClearance -= action;
        }
        
        public override void UnsubscribeOnClearanceAll()
        {
            OnClearance = null;
        }        

        public override void DestroyWrappedReference()
        {
            Cleared = true;
        }

        private void Clear()
        {
            if (Cleared == false)
            {
                OnClearance += nullifyPrivateRefToWrapper;
                // The idea behind is to allow the (very possibly private) reference to the wrapper 
                // from the wrapped object is set to null
                // after all other handlers have been executed.

                // if(_wrappedObject != default(T)) ???
                OnClearance.Invoke();
                OnClearance = null; // Automatically unsubscribes everything

                _wrappedObject = default(T);
            }
        }

    }
}
