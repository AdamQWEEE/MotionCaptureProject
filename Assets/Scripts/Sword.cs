using StarterAssets;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public ThirdPersonController playerController;
    public Transform stabWeaponTransform;
    public Transform originalTransform;
    public float knockCoolTime;
    public int hitEnemyNum;
    

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
            
            if (playerController.canTakeDamage)
            {
                EnemyBase enemy = other.GetComponent<EnemyBase>();
               
                enemy.TakeDamage(10f);
                Debug.Log(enemy.name);
                
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
