using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Faction
{
    /****** Author : nrealus ****** Last documentation update : 23-04-2020 ******/

    /// <summary>
    /// Contains defining data and information about a faction. An object's field can be set to it to communicate that belongs to this certain faction.
    /// Not really used too much for now, just a placeholder for an eventual further use
    /// </summary>    
    [CreateAssetMenu(fileName = "FactionData", menuName = "Faction Data")]
    public class FactionData : ScriptableObject
    {

        public string factionName;
        public Color baseColor;

    }
}

