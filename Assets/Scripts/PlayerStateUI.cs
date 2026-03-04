using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStateUI : MonoBehaviour
{
    public bool recoverEnergy;
    public Image energyBar;
    public float energy_per_attack;
    [Header("MagicBar")]
    public Image magicBar;

    [Header("Refs")]

    public Image hpFill;                // 红条
    public Image bufferFill;            // 缓冲条

    [Header("HP")]
    public float maxHp = 100f;
    public float currentHp = 100f;
    private float _bufferTarget;

    [Header("Buffer")]
    public float bufferSpeed = 0.5f;    // 缓冲条缩减速度（0.5 ~ 2 自己调）
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    [Header("Stance")]
    public Image stance_right;
    public Image stance_left;
    public float stanceValue;
    public ThirdPersonController player;
    void Start()
    {
        currentHp = maxHp;
        hpFill.fillAmount = 1f;
        bufferFill.fillAmount = 1f;
        _bufferTarget = 1f;
        player = GetComponent<ThirdPersonController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (recoverEnergy && energyBar.fillAmount<1)
        {
            energyBar.fillAmount +=0.2f* Time.deltaTime;
        }
        else
        {
            recoverEnergy = false;
        }

        if (stanceValue > 0)
        {
            stance_left.fillAmount = 0;
            stance_right.fillAmount = stanceValue;
        }
        else if (stanceValue == 0)
        {
            stance_left.fillAmount = 0f;
            stance_right.fillAmount = 0f;
        }
        else if (stanceValue < 0)
        {
            stance_left.fillAmount = stanceValue*(-1f);
            stance_right.fillAmount = 0f;
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            //stanceValue =Mathf.Min(stanceValue + 0.3f,1f);
            //player.swordDirUI.
            
            
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            //stanceValue = Mathf.Max(stanceValue - 0.3f, -1f);

        }
    }

    private void LateUpdate()
    {
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

    public void ConsumeEnergy()
    {
        if (energyBar.fillAmount>0f)
        {
            energyBar.fillAmount -= energy_per_attack;
        }
        
    }

    public void ConsumeMagic(float amount)
    {
        if(magicBar.fillAmount > 0f)
        {
            magicBar.fillAmount -= amount;
        }
    }

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
        Debug.Log("造成一次伤害");
        if (currentHp == 0)
        {
            GetComponent<ThirdPersonController>().PlayerDead();
        }
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
