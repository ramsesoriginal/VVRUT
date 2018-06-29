﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game3D.Camera
{

    public class MoveToTarget : MonoBehaviour
    {
        public MoveTarget cameraPositionTarget;

        public Vector3 targetPosCache;
        public Vector3 ownPosCache;

        public float speed;
        private float progress;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            var newPos = cameraPositionTarget.transform.position;

            if (targetPosCache != newPos)
            {
                progress = 0;
                targetPosCache = newPos;
                ownPosCache = transform.position;
            }

            transform.position = Vector3.Lerp(ownPosCache, newPos, progress);

            if (progress > 1)
            {
                progress += speed;
            }
        }
    }
}
