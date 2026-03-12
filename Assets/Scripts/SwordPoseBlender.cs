using UnityEngine;
using System.Collections;

public class SwordPoseBlender : MonoBehaviour
{
    public enum Dir { Up = 0, Down = 1, Left = 2, Right = 3, Defense=4, Unlock=5 }

    [Header("Sword Transform (same parent space as grips)")]
    [SerializeField] private Transform sword; // 默认 this.transform

    [Header("Grip Targets (same parent as sword)")]
    [SerializeField] private Transform gripUp;
    [SerializeField] private Transform gripDown;
    [SerializeField] private Transform gripLeft;
    [SerializeField] private Transform gripRight;
    [SerializeField] private Transform gripDefense;
    [SerializeField] private Transform gripUnlock;

    [Header("Blend (Local)")]
    [Tooltip("过渡时间(秒)。0.06~0.15 通常手感不错")]
    [Min(0f)]
    [SerializeField] private float blendTime = 0.4f;

    [Tooltip("使用 SmoothStep 曲线让过渡更自然")]
    [SerializeField] private bool smoothStep = true;

    private Coroutine _co;

    private void Reset()
    {
        sword = transform;
    }

    private Transform GetGrip(Dir dir)
    {
        return dir switch
        {
            Dir.Up => gripUp,
            Dir.Down => gripDown,
            Dir.Left => gripLeft,
            Dir.Right => gripRight,
            Dir.Defense => gripDefense,
            Dir.Unlock => gripUnlock,
            _ => null
        };
    }

    /// <summary>立即把剑 localPose 对齐到目标 grip（常用于初始化）</summary>
    public void SnapToDir(Dir dir)
    {
        if (!sword) sword = transform;
        Transform g = GetGrip(dir);
        if (!g) return;

        sword.localPosition = g.localPosition;
        sword.localRotation = g.localRotation;
    }

    /// <summary>平滑把剑 localPose 对齐到目标 grip（方向切换时调用）</summary>
    public void BlendToDir(Dir dir)
    {
        if (!sword) sword = transform;
        Transform g = GetGrip(dir);
        if (!g) return;

        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(CoBlendLocal(g));
    }

    private IEnumerator CoBlendLocal(Transform targetGrip)
    {
        Vector3 p0 = sword.localPosition;
        Quaternion r0 = sword.localRotation;

        // 注意：目标 grip 可能你在运行时也会调，所以每帧读取目标 localPose
        float t = 0f;
        float inv = (blendTime <= 0f) ? 0f : 1f / blendTime;

        while (t < 1f)
        {
            t += Time.deltaTime * inv;
            float s = Mathf.Clamp01(t);
            if (smoothStep) s = s * s * (3f - 2f * s);

            Vector3 p1 = targetGrip.localPosition;
            Quaternion r1 = targetGrip.localRotation;

            sword.localPosition = Vector3.Lerp(p0, p1, s);
            sword.localRotation = Quaternion.Slerp(r0, r1, s);

            yield return null;
        }

        sword.localPosition = targetGrip.localPosition;
        sword.localRotation = targetGrip.localRotation;
        _co = null;
    }
}
