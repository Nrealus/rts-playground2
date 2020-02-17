using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;

namespace Core.Helpers
{
    public interface IHasRefWrapperAndTree<T> : IHasRefWrapper<T> where T : ReferenceWrapper
    {
        List<T> GetMeAndAllChildrenWrappersList();

        T GetParentWrapper();
    }
}