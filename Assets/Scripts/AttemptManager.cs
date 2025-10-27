using TMPro;
using UnityEngine;

public class AttemptManager : MonoBehaviour
{
    public static int attemptCount = 1;
    public TextMeshProUGUI attemptText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateAttemptText();
    }

    public void UpdateAttemptText()
    {
        if (attemptText != null)
        {
            attemptText.text = "Attempt " + attemptCount;
        }
    }

    public static void IncreaseAttemptCount()
    {
        attemptCount++;
    }

    public static void ResetAttempts()
    {
        attemptCount = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
