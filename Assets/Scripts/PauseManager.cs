using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    public GameObject pausePanel;
    private bool isPaused = false;
    public bool getIsPaused() { return isPaused; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }



    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }


    public void ResumeGame()
    {
        LevelManager.Instance.UnpauseMusic();
        pausePanel.SetActive(false);        
        LevelManager.Instance.UnpauseMusic();
        Time.timeScale = 1f;
        isPaused = false;
    }


    public void PauseGame()
    {
        LevelManager.Instance.PauseMusic();
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        AttemptManager.IncreaseAttemptCount();   
        isPaused = false;

        Scene curScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(curScene.name);
        pausePanel.SetActive(false);
    }




    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        AttemptManager.ResetAttempts();
        UIManager.ShouldShowLevelSelectStatic = true;

        SceneManager.LoadScene("MenuScene");
    }
}
