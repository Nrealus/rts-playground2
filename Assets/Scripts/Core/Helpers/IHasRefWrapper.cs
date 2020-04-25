using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;

namespace Core.Helpers
{
    /// <summary>
    /// ---- General Description, by nrealus, last update : 23-04-2020 ----
    ///
    /// This interface is here to mark a class that needs to have a ReferenceWrapper "container" for it.
    /// </summary>    
    public interface IHasRefWrapper<T> where T : ReferenceWrapper
    {
        T GetMyWrapper();

        //void ClearWrapper();
    }
}