using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Helpers
{
    /****** Author : nrealus ****** Last documentation update : 23-04-2020 ******/
    
    /// <summary>
    /// This interface is simply here to mark a class that needs to access a camera object.
    /// </summary>
    public interface IHasRefToCamera
    {
        Camera GetMyCamera();

    }
}