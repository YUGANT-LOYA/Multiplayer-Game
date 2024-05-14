using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolsterStateMachine : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Character character = animator.GetComponent<Character>();

        if (character != null)
        {
            character.HolsterFinished();
        }
    }
}
