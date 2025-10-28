using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public AudioSource levelMusic;

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


        //Debug.Log("—павним эффект смерти в позиции: " + deathPos);
        GameObject effect = Instantiate(deathEffectPrefab, deathPos, Quaternion.identity);
        //Debug.Log("Ёффект создан: " + effect.name + " | позици€: " + effect.transform.position);

        Instantiate(deathEffectPrefab, deathPos, Quaternion.identity);

        Rigidbody rb = playerObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        AttemptManager.IncreaseAttemptCount();

        // даЄм частицам проигратьс€ 1 секунду перед перезапуском
        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
