using StarterAssets;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public EnemyBase enemy;
    public Transform weaponEnd;
    public Transform explodePrefab;
    private Transform explodeItem;
    private bool isExplode;
    private bool isCounterHeavy;
    AnimatorStateInfo stateInfo;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        if (!isExplode && stateInfo.IsName("Jumpattack"))
        {
            if (weaponEnd.transform.position.y < 0.1f && !isCounterHeavy)
            {
                explodeItem= Instantiate(explodePrefab);
                AudioManager.Instance.PlayAxeExplode();
                explodeItem.position= weaponEnd.transform.position;
                Destroy(explodeItem.gameObject, 1f);
                
                isExplode=true;
                Invoke("ResetExplode", 3f);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (enemy.canApplyDamage)
            {
                ThirdPersonController player=other.GetComponent<ThirdPersonController>();
                if (player.isDead) return;

                if (player.isCounter|| player.isCounterReward)
                {
                    
                    player.ShowCounterEffect();
                    if (player.isDark != enemy.isDark)
                        enemy.AddStance(25);
                    if (stateInfo.IsName("Jumpattack"))
                    {
                        isCounterHeavy = true;
                        
                    }
                    player.isCounterReward=false;
                    AudioManager.Instance.PlayCounter();
                    //player.isCounter=false;
                    return;
                }
                else
                {
                    if (player.isRollReward)
                    {
                        player.isRollReward=false;
                        return;
                    }
                    player.playerState.TakeDamage(10);
                    if (Random.Range(0, 100) > 50)
                    {
                        player.GetHit();
                    }
                    Debug.Log("´òµ½Íæ¼Ò");
                    //enemy.HideAllTip();
                    enemy.canApplyDamage = false;
                    if (stateInfo.IsName("Jumpattack"))
                    {
                        isCounterHeavy = false;
                    }
                    AudioManager.Instance.PlayHitPlayer();

                }


                
            }
        }
    }

    private void ResetExplode()
    {
        isExplode=false;
    }
}
