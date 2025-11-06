using UnityEngine;
using TMPro;
using UnityEngine.Rendering;

public class LevelProgress : MonoBehaviour
{
    public Transform player;
    public Transform finish;
    public TMP_Text progressText;

    float startZ;
    float finishZ;

    public static LevelProgress Instance;

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
        startZ = player.position.z;
        finishZ = finish.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        float progress = Mathf.InverseLerp(startZ, finishZ, player.position.z);
        int percent = Mathf.Clamp(Mathf.RoundToInt(progress * 100f), 0, 100);
        progressText.text = percent.ToString() + "%";
    }

    public void ForceHundredPercent()
    {
        progressText.text = "100%";
    }
}
