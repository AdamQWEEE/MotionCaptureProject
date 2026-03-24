using StarterAssets;
using UnityEngine;

public class RangedEnemy : EnemyBase
{
    [Header("Ranged Attack")]
    public GameObject boomerangPrefab;
    public Transform firePoint;
    public bool useHomingOnOut = false;

    [Header("Attack Interval")]
    public float attackInterval = 5f;   // ===== [新增] 攻击间隙：攻击后5秒才能再攻击 =====
    public float attackIntervalTimer = 0f; // ===== [新增] 攻击间隙计时器 =====
    private bool canAttack = true; // ===== [新增] 当前是否允许攻击 =====

    [Header("Hurt")]
    [Range(0f, 1f)]
    public float hurtChance = 0.6f;

    private Animator animator;
    private bool isAttacking = false;

    protected override void Start()
    {
        base.Start();

        enemyType = EnemyType.Ranged;
        usePatrol = false;
        currentState = EnemyState.Idle;

        animator = GetComponentInChildren<Animator>();

        // ===== [保留] 远程敌人不需要追击 =====
        chaseRange = attackRange;
    }

    protected override void Update()
    {
        // ===== [新增] 处理攻击间隙倒计时 =====
        if (!canAttack)
        {
            attackIntervalTimer -= Time.deltaTime;
            if (attackIntervalTimer <= 0f)
            {
                canAttack = true;
            }
        }

        base.Update();
    }

    protected override void CheckStateTransition()
    {
        if (target == null || isDead || isHurt) return;

        float distance = Vector3.Distance(transform.position, target.position);

        // ===== [修改] 只有在攻击范围内 且 canAttack=true 时，才进入 Attack =====
        if (distance <= attackRange && canAttack)
        {
            currentState = EnemyState.Attack;
        }
        else
        {
            currentState = EnemyState.Idle;
        }
    }

    protected override void UpdateIdle()
    {
        if (animator != null)
            animator.SetFloat("Speed", 0f);
    }

    protected override void UpdateChase()
    {
        // ===== [保留] 远程敌人不追击，直接保持 Idle =====
        currentState = EnemyState.Idle;
    }

    protected override void UpdateAttack()
    {
        if (target == null) return;
        if (isAttacking) return;

        // ===== [新增] 如果当前不允许攻击，就直接回 Idle =====
        if (!canAttack)
        {
            currentState = EnemyState.Idle;
            return;
        }

        FaceTarget();
        Attack();
    }

    protected override void Attack()
    {
        isAttacking = true;
        //attackIntervalTimer=attackInterval;

        if (animator != null)
        {
            animator.SetTrigger("Throw");
        }
        else
        {
            // 没动画时兜底
            ThrowBoomerang();
            EndAttack();
        }
    }

    // ===== 动画事件调用：投掷动作中真正扔出回旋镖 =====
    public void ThrowBoomerang()
    {
        if (boomerangPrefab == null || firePoint == null || target == null) return;

        GameObject obj = Instantiate(boomerangPrefab, firePoint.position, Quaternion.identity);

        Vector3 dir = (target.position - firePoint.position).normalized;

        Boomerang boomerang = obj.GetComponent<Boomerang>();
        if (boomerang != null)
        {
            boomerang.Init(transform, dir, target, useHomingOnOut);
        }
    }

    // ===== 动画事件调用：攻击动画快结束时触发 =====
    public void EndAttack()
    {
        isAttacking = false;

        // ===== [新增] 攻击结束后进入攻击间隙 =====
        canAttack = false;
        attackIntervalTimer = attackInterval;

        if (!isDead && !isHurt)
            currentState = EnemyState.Idle;
    }

    public override void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHP -= damage;
        if (hpBar != null) hpBar.TakeDamage((int)damage);

        if (currentHP <= 0f)
        {
            Die();
            return;
        }

        // 60%概率进入受击
        if (!isHurt && Random.value <= hurtChance)
        {
            isHurt = true;
            currentState = EnemyState.Hurt;
            OnHurt();
        }
    }

    protected override void OnHurt()
    {
        isAttacking = false;

        // ===== [可选新增] 受击时不重置攻击冷却，保持当前攻击间隙逻辑 =====
        // 如果你想“受击后立刻允许重新攻击”，可以在这里写：
        // canAttack = true;
        // attackIntervalTimer = 0f;

        if (animator == null) return;

        int hurtIndex = Random.Range(1, 5); // 1~4
        animator.SetTrigger("Hurt" + hurtIndex);
    }

    protected override void UpdateHurt()
    {
        // ===== [保留] Hurt恢复由动画事件 EndHurt() 控制 =====
    }

    // ===== 动画事件调用：Hurt动画快结束时触发 =====
    public void EndHurt()
    {
        if (isDead) return;

        isHurt = false;
        currentState = EnemyState.Idle;
    }

    protected override void Die()
    {
        if (isDead) return;

        isDead = true;
        currentState = EnemyState.Dead;
        isAttacking = false;

        if (animator != null)
        {
            animator.SetTrigger("Die");
            ThirdPersonController.Instance.ChangeToFreeView();
        }
        else
        {
            
            Destroy(gameObject, 3.5f);
        }
    }

    protected override void OnDie()
    {
        // 留空，Die()里直接播动画
    }

    // ===== 动画事件调用：死亡动画末尾 =====
    public void EndDie()
    {
        Destroy(gameObject);
    }
}
