using UnityEngine;

public class RangedEnemy : EnemyBase
{
    [Header("Ranged")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;

    protected override void Start()
    {
        base.Start();
        enemyType = EnemyType.Ranged;
        attackRange = 6f; // 远程怪攻击范围通常更大
    }

    protected override void Attack()
    {
        Debug.Log($"{name} 远程投掷攻击");

        if (projectilePrefab == null || firePoint == null || target == null) return;

        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Vector3 dir = (target.position - firePoint.position).normalized;

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = dir * projectileSpeed;
        }

        // 动画可接：
        // animator.SetTrigger("Throw");
    }

    protected override void OnHurt()
    {
        base.OnHurt();
        Debug.Log($"{name} 远程怪受击");
    }

    protected override void OnDie()
    {
        Debug.Log($"{name} 远程怪死亡");
        base.OnDie();
    }

    public void CastFireBall()
    {
        Debug.Log("投掷");
    }
}
