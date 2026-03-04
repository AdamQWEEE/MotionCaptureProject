using PixelCrushers.DialogueSystem;
using StarterAssets;
using UnityEngine;


public class Divinity : MonoBehaviour
{
    public bool canTalk;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (canTalk && Input.GetKeyDown(KeyCode.E))
        {
            //ThirdPersonController.Instance.EquipAxe();
            Tutorial.Instance.HideTip();
            GetComponent<DialogueSystemTrigger>().enabled = true;
            ThirdPersonController.Instance.LockCameraPosition = true;
            ThirdPersonController.Instance.GetComponent<StarterAssetsInputs>().cursorLocked = true;
            //Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canTalk = true;
            Tutorial.Instance.ShowInteractTip();
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canTalk = false;
            Tutorial.Instance.HideTip();
        }
    }

    public void Disappear()
    {
        Destroy(GameObject.Find("Divinity"),4f);
        Invoke("UnlockCamera", 4f);
    }

    private void UnlockCamera()
    {
        ThirdPersonController.Instance.LockCameraPosition = false;
        ThirdPersonController.Instance.GetComponent<StarterAssetsInputs>().cursorLocked = false;
    }
}
