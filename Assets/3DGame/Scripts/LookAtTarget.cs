using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game3D.Camera
{

    public class LookAtTarget : MonoBehaviour
    {

        public MoveTarget cameraPositionTarget;

        public float speed;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            transform.LookAt(cameraPositionTarget.transform);
        }
    }
}