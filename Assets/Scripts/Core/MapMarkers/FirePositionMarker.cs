using System.Collections;
using System.Collections.Generic;
using Core.Helpers;
using UnityEngine;

namespace Core.MapMarkers
{

    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    /// <summary>
    /// A MapMarker subclass, used to map positions for attack tasks, rules of engagements etc.    
    /// </summary>   
    public class FirePositionMarker : MapMarker, IHasRefWrapper<MapMarkerWrapper<FirePositionMarker>>
    {

        public new MapMarkerWrapper<FirePositionMarker> GetRefWrapper()
        {
            return _myWrapper as MapMarkerWrapper<FirePositionMarker>;
        }

        public static FirePositionMarker CreateInstance(Vector3 position, float radius)
        {
            FirePositionMarker res = Instantiate<FirePositionMarker>(
                GameObject.Find("ResourcesList").GetComponent<ResourcesListComponent>().firePositionMarkerPrefab,
                GameObject.Find("UI World Canvas").transform);
            
            res.Init(position, radius);

            return res;
        }

        private float _radius = 1;
        public float radius
        {
            get {
                return _radius;
            }

            set {
                _radius = value;
                transform.localScale = _radius * new Vector3(1,1,1);
            }
        }

        public float moveSpeed = 0.5f;
        public float offset = 5f;
        public bool following;

        private void Init(Vector3 position, float radius)
        {
            transform.position = position;
            this.radius = radius;            

            _myWrapper = new MapMarkerWrapper<FirePositionMarker>(this, () => {_myWrapper = null;});
            GetRefWrapper().SubscribeOnClearance(DestroyMe);

            following = false;            
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
