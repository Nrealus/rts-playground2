using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nrealus.Extensions;
using Nrealus.Extensions.ReferenceWrapper;
using System;

namespace Core.Helpers
{
    /****** Author : nrealus ****** Last documentation update : 25-07-2020 ******/
    
    ///<summary>
    /// Classes that implement this interface expose methods that can be used to achieve something similar to a C++ "Destructor".
    /// A typical implementation of this interface uses a private event or observer-type object, which is fired "on destruction" i.e. in DestroyThis.
    /// As such, an instance of an implementing class should be "properly" destroyed with a call to DestroyThis.
    ///</summary>
    public interface IDestroyable
    {
        void SubscribeOnDestruction(string key, Action action);

        void SubscribeOnDestructionAtEnd(string key, Action action);

        void SubscribeOnDestruction(string key, Action action, bool combineActionsIfKeyAlreadyExists);

        void SubscribeOnDestructionAtEnd(string key, Action action, bool combineActionsIfKeyAlreadyExists);

        void UnsubscribeOnDestruction(string key);

        void DestroyThis();

    }

}