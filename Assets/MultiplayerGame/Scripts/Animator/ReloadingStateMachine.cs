using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class ReloadingStateMachine : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Character character = animator.GetComponent<Character>();

        if (character != null)
        {
            character.ReloadFinished();
        }
    }
}