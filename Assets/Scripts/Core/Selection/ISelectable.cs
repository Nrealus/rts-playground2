using Core.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nrealus.Extensions;
using Nrealus.Extensions.ReferenceWrapper;
using Nrealus.Extensions.Observer;
using Core.Helpers;

namespace Core.Selection
{

    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    /// <summary>
    /// This interface is implemented by classes whose instances must be able to be "selected" by a Selector object.
    /// </summary>   
    public interface ISelectable : IDestroyable
    {
        EasyObserver<string,(Selector, bool, int)> GetOnSelectionStateChangeObserver();

        void InvokeOnSelectionStateChange(Selector selector, bool newSelectionState, int channel);
    }

}