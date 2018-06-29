using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game3D.Camera
{

    public class LookAtTarget : MonoBehaviour
    {

        public MoveTarget cameraPositionTarget;

        public Vector3 targetPosCache;
        public Vector3 ownPosCache;
        public Vector3 oldForward;
        public Vector3 targetForward;
        //public Quaternion ownRotationCache;
        //public Quaternion targetRotationCache;

        public float speed;
        public float progress;

        // Use this for initialization
        void Start()
        {
            targetPosCache = transform.position;
            ownPosCache = transform.position;
            //ownRotationCache = transform.rotation;
            //targetRotationCache = transform.rotation;
            oldForward = transform.forward;
            targetForward = transform.forward;
        }

        // Update is called once per frame
        void Update()
        {
            var newPos = cameraPositionTarget.transform.position;
            var ownPos = transform.position;

            if (targetPosCache != newPos || ownPosCache != ownPos)
            {
                progress = 0;
                targetPosCache = newPos;
                ownPosCache = ownPos;
                //ownRotationCache = transform.rotation;
                //Vector3 direction = targetPosCache - ownPosCache;

                oldForward = transform.forward;
                targetForward = targetPosCache - ownPosCache;
                //targetRotationCache = Quaternion.FromToRotation(transform.forward, direction);
            }

            //transform.rotation = Quaternion.Lerp(ownRotationCache, targetRotationCache, progress);

            transform.forward = Vector3.Lerp(oldForward, targetForward, progress);

            //Have to cache transform.rotation

            if (progress <= 1)
            {
                progress += speed;
            }
        }
    }
}