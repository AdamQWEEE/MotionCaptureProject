using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class SwitchSwordUI : Singleton<SwitchSwordUI>
{
    public GameObject switchDarkBtn;
    public GameObject switchLightBtn;
    public GameObject bg_switchDarkBtn;
    public GameObject bg_switchLightBtn;
    public TextMeshProUGUI countdown_switchdark;
    public TextMeshProUGUI countdown_switchlight;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (ThirdPersonController.Instance.switchCoolDown >= 0)
        {
            countdown_switchdark.text = Mathf.Ceil(ThirdPersonController.Instance.switchCoolDown).ToString();
            countdown_switchlight.text=Mathf.Ceil(ThirdPersonController.Instance.switchCoolDown).ToString();
        }
        else
        {
            bg_switchDarkBtn.SetActive(false);
            bg_switchLightBtn.SetActive(false);
        }
    }

    public void ShowSwitchDarkBtn()
    {
        switchDarkBtn.SetActive(true);
        bg_switchDarkBtn.SetActive(true);
        ThirdPersonController.Instance.switchCoolDown = 6f;
        switchLightBtn.SetActive(false);
    }

    public void ShowSwitchLightBtn()
    {
        switchLightBtn.SetActive(true);
        bg_switchLightBtn.SetActive(true);
        ThirdPersonController.Instance.switchCoolDown = 6f;
        switchDarkBtn.SetActive(false);
    }

    
}
