using StarterAssets;
using UnityEngine;

public class VolumeBall : MonoBehaviour
{
    public EnemyBase enemy;
    public ThirdPersonController player;
    public Material volueball_bright;
    public Material volueball_dark;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemy=GameObject.Find("Boss").GetComponent<EnemyBase>();
        player=ThirdPersonController.Instance;
        if (enemy.isDark)
        {
            GetComponent<MeshRenderer>().material = volueball_dark;
        }
        else
        {
            GetComponent<MeshRenderer>().material = volueball_bright;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale += new Vector3(1, 1, 1) * Time.deltaTime*10f;

        float disToPlayer= (transform.position - player.transform.position).magnitude - transform.GetComponent<SphereCollider>().radius * transform.localScale.x;
        Debug.Log(disToPlayer);
        if (disToPlayer < 1.5f)
        {
            if (!Tutorial.Instance.isFinishRollTip_volumeBall)
            {
                Tutorial.Instance.ShowRollTip_VolumeBall();
                //Time.timeScale = 0.05f;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //if (!Tutorial.Instance.isFinishSwapTip)
            //{               
            //    Tutorial.Instance.ShowSwapTip();
            //    Time.timeScale = 0.05f;
            //}
            Tutorial.Instance.HideTip();
            Time.timeScale = 1.0f;
            player.DelayShowSwapTip();

            if (!player.isRolling)
            {
                player.GetHeavyAttack();
            }
            if (enemy.isDark)
            {
                VolumeController.Instance.ChangeDarkVolume();
                Destroy(gameObject);
            }
            else
            {

                VolumeController.Instance.ChangeLightVolume();
                Destroy(gameObject);

            }
            AudioManager.Instance.PlayGetImpulse();
        }
    }

}
