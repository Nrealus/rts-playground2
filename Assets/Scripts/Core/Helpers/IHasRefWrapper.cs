using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;

namespace Core.Helpers
{
    public interface IHasRefWrapper<T> where T : ReferenceWrapper
    {
        T GetMyWrapper();

        void ClearWrapper();
    }
}