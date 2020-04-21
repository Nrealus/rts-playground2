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