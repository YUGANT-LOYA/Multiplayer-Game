using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class ReloadingStateMachine : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ThirdPersonController controller = animator.GetComponent<ThirdPersonController>();

        if (controller != null)
        {
            //controller.ReloadFinished();
        }
    }
}
