using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceHealthBar : MonoBehaviour
{
    [Header("Refs")]   

    public Image hpFill;                // 红条
    public Image bufferFill;            // 缓冲条

    [Header("HP")]
    public float maxHp = 100f;
    public float currentHp = 100f;

    [Header("Buffer")]
    public float bufferSpeed = 0.5f;    // 缓冲条缩减速度（0.5 ~ 2 自己调）

    private float _bufferTarget;        // 缓冲条目标 fillAmount
    private Camera _cam;

    void Awake()
    {
        _cam = Camera.main;
        currentHp = maxHp;
        hpFill.fillAmount = 1f;
        bufferFill.fillAmount = 1f;
        _bufferTarget = 1f;
    }

    void LateUpdate()
    {
        
        if (_cam != null)
        {
            // 简单 billboard：正面永远朝向摄像机方向
            transform.forward = _cam.transform.forward;
            // 或：transform.LookAt(transform.position + _cam.transform.rotation * Vector3.forward,
            //                     _cam.transform.rotation * Vector3.up);
        }

        // 缓冲条缓慢追赶目标 fillAmount
        if (bufferFill != null)
        {
            if (bufferFill.fillAmount > _bufferTarget)
            {
                bufferFill.fillAmount = Mathf.MoveTowards(
                    bufferFill.fillAmount,
                    _bufferTarget,
                    bufferSpeed * Time.deltaTime
                );
            }
        }


    }

    /// <summary>受到伤害：红条立刻跳变，缓冲条慢慢缩到新值</summary>
    public void TakeDamage(float damage)
    {
        if (damage <= 0f) return;

        currentHp = Mathf.Max(0f, currentHp - damage);
        float normalized = currentHp / maxHp;

        // 红条直接跳到真实血量
        if (hpFill != null)
            hpFill.fillAmount = normalized;

        // 缓冲条的目标值更新到更小的目标
        if (normalized < _bufferTarget)
            _bufferTarget = normalized;
    }

    /// <summary>如果以后有加血逻辑，可以单独控制缓冲条回升</summary>
    public void Heal(float value)
    {
        if (value <= 0f) return;

        currentHp = Mathf.Min(maxHp, currentHp + value);
        float normalized = currentHp / maxHp;

        if (hpFill != null)
            hpFill.fillAmount = normalized;

        // 加血时你可以选择：缓冲条立刻跳到新值 or 慢慢涨
        _bufferTarget = normalized;
        bufferFill.fillAmount = normalized;
    }
}
