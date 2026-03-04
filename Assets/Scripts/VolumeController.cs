using UnityEngine;
using UnityEngine.Rendering;

public class VolumeController : Singleton<VolumeController>
{
    public VolumeProfile darkPP;
    public VolumeProfile lightPP;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeDarkVolume()
    {
        GetComponent<Volume>().profile = darkPP;
    }

    public void ChangeLightVolume() 
    {


        GetComponent<Volume>().profile = lightPP;

    }


}
