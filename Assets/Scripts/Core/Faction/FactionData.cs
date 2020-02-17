using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Faction
{
    /// <summary>
    /// Contains defining data and information about a faction. An object's field can be set to it to communicate that belongs to this certain faction.
    /// </summary>
    [CreateAssetMenu(fileName = "FactionData", menuName = "Faction Data")]
    public class FactionData : ScriptableObject
    {

        public string factionName;
        public Color baseColor;

    }
}

