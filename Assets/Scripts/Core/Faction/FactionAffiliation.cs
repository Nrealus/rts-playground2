using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Faction
{ 
    /// <summary>
    /// ---- General Description, by nrealus, last update : 23-04-2020 ----
    ///
    /// Allows us to "tag" an object with affiliation to a certain faction, via its FactionData scriptable object asset.
    /// Not really used too much for now, just a placeholder for an eventual further use
    /// </summary>    
    public class FactionAffiliation : MonoBehaviour
    {
        [SerializeField] private FactionData myFaction;
        public FactionData MyFaction { get { return myFaction; } private set { myFaction = value; } }

    }
}