using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Faction
{ 
    /// <summary>
    /// Allows us to "tag" an object with affiliation to a certain faction, via its FactionData scriptable object asset.
    /// </summary>
    public class FactionAffiliation : MonoBehaviour
    {
        [SerializeField] private FactionData myFaction;
        public FactionData MyFaction { get { return myFaction; } private set { myFaction = value; } }

    }
}