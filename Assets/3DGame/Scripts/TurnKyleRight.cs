using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnKyleRight : MonoBehaviour {

    public float tolerance;
    public float AngleTolerance;

    public Transform[] TurnRightTrigger;

    public Transform currentTrigger;

    private Animator animator;

    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update () {
		foreach (var t in TurnRightTrigger)
        {
            if ((t.position-transform.position).magnitude < tolerance)
            {
                currentTrigger = t;
                TurnRight();
                return;
            }
        }

        if (currentTrigger != null && Vector3.Angle(transform.forward, currentTrigger.forward) < AngleTolerance)
        {
            StopTurnRight();
            return;
        }

    }

    void TurnRight()
    {
        animator.SetBool("TurnRight", true);
    }

    void StopTurnRight()
    {
        animator.SetBool("TurnRight", false);
        currentTrigger = null;
    }
}
