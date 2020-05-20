using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;

namespace Core.Helpers
{
    /****** Author : nrealus ****** Last documentation update : 23-04-2020 ******/

    /// <summary>
    /// This interface is here to mark a class that needs to have a ReferenceWrapper "container" for it.
    /// </summary>    
    public interface IHasRefToRefWrapper<T> where T : RefWrapper
    {
        T GetMyWrapper();

        //void ClearWrapper();
    }
}