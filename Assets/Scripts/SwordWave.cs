using StarterAssets;
using UnityEngine;

public class SwordWave : MonoBehaviour
{
    public float speed = 8f;
    public float lifeTime = 1f;
    private bool canEmit;
    private Vector3 _dir;
    private Transform _owner;
    private ParticleSystem _ps;

    public void Init(Transform owner, Vector3 dir)
    {
        _owner = owner;
        _dir = dir.normalized;

        _ps = GetComponentInChildren<ParticleSystem>();
        if (_ps != null) _ps.Play();

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        
        if(canEmit)
        //transform.position += _dir * speed * Time.deltaTime;
            transform.position += _dir * speed *3f* Time.deltaTime;
    }

    private void OnDestroy()
    {
        // 这里可以实例化单独的命中特效
        // Instantiate(hitVfxPrefab, transform.position, Quaternion.identity);
    }

    public void EmitWave()
    {
        canEmit = true;
        Destroy(gameObject, lifeTime);
        _dir = ThirdPersonController.Instance.transform.forward;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyBase enemy = other.gameObject.GetComponent<EnemyBase>();
            if (enemy.state == EnemyBase.EnemyState.ChangeYinYang) return;

            
                          
            enemy.WaveDamage(8);
            enemy.FallBack();
            GetComponent<BoxCollider>().enabled = false;
            
            
            
            enemy.WaveDamage(2);
            
            AudioManager.Instance.PlayHit();
        }
    }
}
