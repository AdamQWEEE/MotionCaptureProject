using UnityEngine;
using UnityEngine.UI;


public class EnemyStanceBar : MonoBehaviour
{
    [Header("Refs")]

    public Image stanceUI_Left;                // 红条
    public Image stanceUI_Right;            // 缓冲条

    [Header("Stance")]
    public float maxStance = 100f;
    public float currentStance = 0f;
    private Camera _cam;

    public EnemyBase enemy;

    void Awake()
    {
        _cam = Camera.main;
        currentStance = 0;
        stanceUI_Left.fillAmount = 0f;
        stanceUI_Right.fillAmount = 0f;
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

        float normalized = currentStance / maxStance;

        stanceUI_Left.fillAmount = normalized;
        stanceUI_Right.fillAmount = normalized;

        if (currentStance > 0 && currentStance<maxStance)
        {
            currentStance -= Time.deltaTime*2f;
            
        }
        


    }

    /// <summary>受到伤害：红条立刻跳变，缓冲条慢慢缩到新值</summary>
    public void AddStance(float amount)
    {
        if (amount <= 0) {
            return;
        } 



        currentStance = Mathf.Min(maxStance, currentStance + amount);
        if (currentStance == maxStance)
        {
            enemy.LoseBalance();
            enemy.ShowExecutionMarker();
        }
        
    }

    public void ResetStance()
    {
        currentStance = 0;
    }

    /// <summary>如果以后有加血逻辑，可以单独控制缓冲条回升</summary>
    
}
