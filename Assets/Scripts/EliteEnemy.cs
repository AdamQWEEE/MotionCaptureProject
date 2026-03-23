using UnityEngine;

public class EliteEnemy : EnemyBase
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

    public enum EliteActionType
    {
        LightAttack,
        HeavyAttack,
        GuardUp,
        GuardDown,
        GuardLeft,
        GuardRight
    }

    [Header("Elite")]
    public float actionInterval = 1f;   // 每次攻防动作后的间隔
    private bool canAct = true;         // 是否允许下一次攻防
    

    private Animator animator;
    [Header("IK Controller")]
    public HandIKController IKController;


    protected override void Start()
    {
        base.Start();
        enemyType = EnemyType.Melee;
        animator = GetComponentInChildren<Animator>();
        IKController = GetComponentInChildren<HandIKController>();
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
        //StopMove();
        //FaceTarget();

        //// ===== [新增] 攻击时速度设为0 =====
        //if (animator != null)
        //    animator.SetFloat("MeleeSpeed", 0f);

        //if (attackTimer <= 0f)
        //{
        //    Attack();
        //    attackTimer = attackCooldown;
        //}

        StopMove();
        FaceTarget();

        if (animator != null)
            animator.SetFloat("MeleeSpeed", 0f);

        // attackTimer 仍然作为“间隙计时器”使用
        if (attackTimer > 0f) return;

        if (!canAct) return;

        Attack();
    }

    protected override void Attack()
    {
        canAct = false;

        EliteActionType action = GetRandomAction();

        switch (action)
        {
            case EliteActionType.LightAttack:
                Debug.Log($"{name} 播放轻攻击");
                if (animator != null) animator.SetTrigger("LightAttack");
                break;

            case EliteActionType.HeavyAttack:
                Debug.Log($"{name} 播放重攻击");
                if (animator != null) animator.SetTrigger("HeavyAttack");
                break;

            case EliteActionType.GuardUp:
                Debug.Log($"{name} 播放上防御");
                defenceDirID = 1;
                if (animator != null) animator.SetTrigger("GuardUp");
                break;

            case EliteActionType.GuardDown:
                Debug.Log($"{name} 播放下防御");
                defenceDirID = 2;
                if (animator != null) animator.SetTrigger("GuardDown");
                break;

            case EliteActionType.GuardLeft:
                Debug.Log($"{name} 播放左防御");
                defenceDirID = 3;
                if (animator != null) animator.SetTrigger("GuardLeft");
                break;

            case EliteActionType.GuardRight:
                Debug.Log($"{name} 播放右防御");
                defenceDirID = 4;
                if (animator != null) animator.SetTrigger("GuardRight");
                break;
        }

        // 这里可以接Animator参数
        // animator.SetTrigger("Attack" + comboIndex);

        // 实际伤害检测可以用 OverlapSphere / 动画事件
    }

    private EliteActionType GetRandomAction()
    {
        //int roll = Random.Range(0, 6);
        int roll = 2;//临时测试用，测试完恢复随机数
        switch (roll)
        {
            case 0: return EliteActionType.LightAttack;
            case 1: return EliteActionType.HeavyAttack;
            case 2: return EliteActionType.GuardUp;
            case 3: return EliteActionType.GuardDown;
            case 4: return EliteActionType.GuardLeft;
            default: return EliteActionType.GuardRight;
        }
    }

    // ===== 动画事件：在每个攻防动作快结束时调用 =====
    public void ResetActionCooldown()
    {
        attackTimer = actionInterval;
        canAct = true;
    }

    public override void TakeDamage(float damage)
    {
        if (isDead) return;
        if (failDefence) 
        {
            Debug.Log("精英怪受伤");
            currentHP -= damage;//不在防御或者防御方向不对才受伤并执行后续逻辑
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
        else
        {
            Debug.Log("精英怪防御住攻击");
        }

        
    }

    protected override void OnHurt()
    {
        StopMove();
        IKController.CloseIK();//受伤时关闭IK约束

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
        attackTimer = 1f;
        canAct = true;
        isHurt = false;
        IKController.OpenIK();//恢复后开启IK判定

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
        IKController.CloseIK();
        // ===== [修改] 播放死亡动画 =====
        if (animator != null)
        {
            animator.SetTrigger("Die");
            animator.SetFloat("MeleeSpeed", 0f);
        }

        Debug.Log($"{name} 近战怪死亡");

        // 如果你没有给死亡动画末尾加事件，就保留延时销毁
        Destroy(gameObject, 5f);
    }


    public void EndDie()
    {
        Destroy(gameObject);
    }
}
