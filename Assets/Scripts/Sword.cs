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
    public SwordDirManager dirManager;


    private void Awake()
    {
        playerController = GetComponentInParent<ThirdPersonController>();
        dirManager = GetComponent<SwordDirManager>();
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
                //Debug.Log("碰到对象: " + other.name);
                //Debug.Log("敌人防御ID: " + enemy.defenceDirID);
                //Debug.Log("玩家攻击方向ID" + playerController.attackDirID);

                
                Debug.Log(enemy.name);
                if (playerController.attackDirID != enemy.defenceDirID)
                {
                    enemy.failDefence = true;
                    playerController.failAttack = false;
                    AudioManager.Instance.PlayHit();
                    if (!playerController.CheckSkillAttack())
                    {

                        enemy.TakeDamage(20f);
                        
                    }
                    else
                    {
                        enemy.TakeDamage(45f);
                        
                    }
                    
                }//当玩家攻击ID不等于敌人防御方向时，敌人防御失败,
                else
                {
                    enemy.failDefence = false;
                    playerController.failAttack = true;
                    //AudioManager.Instance.PlayCounter();
                    playerController.GetComponent<SwordVFX>().ShowVFX1();
                    Debug.Log("玩家攻击被打断"+playerController.failAttack);
                }

                hitEnemy = true;//标记击中敌人，在攻击动画事件中重置
                if (!playerController.isTired&&!playerController.failAttack){
                    playerController.playerState.stanceValue = Mathf.Min(playerController.playerState.stanceValue + 0.2f, 1f);//非疲劳状态打中敌人加体力
                }
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
