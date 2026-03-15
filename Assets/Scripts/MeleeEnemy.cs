using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    [Header("Melee")]
    public int comboCount = 3;
    public float comboResetTime = 2f;

    private int comboIndex = 0;
    private float comboTimer = 0f;

    [Header("Patrol (Waypoints)")]
    public Transform[] patrolPoints;
    public float waitAtPointTime = 1.0f;
    int patrolIndex = 0;
    float waitTimer = 0f;

    protected override void Start()
    {
        base.Start();
        enemyType = EnemyType.Melee;
        if (usePatrol && (agent == null || patrolPoints == null || patrolPoints.Length == 0))
            usePatrol = false;
    }

    protected override void Update()
    {
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
    }

    protected override void Attack()
    {
        comboIndex++;
        if (comboIndex > comboCount)
            comboIndex = 1;

        comboTimer = comboResetTime;

        Debug.Log($"{name} 近战第 {comboIndex} 段攻击");

        // 这里可以接Animator参数
        // animator.SetTrigger("Attack" + comboIndex);

        // 实际伤害检测可以用 OverlapSphere / 动画事件
    }

    protected override void OnHurt()
    {
        base.OnHurt();
        Debug.Log($"{name} 近战怪受击");
    }

    protected override void OnDie()
    {
        Debug.Log($"{name} 近战怪死亡");
        base.OnDie();
    }
}
