using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Faction
{ 
    /****** Author : nrealus ****** Last documentation update : 23-04-2020 ******/

    /// <summary>
    /// Allows us to "tag" an object with affiliation to a certain faction, via its FactionData scriptable object asset.
    /// Not really used too much for now, just a placeholder for an eventual further use
    /// </summary>    
    public class FactionAffiliation : MonoBehaviour
    {
        [SerializeField] private FactionData _faction;
        public FactionData GetFaction() { return _faction; }

    }
}