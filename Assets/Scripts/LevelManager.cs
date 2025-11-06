using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public AudioSource levelMusic;
    public AudioClip deathSoundClip;

    public GameObject levelCompleteUI;

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
        
    }

    public void PauseMusic()
    {
        if (levelMusic != null && levelMusic.isPlaying)
        {
            levelMusic.Pause();
        }
    }

    public void UnpauseMusic()
    {
        if (levelMusic != null && !levelMusic.isPlaying)
        {
            levelMusic.UnPause();
        }
    }


    public void PlayerDied(GameObject playerObject, Vector3 deathPos, GameObject deathEffectPrefab)
    {
        if (levelMusic != null && levelMusic.isPlaying)
        {
            levelMusic.Stop();
        }

        StartCoroutine(PlayerDeathRoutine(playerObject, deathPos, deathEffectPrefab));
    }

    private IEnumerator PlayerDeathRoutine(GameObject playerObject, Vector3 deathPos, GameObject deathEffectPrefab)
    {
        Transform cameraHolder = playerObject.transform.Find("CameraHolder");
        if (cameraHolder != null)
            cameraHolder.SetParent(this.transform);

        Transform playerCube = playerObject.transform.Find("PlayerCube");
        if (playerCube != null)
            playerCube.gameObject.SetActive(false);


        //Спавним эффект смерти в позиции: deathPos
        GameObject effect = Instantiate(deathEffectPrefab, deathPos, Quaternion.identity);        

       // Instantiate(deathEffectPrefab, deathPos, Quaternion.identity);

        if (deathSoundClip != null)
        {
            AudioSource.PlayClipAtPoint(deathSoundClip, deathPos);
        }

        Rigidbody rb = playerObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }            
            rb.isKinematic = true;
        }

        AttemptManager.IncreaseAttemptCount();

        // даём частицам проиграться 1 секунду перед перезапуском
        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public void LevelCompleted()
    {
        if (levelMusic != null && levelMusic.isPlaying)
        {
            levelMusic.Stop();
        }

        LevelProgress.Instance.ForceHundredPercent();

        Time.timeScale = 0f;

        var player = GameObject.FindGameObjectWithTag("PlayerCube");
        player.GetComponent<PlayerMovement>().enabled = false;

        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(true);
        }
    }
}
