using StarterAssets;
using UnityEngine;

public class AxePick : MonoBehaviour
{
    public bool canPick;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (canPick &&Input.GetKeyDown(KeyCode.E)) 
        {
            ThirdPersonController.Instance.EquipAxe();
            Tutorial.Instance.HideTip();
            Destroy(gameObject);
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canPick = true;
            Tutorial.Instance.ShowInteractTip();
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")){
            canPick = false;
            Tutorial.Instance.HideTip();
        }
    }
}
