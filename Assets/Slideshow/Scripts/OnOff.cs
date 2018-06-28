﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slides
{
    public class OnOff : StateMachineBehaviour
    {

        public int ObjectsIndex;

        private OnOffObjects obj;

        public GameObject[] turnOff
        {
            get
            {
                return obj.stateObjects[ObjectsIndex].turnOff;
            }
        }

        public GameObject[] turnOn
        {
            get
            {
                return obj.stateObjects[ObjectsIndex].turnOn;
            }
        }

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            obj = animator.GetComponent<OnOffObjects>();
            foreach (var g in turnOff)
            {
                g.SetActive(false);
            }
            foreach (var g in turnOn)
            {
                g.SetActive(true);
            }
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        /*override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            obj = animator.GetComponent<OnOffObjects>();
            foreach (var g in turnOff)
            {
                g.SetActive(false);
            }
            foreach (var g in turnOn)
            {
                g.SetActive(true);
            }
        }*/

        // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}
    }
}