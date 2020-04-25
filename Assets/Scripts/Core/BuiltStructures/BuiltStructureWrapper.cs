using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;
using Core.Handlers;
using GlobalManagers;
using System;
using Core.MapMarkers;

namespace Core.BuiltStructures
{

    /// <summary>
    /// ---- General Description, by nrealus, last update : 23-04-2020 ----
    ///
    /// The wrapper class for BuiltStructures. Used as an isolating container for a BuiltStructure instance.
    /// It is also linked with a BuildingMarker through a MapMarkerWrapper
    /// Just as with BuiltStructure, this is still being figured out.
    /// </summary>
    public class BuiltStructureWrapper<T> : BuiltStructureWrapper where T : BuiltStructure
    {
        public BuiltStructureWrapper(BuiltStructure wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
        }
        
    }

    public abstract class BuiltStructureWrapper : ReferenceWrapper<BuiltStructure>
    {
        public BuiltStructureWrapper(BuiltStructure wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
            //SubscribeOnClearance(() => {if (buildingMarkerWrapper != null ) { buildingMarkerWrapper.DestroyWrappedReference(); } });
        }        

        public T GetWrappedAs<T>() where T : BuiltStructure
        {
            return WrappedObject as T;
        }

        public MapMarkerWrapper<BuildingMarker> buildingMarkerWrapper;        

    }
}