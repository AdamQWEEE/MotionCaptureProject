using StarterAssets;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public ThirdPersonController playerController;
    public Transform stabWeaponTransform;
    public Transform originalTransform;
    public float knockCoolTime;
    public int hitEnemyNum;
    public bool hitEnemy = false;


    private void Awake()
    {
        playerController = GetComponentInParent<ThirdPersonController>();
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        knockCoolTime-=Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("HitEnemy"+hitEnemy);
            if (playerController.canTakeDamage)
            {
                EnemyBase enemy = other.GetComponent<EnemyBase>();
               
                enemy.TakeDamage(10f);
                Debug.Log(enemy.name);
                hitEnemy = true;//标记击中敌人，在攻击动画事件中重置
                playerController.playerState.stanceValue = Mathf.Min(playerController.playerState.stanceValue + 0.2f, 1f);//打中敌人加体力
                //AudioManager.Instance.PlayHit();
                //hitEnemyNum++;
                playerController.canTakeDamage = false;
            }
        }
    }

    public void SetStabTransform()
    {
        CancelInvoke();
        transform.localPosition = stabWeaponTransform.localPosition;
        transform.localRotation = stabWeaponTransform.localRotation;
        Invoke("ResetSwordTransform", 2f);
    }

    public void ResetSwordTransform()
    {
        transform.localPosition=originalTransform.localPosition;
        transform.localRotation = originalTransform.localRotation;
    }
}
