using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class FireBall : MonoBehaviour
{

    public float lifeTime = 5f;          // 最长存在时间
    public float rotateSpeed = 720f;     // 模型转向速度（可选）

    private Rigidbody _rb;
    private bool _launched;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;
    }

    public void Launch(Vector3 initialVelocity)
    {
        _launched = true;
        _rb.linearVelocity = initialVelocity;
        // 朝向速度方向
        if (initialVelocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(initialVelocity.normalized);
        }

        // 自动销毁
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        if (!_launched) return;

        // 让火球朝当前速度方向慢慢对齐（视觉上更自然）
        Vector3 v = _rb.linearVelocity;
        if (v.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(v.normalized);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotateSpeed * Time.fixedDeltaTime
            );
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        
        // 在这里处理伤害、粒子特效、爆炸等
        // 比如：
        //   EnemyBase enemy = other.collider.GetComponentInParent<EnemyBase>();
        //   if (enemy != null) enemy.TakeDamage(damageAmount);
        // 然后销毁自己：
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            other.gameObject.GetComponent<EnemyBase>().TakeDamage(15);
            AudioManager.Instance.PlayFireBallHit();
            Destroy(gameObject);
        }
    }
}
