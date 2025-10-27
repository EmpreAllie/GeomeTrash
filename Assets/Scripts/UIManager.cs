using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject panelMainMenu;
    public GameObject panelSelectLevel;
    public GameObject panelFAQ;
    public GameObject panelSettings;

    private GameObject currentActivePanel;

    public static bool ShouldShowLevelSelectStatic = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (ShouldShowLevelSelectStatic)
        {
            ShowPanel(panelSelectLevel);

            ShouldShowLevelSelectStatic = false;
        }
        else
        {
            ShowPanel(panelMainMenu);
        }
    }




    public void ShowPanel(GameObject panelToShow) 
    { 
        panelMainMenu.SetActive(false);
        panelSelectLevel.SetActive(false);
        panelFAQ.SetActive(false);
        panelSettings.SetActive(false);

        panelToShow.SetActive(true);
        currentActivePanel = panelToShow;
    }



    public void OnClickStartGame()
    {
        ShowPanel(panelSelectLevel);
    }



    public void OnClickOpenFAQ()
    {
        ShowPanel(panelFAQ);
    }



    public void OnClickOpenSettings()
    {
        ShowPanel(panelSettings);
    }




    public void OnClickLoadLevel(int levelIndex) 
    {
        string sceneToLoad = "";

        if (levelIndex == 1)
        {
            sceneToLoad = "StereoSadness";
        }

        SceneManager.LoadScene(sceneToLoad);
        
    }




    public void OnClickBackToMenu()
    {
        ShowPanel(panelMainMenu);
    }


    



    public void OnCLickQuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

        Debug.Log("Quit Game");
    }




    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentActivePanel != panelMainMenu)
            {
                OnClickBackToMenu();
            }
        }
    }
}
