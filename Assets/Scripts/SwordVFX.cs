using UnityEngine;

public class SwordVFX : MonoBehaviour
{
    public GameObject[] swordEffects;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowVFX1()
    {
        foreach (var effect in swordEffects) 
        {
            if (effect!=null)
            {
                effect.SetActive(false);
            }
        }
        swordEffects[0].SetActive(true);
        AudioManager.Instance.PlayAttack();
    }

    public void ShowVFX2()
    {
        foreach (var effect in swordEffects)
        {
            if (effect != null)
            {
                effect.SetActive(false);
            }
        }
        swordEffects[1].SetActive(true);
        AudioManager.Instance.PlayAttack();
    }
    public void ShowVFX3()
    {
        foreach (var effect in swordEffects)
        {
            if (effect != null)
            {
                effect.SetActive(false);
            }
        }
        swordEffects[2].SetActive(true);
        AudioManager.Instance.PlayAttack();
    }

    public void ShowVFX4()
    {
        foreach (var effect in swordEffects)
        {
            if (effect != null)
            {
                effect.SetActive(false);
            }
        }
        swordEffects[3].SetActive(true);
        AudioManager.Instance.PlayAttack();
    }

    public void ShowVFX5()
    {
        foreach (var effect in swordEffects)
        {
            if (effect != null)
            {
                effect.SetActive(false);
            }
        }
        swordEffects[4].SetActive(true);
        AudioManager.Instance.PlayAttack();
    }

    public void ShowVFX6()
    {
        foreach (var effect in swordEffects)
        {
            if (effect != null)
            {
                effect.SetActive(false);
            }
        }
        swordEffects[5].SetActive(true);
        AudioManager.Instance.PlayAttack();
    }

    public void ShowVFX7()
    {
        foreach (var effect in swordEffects)
        {
            if (effect != null)
            {
                effect.SetActive(false);
            }
        }
        swordEffects[6].SetActive(true);
        AudioManager.Instance.PlayAttack();
    }

    public void ShowVFX8()
    {
        foreach (var effect in swordEffects)
        {
            if (effect != null)
            {
                effect.SetActive(false);
            }
        }
        swordEffects[7].SetActive(true);
        AudioManager.Instance.PlayAttack();
    }
}
