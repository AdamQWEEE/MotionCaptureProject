using UnityEngine;
using static EnemyBase;

public class BossEnemy : EnemyBase
{
    public enum BossAttackMode
    {
        Smash,      // 近战重击
        Dash,       // 冲刺
        Projectile, // 发射弹幕
        Summon      // 召唤
    }

    [Header("Boss")]
    public BossAttackMode currentAttackMode;

    protected override void Start()
    {
        base.Start();
        enemyType = EnemyType.Boss;
        maxHP = 500f;
        currentHP = maxHP;
        attackRange = 8f;
    }

    protected override void Attack()
    {
        ChooseAttackMode();

        switch (currentAttackMode)
        {
            case BossAttackMode.Smash:
                DoSmash();
                break;
            case BossAttackMode.Dash:
                DoDash();
                break;
            case BossAttackMode.Projectile:
                DoProjectile();
                break;
            case BossAttackMode.Summon:
                DoSummon();
                break;
        }
    }

    private void ChooseAttackMode()
    {
        float distance = target == null ? 999f : Vector3.Distance(transform.position, target.position);

        if (distance < 3f)
        {
            currentAttackMode = Random.value > 0.5f ? BossAttackMode.Smash : BossAttackMode.Dash;
        }
        else
        {
            currentAttackMode = Random.value > 0.5f ? BossAttackMode.Projectile : BossAttackMode.Summon;
        }
    }

    private void DoSmash()
    {
        Debug.Log($"{name} Boss 使用 重击");
    }

    private void DoDash()
    {
        Debug.Log($"{name} Boss 使用 冲刺");
    }

    private void DoProjectile()
    {
        Debug.Log($"{name} Boss 使用 远程弹幕");
    }

    private void DoSummon()
    {
        Debug.Log($"{name} Boss 使用 召唤");
    }

    protected override void OnHurt()
    {
        base.OnHurt();
        Debug.Log($"{name} Boss受击");
    }

    protected override void OnDie()
    {
        Debug.Log($"{name} Boss死亡");
        base.OnDie();
    }
}
