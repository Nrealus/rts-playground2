using Gamelogic.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace VariousUtilsExtensions
{
    public abstract class ReferenceWrapper
    {
        
        public abstract int SubscribeOnClearance(Action action);

        public abstract void UnsubscribeOnClearance(Action action);

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
    public class ReferenceWrapper<T> : ReferenceWrapper
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

        //private Dictionary<int, Action> clearanceActionsDictionary;

        //private Action _cachedClearAction;

        /*public ReferenceWrapper(T wrappedObject)
        {
            _wrappedObject = wrappedObject;

			Cleared = false;
			
            //clearanceActionsDictionary = new Dictionary<int, Action>();

        }*/
        
        public ReferenceWrapper(T wrappedObject, Action nullifyPrivateRefToWrapper)
        {
            _wrappedObject = wrappedObject;

			Cleared = false;
			
            this.nullifyPrivateRefToWrapper = nullifyPrivateRefToWrapper;

            //clearanceActionsDictionary = new Dictionary<int, Action>();

        }

        public override int SubscribeOnClearance(Action action)
        {
            OnClearance += action;
            int key = 0;
            /*for(int k = 0; k < clearanceActionsDictionary.Keys.Count; k++)
            {ClearWrapper
                if (clearanceActionsDictionary.ContainsKey(key))
                    key++;
                else
                    break;
            }
            clearanceActionsDictionary.Add(key, action);*/
            return key;
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
                // Needed so the (possibly private) reference to the wrapper 
                // from the wrapped object is set to null 
                // after all other handlers have been executed.

                OnClearance.Invoke();
                OnClearance = null; // Automatically unsubscribe everything

                _wrappedObject = default(T);
            }
        }

    }
}
