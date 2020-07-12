using System.Collections;
using System.Collections.Generic;
using Core.Deployables;
using Core.Helpers;
using UnityEngine;

namespace Core.MapMarkers
{
    /****** Author : nrealus ****** Last documentation update : 23-04-2020 ******/

    /// <summary>
    /// A MapMarker subclass, used to map buildings or constructibles on the map, linked to a DeployableStructure (through a DeployableStructureWrapper)
    /// </summary>   
    public class DeployableMarker : MapMarker, IHasRefWrapper<MapMarkerWrapper<DeployableMarker>>
    {

        public new MapMarkerWrapper<DeployableMarker> GetRefWrapper()
        {
            return _myWrapper as MapMarkerWrapper<DeployableMarker>;
        }

        public static DeployableMarker CreateInstance(Vector3 position)
        {
            DeployableMarker res = Instantiate<DeployableMarker>(
                GameObject.Find("ResourcesList").GetComponent<ResourcesListComponent>().buildingMarkerPrefab,
                GameObject.Find("UI World Canvas").transform);
            
            res.Init(position);

            return res;
        }

        public DeployableStructureWrapper DeployableStructureWrapper { get; private set; }
        
        public DeployableStructureWrapper CreateAndSetDeployableStructure<T>() where T : DeployableStructure
        {
            DeployableStructure bs = new DeployableStructure();
            DeployableStructureWrapper = bs.GetRefWrapper();
            DeployableStructureWrapper.buildingMarkerWrapper = GetRefWrapper();

            //GetMyWrapper<BuildingMarker>().SubscribeOnClearance(() => { DeployableStructureWrapper.DestroyWrappedReference(); } );

            return DeployableStructureWrapper;
        }


        private void Init(Vector3 position)
        {
            transform.position = position;

            _myWrapper = new MapMarkerWrapper<DeployableMarker>(this, () => {_myWrapper = null;});
            GetRefWrapper().SubscribeOnClearance(DestroyMe);            
        }

        private void Update()
        {

        }

        private void DestroyMe()
        {
            Destroy(gameObject);
        }
    }

}