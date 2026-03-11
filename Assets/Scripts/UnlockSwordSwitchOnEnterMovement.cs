using StarterAssets;
using UnityEngine;

public class UnlockSwordSwitchOnEnterMovement: StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Animator 在角色身上：优先 GetComponent；若 Animator 在子物体上用 InParent
        var player = animator.GetComponent<ThirdPersonController>();
        
        if (player == null) return;
        Debug.Log("进入当前状态");
        // 推荐：由 player 持有 swordDir 引用
        if (player.swordDir != null)
            player.swordDir.UnlockSwitch(); // 或 UnlockSwitch()

        var driver = animator.GetComponent<AnimatorLayerWeightDriver>();
        if (driver) driver.SetUpperTarget(1f);
    }
}
