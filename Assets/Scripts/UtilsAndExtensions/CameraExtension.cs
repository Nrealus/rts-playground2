using System;
using System.Collections.Generic;
using UnityEngine;


namespace Nrealus.Extensions
{

    public static class CameraExtension
    {
        private static Vector3 _pointed; // weird
        private static RaycastHit _rh;
        public static Vector3 GetPointedPositionPhysRaycast(this Camera me, Vector3 screenCoordinates)
        {
            if (Physics.Raycast(me.ScreenPointToRay(screenCoordinates), out _rh, Mathf.Infinity))
            {
                _pointed = _rh.point;
            }
            return _pointed;
        }

        public static bool GetWorldPosCloseToScreenPos(this Camera me, Vector3 worldPosition, Vector3 screenPosition, float proximityDistance)
        {
            Vector3 sp = me.WorldToScreenPoint(worldPosition);
            sp.z = 0;
            return (sp - screenPosition).magnitude < proximityDistance;
        }

    }
}

