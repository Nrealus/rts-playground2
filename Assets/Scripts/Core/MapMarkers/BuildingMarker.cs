using System.Collections;
using System.Collections.Generic;
using Core.BuiltStructures;
using UnityEngine;

namespace Core.MapMarkers
{
    /****** Author : nrealus ****** Last documentation update : 23-04-2020 ******/

    /// <summary>
    /// A MapMarker subclass, used to map buildings or constructibles on the map, linked to a BuiltStructure (through a BuiltStructureWrapper)
    /// </summary>   
    public class BuildingMarker : MapMarker
    {

        /*
        public static void UpdateWaypointMarker(MapMarkerWrapper<WaypointMarker> waypointMarkerWrapper, bool following, Vector3 screenPositionToFollow)
        {
            if(waypointMarkerWrapper.WrappedObject != null)
                waypointMarkerWrapper.GetWrappedAs<WaypointMarker>().waypointMarkerTransform.FollowScreenPosition(following, screenPositionToFollow);
        }
        */

        public static BuildingMarker CreateInstance(Vector3 position)
        {
            BuildingMarker res = Instantiate<BuildingMarker>(
                GameObject.Find("ResourcesList").GetComponent<ResourcesListComponent>().buildingMarkerPrefab,
                GameObject.Find("WorldUICanvas").transform);
            
            res.Init(position);

            return res;
        }

        public BuiltStructureWrapper builtStructureWrapper { get; private set; }
        
        public BuiltStructureWrapper CreateAndSetBuiltStructure<T>() where T : BuiltStructure
        {
            BuiltStructure bs = new BuiltStructure();
            builtStructureWrapper = bs.GetMyWrapper<BuiltStructure>();
            builtStructureWrapper.buildingMarkerWrapper = GetMyWrapper<BuildingMarker>();

            //GetMyWrapper<BuildingMarker>().SubscribeOnClearance(() => { builtStructureWrapper.DestroyWrappedReference(); } );

            return builtStructureWrapper;
        }


        private void Init(Vector3 position)
        {
            transform.position = position;

            _myWrapper = new MapMarkerWrapper<BuildingMarker>(this, () => {_myWrapper = null;});
            GetMyWrapper<BuildingMarker>().SubscribeOnClearance(DestroyMe);            
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