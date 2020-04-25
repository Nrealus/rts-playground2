using System.Collections;
using System.Collections.Generic;
using Core.BuiltStructures;
using UnityEngine;

namespace Core.MapMarkers
{
    /// <summary>
    /// ---- General Description, by nrealus, last update : 23-04-2020 ----
    ///
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

        public BuiltStructureWrapper builtStructureWrapper { get; private set; }

        public Vector3 myPosition { get; private set; }
        
        private BuildingMarkerComponent buildingMarkerComponent;


        public BuiltStructureWrapper CreateAndSetBuiltStructure<T>() where T : BuiltStructure
        {
            BuiltStructure bs = new BuiltStructure();
            builtStructureWrapper = bs.GetMyWrapper<BuiltStructure>();
            builtStructureWrapper.buildingMarkerWrapper = GetMyWrapper<BuildingMarker>();

            //GetMyWrapper<BuildingMarker>().SubscribeOnClearance(() => { builtStructureWrapper.DestroyWrappedReference(); } );

            return builtStructureWrapper;
        }


        public BuildingMarker(Vector3 position)
        {
            this.myPosition = position;
            
            _myWrapper = new MapMarkerWrapper<BuildingMarker>(this, () => {_myWrapper = null;});
            GetMyWrapper<BuildingMarker>().SubscribeOnClearance(DestroyMarkerComponent);

            buildingMarkerComponent = MonoBehaviour.Instantiate<BuildingMarkerComponent>(
                GameObject.Find("ResourcesList").GetComponent<ResourcesListComponent>()
                .buildingMarkerComponentPrefab, GameObject.Find("WorldUICanvas").transform);
            buildingMarkerComponent.transform.position = position;
            buildingMarkerComponent.associatedMarkerWrapper = GetMyWrapper2<MapMarkerWrapper<BuildingMarker>>();
        }

        public override void UpdateMe()
        {
            myPosition = buildingMarkerComponent.transform.position;
        }

        private void DestroyMarkerComponent()
        {
            MonoBehaviour.Destroy(buildingMarkerComponent.gameObject);
            //GetMyWrapper<WaypointMarker>().UnsubscribeOnClearance(DestroyMarkerTransform);
            //GetMyWrapper<WaypointMarker>().UnsubscribeOnClearanceAll();
        }
    }

}