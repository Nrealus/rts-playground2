using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Helpers
{
    /// <summary>
    /// ---- General Description, by nrealus, last update : 23-04-2020 ----
    ///
    /// This interface is simply here to mark a class that needs to access a camera object.
    /// </summary>    
    public interface IHasCameraRef
    {
        Camera GetMyCamera();

    }
}