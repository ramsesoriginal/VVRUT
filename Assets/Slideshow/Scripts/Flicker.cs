using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slides
{
    public class Flicker : MonoBehaviour
    {
        public GameObject[] onObjects;
        public GameObject[] offObjects;
        protected GameObject[] currentlyOnObjects;
        protected GameObject[] currentlyOffObjects;
        public float minTime;
        public float maxTime;
        public float nextFlickerTime;

        // Use this for initialization
        void Start()
        {
            currentlyOnObjects = onObjects;
            currentlyOffObjects = offObjects;
            nextFlickerTime = Time.time + Random.Range(minTime, maxTime);
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time > nextFlickerTime)
            {
                foreach (var g in currentlyOnObjects)
                {
                    g.SetActive(false);
                }
                foreach (var g in currentlyOffObjects)
                {
                    g.SetActive(true);
                }
                var t = currentlyOnObjects;
                currentlyOnObjects = currentlyOffObjects;
                currentlyOffObjects = t;
                nextFlickerTime = Time.time + Random.Range(minTime, maxTime);
            }
        }
    }
}
