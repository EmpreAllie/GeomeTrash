using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayerDied(GameObject playerObject, Vector3 deathPos, GameObject deathEffectPrefab)
    {
        Instantiate(deathEffectPrefab, deathPos, Quaternion.identity);

        playerObject.transform.GetChild(0).gameObject.SetActive(false);

        Time.timeScale = 0f;

        StartCoroutine(RestartLevelAfterDelay(1f));
    }

    private IEnumerator RestartLevelAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Time.timeScale = 1f;

        // просто заново загружается та же сцена-уроень, где по дефолту куб движется вперед
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
