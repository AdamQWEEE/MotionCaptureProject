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
    private bool isPerformingAction = false;

    [Header("Action Weights")]
    [Range(0, 100)] public int guardWeight = 40;      // 近距离时防御权重
    [Range(0, 100)] public int meleeWeight = 60;      // 近距离时近战权重

    protected override void Start()
    {
        base.Start();
        enemyType = EnemyType.Boss;
        currentHP = maxHP;
    }

    protected override void Update()
    {
        if (isDead) return;

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
            actionTimer -= Time.deltaTime;

            if (actionTimer <= 0f)
            {
                EndCurrentAction();
            }

            return;
        }

        base.Update();
    }

    protected override void CheckStateTransition()
    {
        if (target == null || isHurt || isDead) return;
        if (bossPhase != BossPhaseState.Normal) return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= closeRange)
        {
            currentState = EnemyState.Attack;
        }
        else if (distance <= chaseRange)
        {
            currentState = EnemyState.Chase;
        }
        else
        {
            currentState = EnemyState.Idle;
        }
    }

    protected override void UpdateIdle()
    {
        StopMove();
        currentAction = BossActionType.None;
        // animator.SetBool("IsMoving", false);
    }

    protected override void UpdateChase()
    {
        if (target == null) return;

        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = moveSpeed;
            agent.stoppingDistance = 2f;
            agent.SetDestination(target.position);
            Debug.Log("追逐玩家");
        }

        FaceTarget();
        //base.UpdateChase();
        // animator.SetBool("IsMoving", true);
    }

    protected override void UpdateAttack()
    {
        StopMove();
        FaceTarget();

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
            return;
        }

        SelectAndPerformAction();
        attackTimer = attackCooldown;
    }

    protected override void UpdateHurt()
    {
        StopMove();

        // 这里先简化：短暂受击后恢复
        isHurt = false;

        if (!isDead && bossPhase == BossPhaseState.Normal)
        {
            currentState = EnemyState.Idle;
        }
    }

    private void SelectAndPerformAction()
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= closeRange)
        {
            currentAction = SelectCloseRangeAction();
        }
        else
        {
            currentAction = SelectFarRangeAction();
        }

        PerformAction(currentAction);
    }

    private BossActionType SelectCloseRangeAction()
    {
        // 近距离：四向防御 + 三种近战攻击，7选1
        int totalWeight = guardWeight + meleeWeight;
        int roll = Random.Range(0, totalWeight);

        if (roll < guardWeight)
        {
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
                // animator.SetTrigger("LeftClaw");
                break;

            case BossActionType.RightClaw:
                actionDuration = 1.0f;
                Debug.Log("Boss 使用：右爪攻击");
                // animator.SetTrigger("RightClaw");
                break;

            case BossActionType.CrossClaw:
                actionDuration = 1.2f;
                Debug.Log("Boss 使用：交叉攻击");
                // animator.SetTrigger("CrossClaw");
                break;

            case BossActionType.GuardUp:
                actionDuration = 0.8f;
                Debug.Log("Boss 使用：上防御");
                // animator.SetTrigger("GuardUp");
                break;

            case BossActionType.GuardDown:
                actionDuration = 0.8f;
                Debug.Log("Boss 使用：下防御");
                // animator.SetTrigger("GuardDown");
                break;

            case BossActionType.GuardLeft:
                actionDuration = 0.8f;
                Debug.Log("Boss 使用：左防御");
                // animator.SetTrigger("GuardLeft");
                break;

            case BossActionType.GuardRight:
                actionDuration = 0.8f;
                Debug.Log("Boss 使用：右防御");
                // animator.SetTrigger("GuardRight");
                break;

            case BossActionType.Roar:
                actionDuration = 1.8f;
                Debug.Log("Boss 使用：怒吼攻击");
                // animator.SetTrigger("Roar");
                break;

            case BossActionType.SmashAOE:
                actionDuration = 1.6f;
                Debug.Log("Boss 使用：砸地AOE");
                // animator.SetTrigger("SmashAOE");
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

        isHurt = true;
        currentState = EnemyState.Hurt;
        OnHurt();
    }

    protected override void OnHurt()
    {
        StopMove();
        isPerformingAction = false;
        currentAction = BossActionType.None;

        Debug.Log("Boss 受击");
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
        // animator.SetTrigger("Dead");
        Destroy(gameObject, 3f);
    }

    protected override void Attack()
    {
        // Boss 不直接使用 EnemyBase 的抽象 Attack 逻辑
        // 实际逻辑已经放进 UpdateAttack -> SelectAndPerformAction
    }
}
