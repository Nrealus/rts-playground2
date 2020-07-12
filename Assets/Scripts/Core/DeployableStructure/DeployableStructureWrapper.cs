using Core.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VariousUtilsExtensions;
using Core.Handlers;
using GlobalManagers;
using System;
using Core.MapMarkers;

namespace Core.Deployables
{

    /****** Author : nrealus ****** Last documentation update : 23-04-2020 ******/

    /// <summary>
    /// The wrapper class for DeployableStructures. Used as an isolating container for a DeployableStructure instance.
    /// It is also linked with a BuildingMarker through a MapMarkerWrapper
    /// Just as with DeployableStructure, this is still being figured out.
    /// </summary>
    public class DeployableStructureWrapper<T> : DeployableStructureWrapper where T : DeployableStructure
    {

        public new T GetWrappedReference()
        {
            return _wrappedObject as T;
        }

        protected void Special_SetWrappedObject(T value)
        {
            _wrappedObject = value;
        }

        public DeployableStructureWrapper(T wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
        }

        protected override void Constructor1(DeployableStructure wrappedObject, Action nullifyPrivateRefToWrapper)
        {
            Special_SetWrappedObject(wrappedObject as T);
            Constructor2(nullifyPrivateRefToWrapper);
        }

    }

    public abstract class DeployableStructureWrapper : RefWrapper2<DeployableStructure>
    {
        
        public DeployableStructureWrapper<T> CastWrapper<T>() where T : DeployableStructure
        {
            return (DeployableStructureWrapper<T>) this;
        }

        public DeployableStructureWrapper(DeployableStructure wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {
        }

        public MapMarkerWrapper<DeployableMarker> buildingMarkerWrapper;        

    }

}