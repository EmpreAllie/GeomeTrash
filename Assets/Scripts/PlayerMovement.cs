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


    public static PlayerMovement Instance;
    public bool IgnoreInputForOneFrame = false;

    public AudioSource levelMusic;

    public enum PlayerMode
    {
        CUBE,
        SHIP
    }


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

        cubeToRotate = transform.Find("PlayerCube");

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


    // Update вызывается каждый кадр
    void Update()
    {
        bool jumpKeyHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Mouse0);

        // логика прыжка
        if (jumpKeyHeld && isGrounded && !PauseManager.Instance.getIsPaused())
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


    // метод реагирования на столкновение
    void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;

        if (other.CompareTag("Spike") || other.CompareTag("SpikeLow"))
        {
            Debug.Log($"Смерть игрока! Pos: {cubeToRotate.position}");            

            LevelManager.Instance.PlayerDied(gameObject, cubeToRotate.position, deathEffectPrefab);
        }
        else if (other.CompareTag("Ground") ||
                 other.CompareTag("Block"))
        {
            ContactPoint contact = collision.contacts[0];
            Vector3 normal = contact.normal;

            if (Vector3.Dot(normal, Vector3.up) > 0.5f)
            {
                if (!isGrounded)
                {
                    isGrounded = true;
                    SnapRotation();
                }

            }
            else
            {
                Debug.Log($"Player death caused by block's side collision");
                LevelManager.Instance.PlayerDied(gameObject, cubeToRotate.position, deathEffectPrefab);
            }

        }       
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
            Debug.Log("Level completed");
            LevelManager.Instance.LevelCompleted();
        }
    }
}
