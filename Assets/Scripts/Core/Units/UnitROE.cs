using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Units
{

    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    ///<summary>
    /// The idea of this class is to encapsulate data and functionality surrounding rules of engagemet for units (unit groups)
    /// They will have values given by and from various things like unit type, but probably mostly the Order that the unit is currently following,
    /// possibly overriding (part of) its default ROE
    /// In-game editing (scaling distance and angles for arcs of fire etc) will probably override all. But a quick switch to default/previous values should be possible.(->preset?)
    ///</summary>
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