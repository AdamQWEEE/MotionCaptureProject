using Unity.VisualScripting;
using UnityEngine;
using static EnemyBase;

public class BossEnemy : EnemyBase
{
    public enum BossPhaseState
    {
        Normal,
        Weak,
        Executed,
        Dead
    }

    public enum BossActionType
    {
        None,

        // 近战攻击
        LeftClaw,
        RightClaw,
        CrossClaw,

        // 远程/中距离攻击
        Roar,
        SmashAOE,

        // 四向防御
        GuardUp,
        GuardDown,
        GuardLeft,
        GuardRight
    }
    public Animator animator;
    [Header("Boss State")]
    public BossPhaseState bossPhase = BossPhaseState.Normal;
    public BossActionType currentAction = BossActionType.None;

    [Header("Distance Setting")]
    public float closeRange = 3.5f;
    public float farRange = 7f;

    [Header("Weak Setting")]
    public float weakHPPercent = 0.15f;   // 低于15%进入虚弱
    public bool canEnterWeak = true;

    [Header("Action Duration")]
    public float actionDuration = 1.2f;   // 当前动作持续时间
    private float actionTimer = 0f;
    public bool isPerformingAction = false;

    [Header("Action Weights")]
    [Range(0, 100)] public int guardWeight = 40;      // 近距离时防御权重
    [Range(0, 100)] public int meleeWeight = 60;      // 近距离时近战权重

    public bool isFarAttackAction = false;
    public bool hasDoneFarAttackThisCycle = false;
    public float farAttackTriggerWidth;
    public float rangeAttackTimer;
    public bool canTriggerCloseAttack;

    [Header("Tiger Material")]
    public Material normal_mat;
    public Material attack_mat;
    public Material defense_mat;
    public SkinnedMeshRenderer tigerBody;

    public enum BossHurtType
    {
        Hurt1,
        Hurt2,
        Hurt3,
        Hurt4
    }

    [Header("Hurt")]
    [Range(0f, 1f)]
    public float hurtTriggerChance = 1f;   // 50%概率触发Hurt

    private BossHurtType currentHurtType;

    protected override void Start()
    {
        base.Start();
        enemyType = EnemyType.Boss;
        currentHP = maxHP;
        animator=GetComponent<Animator>();
    }

    protected override void Update()
    {

        if (isDead) return;
        rangeAttackTimer -= Time.deltaTime;
        // 虚弱判定
        CheckWeakState();

        // 虚弱或处决阶段不走普通逻辑
        if (bossPhase == BossPhaseState.Weak)
        {
            UpdateWeak();
            return;
        }

        if (bossPhase == BossPhaseState.Executed)
        {
            return;
        }

        // 当前有动作在播放时，不重新选动作
        if (isPerformingAction)
        {
            
            return;
        }
        else
        {
            tigerBody.material=normal_mat;
        }


        base.Update();
    }

    protected override void CheckStateTransition()
    {
        if (target == null || isHurt || isDead) return;
        if (bossPhase != BossPhaseState.Normal) return;
        if (isPerformingAction) return;   // [新增] 动作播放中不切换

        float distance = Vector3.Distance(transform.position, target.position);

        // [修改] 超出追击范围 -> Idle
        if (distance > chaseRange)
        {
            currentState = EnemyState.Idle;
        }
        // [修改] 近战距离内 -> Attack
        else if (distance <= closeRange)
        {
            currentState = EnemyState.Attack;
            animator.SetFloat("BossSpeed", 0f);
        }
        // [修改] 中距离（closeRange ~ farRange）-> Attack（执行一次远程攻击）
        else if (distance <= farRange)
        {
            if(!hasDoneFarAttackThisCycle)
                currentState = EnemyState.Attack;
        }
        // [修改] farRange 外但仍在追击范围内 -> Chase
        else
        {
            currentState = EnemyState.Chase;
        }
    }

    protected override void UpdateIdle()
    {
        StopMove();
        currentAction = BossActionType.None;
        animator.SetFloat("BossSpeed", 0f);
    }

    protected override void UpdateChase()
    {
        if (target == null) return;
        Debug.Log("追逐");

        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = moveSpeed;
            animator.SetFloat("BossSpeed",1f);

            // 还没打过远程：先追到 farRange
            if (!hasDoneFarAttackThisCycle)
                agent.stoppingDistance = farRange * 0.9f;
            // 远程打完后：继续追到 closeRange
            else
                agent.stoppingDistance = closeRange * 0.9f;

            agent.SetDestination(target.position);
            //Debug.Log("追逐");
        }

        FaceTarget();
        //base.UpdateChase();
        // animator.SetBool("IsMoving", true);
    }

    protected override void UpdateAttack()
    {
        StopMove();
        FaceTarget();
        animator.SetFloat("BossSpeed", 0f);
        if (target == null) return;
        if (isPerformingAction) return;   // 当前动作还没播完，不重复选动作

        float distance = Vector3.Distance(transform.position, target.position);

        // ===== 1. 玩家跑出 farRange：退出攻击，重新追击 =====
        if (distance > farRange)
        {
            ResetCloseAttackGate();   // [新增]
            currentState = EnemyState.Chase;
            return;
        }

        // ===== 2. 攻击冷却中：先不出手 =====
        //if (attackTimer > 0f)
        //{
        //    attackTimer -= Time.deltaTime;
        //    return;
        //}

        // ===== 3. 中距离：优先打一发远程攻击（只打一轮）=====
        // ===== [修改] 只有在 farRange 外圈的一小段范围内才触发一次远程攻击 =====
        if (distance > farRange - farAttackTriggerWidth && distance <= farRange)
        {
            if (rangeAttackTimer<=0 && !hasDoneFarAttackThisCycle)
            {
                currentAction = SelectFarRangeAction();
                isFarAttackAction = true;
                hasDoneFarAttackThisCycle = true;

                PerformAction(currentAction);
                attackTimer = attackCooldown;
                rangeAttackTimer = 10f;
                //Invoke("ChangeToChase", 2f);
                return;
            }
            //ResetCloseAttackGate();
            currentState = EnemyState.Chase;
            return;
        }

        // 3. 中间推进区：继续追近，同时恢复近战门
        //if (distance > closeRange && distance <= farRange - farAttackTriggerWidth)
        //{
        //    ResetCloseAttackGate();
        //    currentState = EnemyState.Chase;
        //    return;
        //}

        // ===== 4. 近距离：近战/四向防御 =====
        if (distance <= closeRange && canTriggerCloseAttack)
        {
            currentAction = SelectCloseRangeAction();   // 7选1：3攻击 + 4防御
            isFarAttackAction = false;
            hasDoneFarAttackThisCycle = false;
            //canTriggerCloseAttack = false;
            LockCloseAttackGate();
            PerformAction(currentAction);
            //attackTimer = attackCooldown;
            return;
        }
    }

    public void TriggerNextAttack()
    {
        CancelInvoke(nameof(EnableAttack));
        Invoke(nameof(EnableAttack), 0.5f);
    }

    private void EnableAttack()
    {
        if (isDead) return;
        if (bossPhase != BossPhaseState.Normal) return;
        if (isHurt) return;

        canTriggerCloseAttack = true;
        isPerformingAction = false;
    }

    private void ResetCloseAttackGate()
    {
        CancelInvoke(nameof(EnableAttack));
        canTriggerCloseAttack = true;
    }

    private void LockCloseAttackGate()
    {
        CancelInvoke(nameof(EnableAttack));
        canTriggerCloseAttack = false;
    }
    public void ChangeToChase()
    {
        currentState = EnemyState.Chase;
        isPerformingAction = false;
    }

    protected override void UpdateHurt()
    {
        StopMove();
        FaceTarget();
        animator.SetFloat("BossSpeed", 0f);
        // 这里先简化：短暂受击后恢复
        isHurt = false;

        if (!isDead && bossPhase == BossPhaseState.Normal)
        {
            currentState = EnemyState.Idle;
        }
    }

    public void EndHurt()
    {
        if (isDead) return;
        if (bossPhase != BossPhaseState.Normal) return;

        isHurt = false;

        if (target == null)
        {
            currentState = EnemyState.Idle;
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);
        isPerformingAction = false;
        if (distance <= closeRange)
        {
            ResetCloseAttackGate();   // [新增]
            currentState = EnemyState.Attack;
        }
        else if (distance <= chaseRange)
        {
            ResetCloseAttackGate();   // [新增] 追击时也把门恢复，避免以后再进近战时卡死
            currentState = EnemyState.Chase;
        }
        else
        {
            ResetCloseAttackGate();
            currentState = EnemyState.Idle;
        }
    }

    //private void SelectAndPerformAction()
    //{
    //    if (target == null) return;

    //    float distance = Vector3.Distance(transform.position, target.position);

    //    if (distance <= closeRange)
    //    {
    //        currentAction = SelectCloseRangeAction();
    //    }
    //    else
    //    {
    //        currentAction = SelectFarRangeAction();
    //    }

    //    PerformAction(currentAction);
    //}

    private BossActionType SelectCloseRangeAction()
    {
        // 近距离：四向防御 + 三种近战攻击，7选1
        int totalWeight = guardWeight + meleeWeight;
        int roll = Random.Range(0, totalWeight);

        if (roll < guardWeight)
        {
            tigerBody.material = defense_mat;
            int guardRoll = Random.Range(0, 4);
            switch (guardRoll)
            {
                case 0: return BossActionType.GuardUp;
                case 1: return BossActionType.GuardDown;
                case 2: return BossActionType.GuardLeft;
                default: return BossActionType.GuardRight;

            }
        }
        else
        {
            tigerBody.material = attack_mat;
            int atkRoll = Random.Range(0, 3);
            switch (atkRoll)
            {
                case 0: return BossActionType.LeftClaw;
                case 1: return BossActionType.RightClaw;
                default: return BossActionType.CrossClaw;
            }
        }
    }

    private BossActionType SelectFarRangeAction()
    {
        // 玩家走远：怒吼 or 砸地AOE
        tigerBody.material = attack_mat;
        int roll = Random.Range(0, 2);
        return roll == 0 ? BossActionType.Roar : BossActionType.SmashAOE;

    }

    private void PerformAction(BossActionType action)
    {
        isPerformingAction = true;

        switch (action)
        {
            case BossActionType.LeftClaw:
                actionDuration = 1.0f;
                Debug.Log("Boss 使用：左爪攻击");
                animator.SetTrigger("LeftAttack");
                break;

            case BossActionType.RightClaw:
                actionDuration = 1.0f;
                Debug.Log("Boss 使用：右爪攻击");
                animator.SetTrigger("RightAttack");
                break;

            case BossActionType.CrossClaw:
                actionDuration = 1.2f;
                Debug.Log("Boss 使用：交叉攻击");
                animator.SetTrigger("CrossAttack");
                break;

            case BossActionType.GuardUp:
                actionDuration = 0.8f;
                Debug.Log("Boss 使用：上防御");
                animator.SetTrigger("GuardUp");
                break;

            case BossActionType.GuardDown:
                actionDuration = 0.8f;
                Debug.Log("Boss 使用：下防御");
                animator.SetTrigger("GuardDown");
                break;

            case BossActionType.GuardLeft:
                actionDuration = 0.8f;
                Debug.Log("Boss 使用：左防御");
                animator.SetTrigger("GuardLeft");
                break;

            case BossActionType.GuardRight:
                actionDuration = 0.8f;
                Debug.Log("Boss 使用：右防御");
                animator.SetTrigger("GuardRight");
                break;

            case BossActionType.Roar:
                actionDuration = 1.8f;
                Debug.Log("Boss 使用：怒吼攻击");
                animator.SetTrigger("Roar");
                break;

            case BossActionType.SmashAOE:
                actionDuration = 1.6f;
                Debug.Log("Boss 使用：砸地AOE");
                animator.SetTrigger("Smash");
                break;
        }

        actionTimer = actionDuration;
    }

    private void EndCurrentAction()
    {
        isPerformingAction = false;
        currentAction = BossActionType.None;

        if (!isDead && bossPhase == BossPhaseState.Normal)
        {
            currentState = EnemyState.Idle;
        }
    }

    private void CheckWeakState()
    {
        if (!canEnterWeak) return;
        if (bossPhase != BossPhaseState.Normal) return;

        float hpRatio = currentHP / maxHP;
        if (hpRatio <= weakHPPercent)
        {
            EnterWeakState();
        }
    }

    private void EnterWeakState()
    {
        bossPhase = BossPhaseState.Weak;
        currentState = EnemyState.Idle;
        isPerformingAction = false;
        currentAction = BossActionType.None;
        StopMove();

        Debug.Log("Boss 进入虚弱待处决状态");
        // animator.SetTrigger("Weak");
    }

    private void UpdateWeak()
    {
        StopMove();
        FaceTarget();
        // 保持虚弱动作，不主动攻击
    }

    public void ExecuteBoss()
    {
        if (bossPhase != BossPhaseState.Weak) return;

        bossPhase = BossPhaseState.Executed;
        StopMove();

        Debug.Log("Boss 被处决");
        // animator.SetTrigger("ExecuteDeath");

        Die();
    }

    public override void TakeDamage(float damage)
    {
        if (isDead) return;
        if (bossPhase == BossPhaseState.Executed) return;
        hpBar.TakeDamage(damage);
        currentHP -= damage;

        if (currentHP <= 0f)
        {
            // 如果你希望必须先虚弱再处决，就不要直接死
            if (canEnterWeak && bossPhase != BossPhaseState.Weak)
            {
                currentHP = 1f;
                EnterWeakState();
                return;
            }

            Die();
            return;
        }

        // 虚弱状态下不进普通受击
        if (bossPhase == BossPhaseState.Weak) return;

        if (Random.value <= hurtTriggerChance)
        {
            isHurt = true;
            currentState = EnemyState.Hurt;
            OnHurt();
        }
    }

    protected override void OnHurt()
    {
        StopMove();
        isPerformingAction = false;
        currentAction = BossActionType.None;
        isFarAttackAction = false;

        CancelInvoke(nameof(EnableAttack));   // [新增] 清掉旧的延迟攻击恢复
        canTriggerCloseAttack = true;         // [新增] 受击后不要把近战锁死
        isPerformingAction = true;

        int hurtIndex = Random.Range(0, 4);
        switch (hurtIndex)
        {
            case 0: currentHurtType = BossHurtType.Hurt1; break;
            case 1: currentHurtType = BossHurtType.Hurt2; break;
            case 2: currentHurtType = BossHurtType.Hurt3; break;
            default: currentHurtType = BossHurtType.Hurt4; break;
        }
        tigerBody.material = normal_mat;
        Debug.Log("Boss Hurt: " + currentHurtType);
        animator.SetTrigger(currentHurtType.ToString());

        //Debug.Log("Boss 受击");
        // animator.SetTrigger("Hurt");
    }

    protected override void Die()
    {
        isDead = true;
        bossPhase = BossPhaseState.Dead;
        currentState = EnemyState.Dead;
        StopMove();

        Debug.Log("Boss 死亡");
        OnDie();
    }



    protected override void OnDie()
    {
        animator.SetTrigger("Die");
        Destroy(gameObject, 8f);
    }

    protected override void Attack()
    {
        // Boss 不直接使用 EnemyBase 的抽象 Attack 逻辑
        // 实际逻辑已经放进 UpdateAttack -> SelectAndPerformAction
    }
}
