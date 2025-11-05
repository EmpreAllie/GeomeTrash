using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelUIManager : MonoBehaviour
{
    public GameObject levelCompleteUI;

    public void ShowLevelComplete()
    {
        levelCompleteUI.SetActive(true);
    }

    public void OnClickBackToMenu()
    {
        Time.timeScale = 1f;
        AttemptManager.ResetAttempts();

        UIManager.ShouldShowLevelSelectStatic = true;
        SceneManager.LoadScene("MenuScene");
    }

    public void OnClickRestart() 
    {
        Time.timeScale = 1f;
        AttemptManager.ResetAttempts();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
