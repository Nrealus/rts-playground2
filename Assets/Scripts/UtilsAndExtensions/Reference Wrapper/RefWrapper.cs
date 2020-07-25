using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nrealus.Extensions.ReferenceWrapper
{

    /****** Author : nrealus ****** Last documentation update : 24-07-2020 ******/

    /// <summary>
    /// A wrapper class whose initial purpose is to remove any direct references to the wrapped object.
    /// In fact, a multitude of direct references to an object makes it hard to keep track of all of them, and can result in unwanted scenarios,
    /// such as an object outliving its supposed lifespan instead of being collected after it's no longer used (or "destroyed"), because of some remaining non-null references to the object.
    /// To prevent that, we use references to a wrapper that contains one single reference to this object.
    /// Once the object is not needed anymore, we set the reference to it in the wrapper to null, and "clear" the rest of the wrapper.
    /// "Used" wrappers do not contain any useful information anymore, and will then eventually be collected, or may be used again with a pooling system.
    /// </summary>
    /// <typeparam name="T">Type of the wrapped object</typeparam>
    public class RefWrapper<T>
    {
        public T Value { get; private set; }

        public RefWrapper(T value)
        {
            Value = value;
        }

        protected void DestroyRef()
        {
            Value = default(T);
        }

        public override bool Equals(object obj)
        {
            var otherWrapper = obj as RefWrapper<T>;

            return object.ReferenceEquals(this, obj) 
                || (!object.ReferenceEquals(null, Value)
                    && Value.Equals(otherWrapper.Value));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // Choose large primes to avoid hashing collisions
                const int HashingBase = (int)2166136261;
                const int HashingMultiplier = 16777619;

                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ (!object.ReferenceEquals(null, Value) ? Value.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ (!object.ReferenceEquals(null, typeof(T)) ? typeof(T).GetHashCode() : 0);
                return hash;
            }
        }

        public static bool operator ==(RefWrapper<T> wrapper1, RefWrapper<T> wrapper2)
        {
            if (object.ReferenceEquals(wrapper1, wrapper2))
            {
                return true;
            }

            if(object.ReferenceEquals(null, wrapper2))
            {
                return false;
            }

            return (wrapper1.Equals(wrapper2));
        }

        public static bool operator !=(RefWrapper<T> wrapper1, RefWrapper<T> wrapper2)
        {
            return !(wrapper1 == wrapper2);
        }

    }

}
