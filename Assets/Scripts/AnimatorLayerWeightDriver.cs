using UnityEngine;

public class AnimatorLayerWeightDriver : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("Layer Names")]
    public string upperLayerName = "UpperLayer";

    [Header("Blend")]
    [Tooltip("权重变化速度(每秒)。越大切换越快")]
    public float weightSpeed = 8f;

    private int _upperLayerIndex = -1;
    private float _upperTarget = 1f;

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
        _upperLayerIndex = animator.GetLayerIndex(upperLayerName);

        if (_upperLayerIndex < 0)
            Debug.LogError($"Animator 没找到层：{upperLayerName}");

        // 可选：初始就打开上半身
        SetUpperTarget(1f, true);
    }

    void Update()
    {
        if (_upperLayerIndex < 0) return;

        float current = animator.GetLayerWeight(_upperLayerIndex);
        float next = Mathf.MoveTowards(current, _upperTarget, weightSpeed * Time.deltaTime);
        animator.SetLayerWeight(_upperLayerIndex, next);
    }

    /// <summary> 设置 UpperLayer 目标权重（0~1） </summary>
    public void SetUpperTarget(float target, bool snap = false)
    {
        _upperTarget = Mathf.Clamp01(target);

        if (snap && _upperLayerIndex >= 0)
            animator.SetLayerWeight(_upperLayerIndex, _upperTarget);
    }
}
