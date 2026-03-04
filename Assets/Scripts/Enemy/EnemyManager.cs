using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    // 当前场景中激活的敌人
    private static readonly List<EnemyBase> _enemies = new List<EnemyBase>();

    // 仅供调试查看
    public static IReadOnlyList<EnemyBase> Enemies => _enemies;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 如果你希望跨场景存在，可以加上：
        // DontDestroyOnLoad(gameObject);
    }

    #region 注册 / 注销（由 EnemyBase 调用）

    public static void Register(EnemyBase enemy)
    {
        if (enemy == null) return;
        if (_enemies.Contains(enemy)) return;

        _enemies.Add(enemy);
    }

    public static void Unregister(EnemyBase enemy)
    {
        if (enemy == null) return;
        _enemies.Remove(enemy);
    }

    #endregion

    public EnemyBase GetNearestEnemy(Vector3 fromPos, float maxDistance)
    {
        EnemyBase result = null;
        float bestDistSqr = maxDistance * maxDistance;

        foreach (var enemy in _enemies)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            // ★ 直接用敌人本体的位置
            float distSqr = (enemy.transform.position - fromPos).sqrMagnitude;

            if (distSqr < bestDistSqr)
            {
                bestDistSqr = distSqr;
                result = enemy;
            }
        }

        return result;
    }




    /// <summary>
    /// 在已锁定目标基础上，切换到“屏幕左/右”下一个目标。
    /// （简单版本：只看角度，忽略遮挡）
    /// </summary>


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 在 Scene 里看当前有多少敌人
        Gizmos.color = Color.red;
        foreach (var enemy in _enemies)
        {
            if (enemy == null) continue;
            Gizmos.DrawWireSphere(enemy.transform.position, 0.3f);
        }
    }
#endif

}
