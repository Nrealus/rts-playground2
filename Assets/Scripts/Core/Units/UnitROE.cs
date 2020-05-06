using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Units
{

    public class UnitROE
    {
        
        public struct WeaponROE
        {   

            public float sectorDirection;
            public float sectorAngle;
            public float sectorMinDistance;
            public float sectorMaxDistance;
            public float minimumAccuracyToEngage;
            public bool askForPermissionToEngage;

        }

        public List<WeaponROE> weaponsROE;

    }
}