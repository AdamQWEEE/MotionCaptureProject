using StarterAssets;
using UnityEngine;
//using UnityEngine.Windows;

public class LevelManager : Singleton<LevelManager>
{
    public GameObject levelPanel;
    public bool isPause;
    public ThirdPersonController player;
    private void Awake()
    {
        Time.timeScale = 0f;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isPause = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            levelPanel.SetActive(true);
            ThirdPersonController.Instance.GetComponent<StarterAssetsInputs>().cursorLocked = true;
            Time.timeScale = 0f;
            AudioManager.Instance.StopMusic();
            isPause = true;
        }
    }

    public void PlayGame()
    {
        Time.timeScale = 1f;
        isPause = false;
        levelPanel.SetActive(false);
        player = ThirdPersonController.Instance;
        player.GetComponent<StarterAssetsInputs>().cursorLocked = false;
        player.ResetInput();
        //Time.timeScale =; 1f;
        AudioManager.Instance.PlayBossMusic();
       


    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    
}
