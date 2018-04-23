using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AC_IdleAnimation : StateMachineObservalbes
{
    public int RandomIdx = 0;

    private void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
         var idx = AnimIndexProvider.GetIdleAniIndex();
        animator.SetInteger("Random",idx);
        RandomIdx = idx;
    }
}
