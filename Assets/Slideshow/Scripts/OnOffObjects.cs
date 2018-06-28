using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slides
{
    public class OnOffObjects : MonoBehaviour
    {
        [System.Serializable]
        public class OnOffObjectsList
        {

            [SerializeField]
            public GameObject[] turnOff;

            [SerializeField]
            public GameObject[] turnOn;
        }


        public OnOffObjectsList[] stateObjects;

        private Animator animator;

        public void Start()
        {
            animator = GetComponent<Animator>();
        }

        public void Update()
        {
            if (Input.GetButtonDown("Jump") || Input.GetButtonDown("Fire1"))
            {
                animator.SetTrigger("Next");
            }
        }
    }
}