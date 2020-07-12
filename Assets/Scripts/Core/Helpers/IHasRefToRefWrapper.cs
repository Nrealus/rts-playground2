using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nrealus.Extensions;
using Nrealus.Extensions.ReferenceWrapper;

namespace Core.Helpers
{
    /****** Author : nrealus ****** Last documentation update : 23-04-2020 ******/

    /// <summary>
    /// This interface is here to mark a class that needs to have a RefWrapper "container" for it.
    /// </summary>    
    public interface IHasRefWrapper<T> where T : RefWrapper
    {
        T GetRefWrapper();

        //void ClearWrapper();
    }

    /*public interface IHasRefWrapper2<T, U> : IHasRefWrapper<U> where U : RefWrapper<T>
    {
        new U GetRefWrapper();

        //void ClearWrapper();
    }*/

}