using Cinemachine;
using StarterAssets;
using UnityEngine;

public class CameraModeController : Singleton<CameraModeController>
{
    public CinemachineVirtualCamera vcamFree;
    public CinemachineVirtualCamera vcamLock;
    public CinemachineVirtualCamera vcamExecute;
    public CinemachineTargetGroup lockTargetGroup;

    public Transform player;      // 玩家
    public Transform currentEnemy; // 当前锁定敌人，由锁定系统设置
    public ThirdPersonController playerController;


    bool isLockOn;
    private void Start()
    {
        playerController = player.GetComponent<ThirdPersonController>();
    }
    public void SetLockOn(bool value, Transform enemy = null)
    {
        isLockOn = value;
        currentEnemy = enemy;

        if (isLockOn && currentEnemy != null)
        {
            // 更新 TargetGroup：玩家 + 敌人
            lockTargetGroup.m_Targets = new CinemachineTargetGroup.Target[]
            {
                new CinemachineTargetGroup.Target { target = player,       weight = 1, radius = 0.5f },
                new CinemachineTargetGroup.Target { target = currentEnemy, weight = 1, radius = 0.5f }
            };

            // 提升锁定相机 Priority
            vcamLock.Priority = 20;   // > vcamFree
            vcamFree.Priority = 10;
            //vcamExecute.Priority = 0;

            // 关闭自由相机的输入

        }
        else
        {
            // 退出锁定：只保留玩家在 TargetGroup 里（可选）
            lockTargetGroup.m_Targets = new CinemachineTargetGroup.Target[]
            {
                new CinemachineTargetGroup.Target { target = player, weight = 1, radius = 0.5f }
            };

            // 切回自由相机
            vcamLock.Priority = 5;
            vcamFree.Priority = 15;
            //vcamExecute.Priority = 0;


        }
    }

    public void StartExecuteCamera(Transform enemy=null, Transform lookPoint = null)
    {
        //if (vcamExecute == null) return;

        //isExecuting = true;
        //wasLockOnBeforeExecute = isLockOn;
        currentEnemy = enemy;

        // 处决期间关闭锁定 / 自由相机的优先级
        vcamLock.Priority = 0;
        vcamFree.Priority = 0;

        // 绑定跟随与注视
        //if (playerController != null)
        //{
        //    // 这里用你角色上的 CameraRoot，如果名字不同自己替换
        //    var follow = playerController.PlayerCameraRoot;
        //    vcamExecute.Follow = follow;
        //}
        //else
        //{
        //    vcamExecute.Follow = player;
        //}

        //vcamExecute.LookAt = lookPoint != null ? lookPoint : enemy;

        // 最高优先级，让处决相机接管
        vcamExecute.Priority = 40;

        // 处决时一般也会锁输入，这里只做相机，不处理输入
    }

    public void EndExecuteCamera()
    {
        if (vcamExecute == null) return;

        //isExecuting = false;

        // 降低处决相机优先级
        vcamExecute.Priority = 0;
        vcamLock.Priority = 5;
        vcamFree.Priority = 15;

        // 恢复处决前的相机模式
        //if (wasLockOnBeforeExecute && currentEnemy != null)
        //{
        //    // 重新锁定原来的敌人
        //    SetLockOn(true, currentEnemy);
        //}
        //else
        //{
        //    // 回到自由相机
        //    isLockOn = false;
        //    vcamLock.Priority = 5;
        //    vcamFree.Priority = 15;
        //}
    }
}
