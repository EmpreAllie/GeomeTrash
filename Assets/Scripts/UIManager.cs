using UnityEngine;

public class UIManager : MonoBehaviour
{

    public GameObject panelMainMenu;
    public GameObject panelSelectLevel;
    public GameObject panelFAQ;
    public GameObject panelSettings;

    public string levelOneSceneName = "LevelScene_01";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShowPanel(panelMainMenu);
    }

    public void ShowPanel(GameObject panelToShow) 
    { 
        panelMainMenu.SetActive(false);
        panelSelectLevel.SetActive(false);
        panelFAQ.SetActive(false);
        panelSettings.SetActive(false);

        panelToShow.SetActive(true);
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
        Debug.Log($"Загрузка уровня {levelIndex}");
        // SceneManager.LoadScene(levelOne);
    }

    public void OnClickBackToMenu()
    {
        ShowPanel(panelMainMenu);
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
