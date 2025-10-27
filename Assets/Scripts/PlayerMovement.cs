using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float forwardSpeed = 10f;
    public float jumpForce = 5f;
    private bool isGrounded;
    private Rigidbody rb;
    private Vector3 forwardDirection;


    private Transform cubeToRotate;

    [Header("Rotation Settings")]
    public float rotationSpeed = 500f;

    [Header("Game Over")]
    public GameObject deathEffectPrefab;
    public LevelManager levelManager;


    void Awake()
    {
        cubeToRotate = transform.Find("PlayerCube");
        levelManager = FindAnyObjectByType<LevelManager>();

        if (levelManager == null)
        {
            Debug.LogError("LevelManager is NULL");
        }

        rb = GetComponent<Rigidbody>();
        forwardDirection = Vector3.forward;
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }



    void FixedUpdate()
    {
        // автоматическое движение вперёд
        transform.position += forwardDirection * forwardSpeed * Time.fixedDeltaTime;
    }


    // Update is called once per frame
    void Update()
    {
        bool jumpKeyHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Mouse0);

        // логика прыжка
        if (jumpKeyHeld && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;            
        }

        // вращение куба во время прыжка
        if (!isGrounded)
        {
            cubeToRotate.Rotate(Vector3.right, rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    private void SnapRotation()
    {
        float currentX = cubeToRotate.eulerAngles.x;
        float snappedX = Mathf.Round(currentX / 90f) * 90f;
        cubeToRotate.localRotation = Quaternion.Euler(snappedX, 0f, 0f);
    }


    


    void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;

        if (other.CompareTag("Spike"))
        {
            Debug.Log("Game Over!");
            // ... (логика перезапуска уровня или смерти)

            levelManager.PlayerDied(gameObject, cubeToRotate.position, deathEffectPrefab);
        }
        else if (other.CompareTag("Ground") ||
                 other.CompareTag("Block"))
        {
            if (!isGrounded)
            {
                isGrounded = true;
                SnapRotation();
            }            
        }       
    }
}
