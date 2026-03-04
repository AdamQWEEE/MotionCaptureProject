using StarterAssets;
using UnityEngine;

public class Boomerang : MonoBehaviour
{
    public float forwardSpeed = 12f;      // 外圈速度
    public float returnSpeed = 14f;      // 回程速度
    public float maxDistance = 12f;      // 飞多远开始回头
    public float rotateSpeed = 720f;     // 自身旋转速度
    public bool useHomingOnOut = false;  // 是否在外圈阶段弱追踪玩家
    public float homingTurnSpeed = 3f;    // 追踪转向速度
    public int damage = 10;

    public LayerMask hitLayers;           // Player 所在层，用于简单筛选

    Transform _owner;
    Transform _player;
    Vector3 _startPos;
    Vector3 _moveDir;

    enum Phase { Outbound, Returning }
    Phase _phase = Phase.Outbound;

    bool _hasHitPlayerOnOut;
    bool _hasHitPlayerOnReturn;

    public void Init(Transform owner, Vector3 initialDir, Transform player, bool useHomingOnOut)
    {
        _owner = owner;
        _player = player;
        _startPos = transform.position;
        _moveDir = initialDir.normalized;
        this.useHomingOnOut = useHomingOnOut;
        _phase = Phase.Outbound;
    }

    void Update()
    {
        // 自转特效
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.Self);

        switch (_phase)
        {
            case Phase.Outbound:
                UpdateOutbound();
                break;
            case Phase.Returning:
                UpdateReturning();
                break;
        }
    }

    void UpdateOutbound()
    {
        // 可选：弱追踪玩家
        if (useHomingOnOut && _player != null)
        {
            Vector3 desired = (_player.position + Vector3.up * 1.0f - transform.position).normalized;
            _moveDir = Vector3.Slerp(_moveDir, desired, homingTurnSpeed * Time.deltaTime).normalized;
        }

        transform.position += _moveDir * forwardSpeed * Time.deltaTime;

        float dist = Vector3.Distance(_startPos, transform.position);
        if (dist >= maxDistance)
        {
            StartReturn();
        }
    }

    void StartReturn()
    {
        _phase = Phase.Returning;
    }

    void UpdateReturning()
    {
        if (_owner == null)
        {
            Destroy(gameObject);
            return;
        }

        // 回到敌人手附近（可以微微偏上）
        Vector3 targetPos = _owner.position + _owner.forward * 0.5f + Vector3.up * 1.2f;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            returnSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPos) < 0.2f)
        {
            Destroy(gameObject);   // 回到手里，可改成关掉表现然后通知敌人“已归位”
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 只处理 Player
        if (!other.CompareTag("Player")) return;

        // 外圈只打一次，回程只打一次（避免一帧多次判定）
        if (_phase == Phase.Outbound && _hasHitPlayerOnOut) return;
        if (_phase == Phase.Returning && _hasHitPlayerOnReturn) return;

        // 这里调用玩家受伤逻辑
        var player = other.GetComponent<ThirdPersonController>();
        if (!player.isDead)
        {
            player.playerState.TakeDamage(15f);
            AudioManager.Instance.PlayHitPlayer();

        }

        if (_phase == Phase.Outbound) _hasHitPlayerOnOut = true;
        else _hasHitPlayerOnReturn = true;
    }
}
