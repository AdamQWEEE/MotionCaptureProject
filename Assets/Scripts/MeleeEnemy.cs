using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    [Header("Melee")]
    public int comboCount = 3;
    public float comboResetTime = 5f;

    private int comboIndex = 0;
    private float comboTimer = 0f;

    [Header("Patrol (Waypoints)")]
    public Transform[] patrolPoints;
    public float waitAtPointTime = 1.0f;
    int patrolIndex = 0;
    float waitTimer = 0f;

    [Header("Animation Chances")] // ===== [新增] =====
    [Range(0f, 1f)] public float hurtChance = 0.6f;   // 60%概率触发受击动画
    private Animator animator;


    protected override void Start()
    {
        base.Start();
        enemyType = EnemyType.Melee;
        animator = GetComponentInChildren<Animator>();
        if (usePatrol && (agent == null || patrolPoints == null || patrolPoints.Length == 0))
            usePatrol = false;
    }

    protected override void Update()
    {
        if (isDead) return;
        base.Update();

        if (comboTimer > 0)
            comboTimer -= Time.deltaTime;
        else
            comboIndex = 0;
    }

    protected override void UpdatePatrol()
    {
        if (!usePatrol) { StopMove(); return; }
        if (agent == null || patrolPoints == null || patrolPoints.Length == 0) { StopMove(); return; }

        // 等待
        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            StopMove();
            return;
        }

        Transform p = patrolPoints[patrolIndex];
        if (p == null) return;

        agent.isStopped = false;
        agent.speed = moveSpeed;
        agent.stoppingDistance = 0f;

        // 如果当前没在走向该点，就设置目的地
        if (!agent.hasPath)
            agent.SetDestination(p.position);

        // 到达判定：remainingDistance + pathPending
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        {
            waitTimer = waitAtPointTime;
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            agent.ResetPath();
        }

        if (animator != null)
            animator.SetFloat("MeleeSpeed", 1f);
    }

    protected override void UpdateIdle()
    {
        base.UpdateIdle();

        // ===== [新增] Idle时速度设为0 =====
        if (animator != null)
            animator.SetFloat("MeleeSpeed", 0f);
    }

    protected override void UpdateChase()
    {
        base.UpdateChase();

        // ===== [新增] 追击时速度设为1 =====
        
        if (target == null) return;
        Debug.Log("追逐");

        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = moveSpeed;
            if (animator != null)
                animator.SetFloat("MeleeSpeed", 1f);
            agent.SetDestination(target.position);
            
        }

        FaceTarget();
    }

    protected override void UpdateAttack()
    {
        StopMove();
        FaceTarget();

        // ===== [新增] 攻击时速度设为0 =====
        if (animator != null)
            animator.SetFloat("MeleeSpeed", 0f);

        if (attackTimer <= 0f)
        {
            Attack();
            attackTimer = attackCooldown;
        }
    }

    protected override void Attack()
    {
        comboIndex++;
        if (comboIndex > comboCount)
            comboIndex = 1;

        comboTimer = comboResetTime;

        Debug.Log($"{name} 近战第 {comboIndex} 段攻击");

        bool useLightAttack = Random.value < 0.5f;

        if (useLightAttack)
        {
            Debug.Log($"{name} 播放轻攻击");
            if (animator != null)
                animator.SetTrigger("LightAttack");
        }
        else
        {
            Debug.Log($"{name} 播放重攻击");
            if (animator != null)
                animator.SetTrigger("HeavyAttack");
        }

        // 这里可以接Animator参数
        // animator.SetTrigger("Attack" + comboIndex);

        // 实际伤害检测可以用 OverlapSphere / 动画事件
    }

    public override void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHP -= damage;
        
        if (hpBar != null)
            hpBar.TakeDamage((int)damage);

        if (currentHP <= 0f)
        {
            Die();
            return;
        }

        // 60%概率进入受击状态并随机播放4种受击动画
        if (!isHurt && Random.value <= hurtChance)
        {
            isHurt = true;
            currentState = EnemyState.Hurt;
            OnHurt();
        }
    }

    protected override void OnHurt()
    {
        StopMove();

        // ===== [新增] 随机四选一受击动画 =====
        if (animator != null)
        {
            int hurtIndex = Random.Range(1, 5); // 1~4
            Debug.Log($"{name} 播放受击动画 Hurt{hurtIndex}");
            animator.SetTrigger("Hurt" + hurtIndex);
            animator.SetFloat("MeleeSpeed", 0f);
        }
    }

    protected override void UpdateHurt()
    {
        StopMove();

        // ===== [新增说明] 这里不自动恢复，推荐在Hurt动画末尾加动画事件调用 EndHurt() =====
    }

    public void EndHurt()
    {
        if (isDead) return;

        isHurt = false;

        // 回到基础状态判定
        if (target == null)
        {
            currentState = usePatrol ? EnemyState.Patrol : EnemyState.Idle;
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackRange)
            currentState = EnemyState.Attack;
        else if (distance <= chaseRange)
            currentState = EnemyState.Chase;
        else
            currentState = usePatrol ? EnemyState.Patrol : EnemyState.Idle;
    }

    protected override void OnDie()
    {
        StopMove();
        isDead = true;
        // ===== [修改] 播放死亡动画 =====
        if (animator != null)
        {
            animator.SetTrigger("Die");
            animator.SetFloat("MeleeSpeed", 0f);
        }

        Debug.Log($"{name} 近战怪死亡");

        // 如果你没有给死亡动画末尾加事件，就保留延时销毁
        Destroy(gameObject, 3f);
    }


    public void EndDie()
    {
        Destroy(gameObject);
    }
}
