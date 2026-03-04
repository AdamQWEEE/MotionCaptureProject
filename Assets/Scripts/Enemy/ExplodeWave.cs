using StarterAssets;
using UnityEngine;

public class ExplodeWave : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ThirdPersonController player=other.GetComponent<ThirdPersonController>();
            if (player.isDead) return;
            player.GetHeavyAttack();
            player.playerState.TakeDamage(30f);
            GetComponent<Collider>().enabled = false;
            //Destroy(gameObject);
        }
    }
}
