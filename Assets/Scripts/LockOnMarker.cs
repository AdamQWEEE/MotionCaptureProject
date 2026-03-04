using UnityEngine;

public class LockOnMarker : MonoBehaviour
{
    public Transform target;          // 敌人（或敌人身上的一个骨骼）
    //public Vector3 offset = Vector3.up * 1.6f; // 调到大概胸口位置
    public Camera cam;                // 跟随的摄像机

    private void Start()
    {
        if (cam == null)
            cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        // 1. 位置跟随敌人中心（可加偏移）
        //transform.position = target.position + offset;

        // 2. X 轴始终朝向摄像机
        //    toCam 为 marker -> camera 方向，我们希望 transform.right = toCam
        Vector3 toCam = (cam.transform.position - transform.position).normalized;
        if (toCam.sqrMagnitude < 0.0001f) return;

        // 计算一个以世界 Up 为上方向的旋转，使 right 对齐到 toCam
        Vector3 up = Vector3.up;
        Vector3 forward = Vector3.Cross(up, toCam); // forward = up × right
        if (forward.sqrMagnitude < 0.0001f) forward = cam.transform.forward;

        transform.rotation = Quaternion.LookRotation(forward, up);
        // 此时:
        //   transform.right  == toCam   （X 轴朝向相机）
        //   transform.up     == up
        //   transform.forward 为与这两者正交的方向
    }

    // 可选：初始化接口，锁定时调用
    public void Init(Transform target, Camera cam = null)
    {
        this.target = target;
        this.cam = cam != null ? cam : Camera.main;
    }
}
