using UnityEngine;

public class DestroyOnFinish : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();

        float totalDuration = ps.main.duration;

        Destroy(gameObject, totalDuration);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
