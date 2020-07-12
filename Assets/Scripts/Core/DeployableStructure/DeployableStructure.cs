using Core.Handlers;
using Core.Helpers;
using Core.MapMarkers;
using Core.Units;
using Gamelogic.Extensions;
using GlobalManagers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Deployables
{

    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    /// <summary>
    /// Main class for buildings and constructibles.
    /// Extend this class to implement building specific functionality.
    ///
    /// Instances of DeployableStructure are wrapped in DeployableStructureWrapper.
    /// And the encouraged use of interacting with a DeployableStructure instance is to use static functions with a reference to its wrapper as a parameter.
    ///
    /// For now, this class isn't even abstract, because of testing purposes.
    /// Most of the code is inspired by the Order class, that's why there's a lot of its code that is commented below.
    ///    
    /// In other words, this is still being figured out.
    /// </summary>
    public /*abstract*/ class DeployableStructure :
        IHasRefWrapper<DeployableStructureWrapper>
    {
        #region Static functions

        public static float GetHP(DeployableStructureWrapper DeployableStructureWrapper)
        {
            return DeployableStructureWrapper.GetWrappedReference().InstanceGetHP();
        }

        public static void AddHP(DeployableStructureWrapper DeployableStructureWrapper, float amount)
        {
            DeployableStructureWrapper.GetWrappedReference().InstanceSetHP(DeployableStructureWrapper.GetWrappedReference().InstanceGetHP() + amount);
        }

        /*--------*/

        protected DeployableStructureWrapper _myWrapper;
        public DeployableStructureWrapper GetRefWrapper() { return _myWrapper; }

        /*--------*/

        #endregion

        public DeployableStructure()
        {
            //BaseConstructor(); <-- NO : BECAUSE C# CALLS CONSTRUCTORS "FROM TOP TO BOTTOM" (base then derived)
            // TEMPORARY FOR TESTING (as long as this is not an abstract class)
            _myWrapper = new DeployableStructureWrapper<DeployableStructure>(this, () => {_myWrapper = null;});
        }
        
        #region Protected/Private abstract instance methods

        private float _hp;
        protected virtual float InstanceGetHP()
        {
            return _hp;
        }
        protected virtual void InstanceSetHP(float hp)
        {
            _hp = hp;
        }
        
        #endregion*/
        
    }
}