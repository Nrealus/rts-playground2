using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Units
{
    /****** Author : nrealus ****** Last documentation update : 12-07-2020 ******/

    ///<summary>
    /// The locomotion component for unit pieces.
    ///</summary>
    public class UnitMover : MonoBehaviour
    {
        
        private void Start()
        {

        }

        private void Update()
        {

        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        private float speed;
        public void MoveToPosition(Vector3 position, float desiredSpeed, int mode = 1)
        {
            Vector3 d = (position - transform.position);

            if (mode == 0)
            {
                speed = desiredSpeed;
            }
            else if (mode == 1)
            {
                float accel;
                if (Mathf.Abs(desiredSpeed - speed) < desiredSpeed / 10)
                    accel = 0;
                else
                    accel = Mathf.Sign(desiredSpeed - speed);
                accel *= Time.deltaTime;

                speed = Mathf.Lerp(speed, desiredSpeed, accel);
            }

            transform.position += d.normalized * speed;

        }

        public bool DistanceConditionToPosition(Vector3 position, float distance)
        {
            return (transform.position - position).magnitude <= distance;
        }

    }
}