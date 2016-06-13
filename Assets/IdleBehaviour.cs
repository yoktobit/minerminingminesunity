using UnityEngine;
using System.Collections;

public class IdleBehaviour : StateMachineBehaviour {

    float rndTime;
    float startTime;
    float twinkleTime;
    Animator animator;

	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetInteger("idleNextAnimation", 0);
        rndTime = UnityEngine.Random.Range(0.0f, 10.0f);
        startTime = Time.time;
        twinkleTime = startTime + rndTime;
        this.animator = animator;
	}

    void Twinkle()
    {
        if (Time.time > twinkleTime)
        {
            animator.SetInteger("idleNextAnimation", 1);
        }
    }

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        Twinkle();
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	//override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
