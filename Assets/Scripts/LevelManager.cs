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

    bool isDead = false;

    public void PlayerDied(GameObject playerObject, Vector3 deathPos, GameObject deathEffectPrefab)
    {
        if (isDead) return;
        isDead = true;
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


        Debug.Log("������� ������ ������ � �������: " + deathPos);
        GameObject effect = Instantiate(deathEffectPrefab, deathPos, Quaternion.identity);
        Debug.Log("������ ������: " + effect.name + " | �������: " + effect.transform.position);

        Instantiate(deathEffectPrefab, deathPos, Quaternion.identity);

        Rigidbody rb = playerObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // ��� �������� ����������� 1 ������� ����� ������������
        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /*
    public void PlayerDied(GameObject playerObject, Vector3 deathPos, GameObject deathEffectPrefab)
    {
        Transform cameraHolder = playerObject.transform.Find("CameraHolder");        
        if (cameraHolder != null)
        {
            cameraHolder.SetParent(this.transform);
        }

        Transform playerCube = playerObject.transform.Find("PlayerCube");
        if (playerCube != null)
        {
            playerCube.gameObject.SetActive(false);
        }

        Instantiate(deathEffectPrefab, deathPos, Quaternion.identity);

        Rigidbody rb = playerObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; // ��������� ������
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);



        //StartCoroutine(RestartLevelAfterDelay(1f));
    }
    */

    /*
    private IEnumerator RestartLevelAfterDelay(float delay)
    {
        //yield return new WaitForEndOfFrame();

        //Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(delay);
        
        if (effect != null)
        {
            Destroy(effect);
        }

        Time.timeScale = 1f;
        


        // ������ ������ ����������� �� �� �����-������, ��� �� ������� ��� �������� ������
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }*/
}
