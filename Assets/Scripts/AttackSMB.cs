using StarterAssets;
using UnityEngine;

public class AttackSMB : StateMachineBehaviour
{
    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        // 进入 Attack 子状态机
        var ctrl = animator.GetComponent<ThirdPersonController>(); // 或你自己的脚本
        //if (ctrl != null) ctrl.IsInAttackSM = true;

        animator.applyRootMotion = true; // 需要时可在这里开
    }

    public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        // 退出 Attack 子状态机
        var ctrl = animator.GetComponent<ThirdPersonController>();
        //if (ctrl != null) ctrl.IsInAttackSM = false;

        animator.applyRootMotion = false; // 需要时可在这里关（按你项目需求）
    }
}
