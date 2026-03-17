using StarterAssets;
using System;
using System.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;




public abstract class EnemyBase : MonoBehaviour
{




    public enum EnemyType
    {
        Melee,
        Ranged,
        Boss
    }

    public enum EnemyState
    {
        Idle,       // 站桩
        Patrol,     // 巡逻
        Chase,      // 追击
        Attack,     // 攻击
        Hurt,       // 受击
        Dead        // 死亡
    }

    [Header("Base Info")]
    public EnemyType enemyType;
    public EnemyState currentState;

    [Header("Common Stats")]
    public float maxHP = 100f;
    protected float currentHP;
    public WorldSpaceHealthBar hpBar;

    [Header("Move")]
    public bool usePatrol = false;     // false=站桩, true=巡逻
    public float moveSpeed = 2f;
    public float chaseRange = 8f;
    public float attackRange = 2f;

    [Header("Combat")]
    public float attackCooldown = 1.5f;
    protected float attackTimer;

    [Header("Target")]
    public Transform target;

    protected bool isDead;
    protected bool isHurt;

    public NavMeshAgent agent;


    protected virtual void OnEnable()
    {
        EnemyManager.Register(this);
        //state = patrolPoints != null && patrolPoints.Length > 0 ? EnemyState.Patrol : EnemyState.Idle;
    }

    protected virtual void OnDisable()
    {
        // 注意：OnDisable 也会在场景卸载/对象销毁时调用
        EnemyManager.Unregister(this);
    }
    protected virtual void Start()
    {
        currentHP = maxHP;
        currentState = usePatrol ? EnemyState.Patrol : EnemyState.Idle;
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange;
            agent.autoBraking = true;
        }
    }

    protected virtual void Update()
    {
        if (isDead) return;

        attackTimer -= Time.deltaTime;

        switch (currentState)
        {
            case EnemyState.Idle:
                UpdateIdle();
                break;
            case EnemyState.Patrol:
                UpdatePatrol();
                break;
            case EnemyState.Chase:
                UpdateChase();
                break;
            case EnemyState.Attack:
                UpdateAttack();
                break;
            case EnemyState.Hurt:
                UpdateHurt();
                break;
        }

        CheckStateTransition();
    }

    protected virtual void CheckStateTransition()
    {
        if (target == null || isHurt || isDead) return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackRange)
        {
            currentState = EnemyState.Attack;
        }
        else if (distance <= chaseRange)
        {
            currentState = EnemyState.Chase;
        }
        else
        {
            currentState = usePatrol ? EnemyState.Patrol : EnemyState.Idle;
        }
    }

    protected virtual void UpdateIdle()
    {
        StopMove();
    }

    protected virtual void UpdatePatrol()
    {
        // 子类可重写巡逻
    }

    protected virtual void UpdateChase()
    {
        //if (target == null) return;

        //if (agent != null)
        //{
        //    agent.isStopped = false;
        //    agent.speed = moveSpeed;
        //    agent.stoppingDistance = attackRange;
        //    agent.SetDestination(target.position);
            
        //}
        //else
        //{
        //    // 可选兜底：没Agent时仍可用transform移动
        //}
    }

    protected virtual void UpdateAttack()
    {
        StopMove();
        FaceTarget();

        if (attackTimer <= 0f)
        {
            Attack();
            attackTimer = attackCooldown;
        }
    }

    protected virtual void UpdateHurt()
    {
        StopMove();

        // 默认受击状态不做复杂处理
        isHurt = false;
    }

    protected void FaceTarget()
    {
        if (target == null) return;

        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.forward = dir;
    }

    protected void StopMove()
    {
        if (agent == null) return;
        agent.isStopped = true;
        agent.ResetPath();
    }

    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHP -= damage;
        hpBar.TakeDamage(10f);
        if (currentHP <= 0)
        {
            Die();
            return;
        }

        isHurt = true;
        currentState = EnemyState.Hurt;
        OnHurt();
    }

    protected virtual void OnHurt()
    {
        // 播放受击动画、硬直等
    }

    protected virtual void Die()
    {
        isDead = true;
        currentState = EnemyState.Dead;
        StopMove();
        OnDie();
    }

    protected virtual void OnDie()
    {
        // 播放死亡动画、关闭碰撞、掉落等
        Destroy(gameObject, 2f);
    }

    protected abstract void Attack();




}
