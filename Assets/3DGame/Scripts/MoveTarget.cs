using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game3D.Camera
{

    public class MoveTarget : MonoBehaviour
    {

        public Transform followThis;

        public Vector3 offset;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            transform.position = followThis.position + offset;
        }
    }
}