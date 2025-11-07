using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum PlayerMode
{
    CUBE,
    SHIP,
    BALL
}

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;

    public float forwardSpeed = 10f;
    public float jumpForce = 5f;
    private bool isGrounded;
    
    private Vector3 forwardDirection;
    private Transform cubeToRotate;


    [Header("Rotation Settings")]
    public float rotationSpeed = 500f;


    [Header("Game Over")]
    public GameObject deathEffectPrefab;


    [Header("Ship Settings")]
    public GameObject shipPrefab;
    public float tiltFactor = 5f;     // множитель скорости -> угол
    public float maxTiltAngle = 30f;  // макс угол (в градусах)
    public float tiltLerpSpeed = 5f;  // скорость интерпол€ции

    [Header("Ball Settings")]
    public GameObject ballPrefab;           // префаб сферы
    public float ballGravityForce = 20f;    // сила "искусственной" гравитации
    public float ballStickThreshold = 0.7f; // dot нормали дл€ прилипани€
    public float ballRollFactor = 10f;       // множитель дл€ визуального кручени€
    public float ballRotationsPerSecond = 1f; // 2 полных оборота в секунду

    private Vector3 gravityDirection = Vector3.down;
    private bool gravityInverted = false;


    private Transform visualModel;
    private GameObject playerCube;

    [Header("Other Settings")]
    public GameObject currentModel;
    private Rigidbody rb;

    public AudioSource levelMusic;

    private PlayerMode currentMode = PlayerMode.CUBE;



    void Awake()
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
        cubeToRotate = transform.Find("PlayerCube");

        playerCube = transform.Find("PlayerCube").gameObject;
        currentModel = playerCube;
        visualModel = playerCube.transform;

        rb = GetComponent<Rigidbody>();
        forwardDirection = Vector3.forward;

        AlignColliderWithChild();
    }



    void FixedUpdate()
    {
        // автоматическое движение вперЄд
        //transform.position += forwardDirection * forwardSpeed * Time.fixedDeltaTime;

        // гравитаци€ дл€ Ball: используем AddForce в FixedUpdate
        if (currentMode == PlayerMode.BALL)
        {
            rb.AddForce(gravityDirection * ballGravityForce, ForceMode.Acceleration);
        }
    }


    // Update вызываетс€ каждый кадр
    void Update()
    {
        bool jumpKeyHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Mouse0);

        if (PauseManager.Instance.getIsPaused())
            return;

        if (LevelManager.Instance.isDead) 
            return;

        transform.position += forwardDirection * forwardSpeed * Time.deltaTime;

        if (currentMode == PlayerMode.CUBE)
        {
            // логика прыжка
            if (jumpKeyHeld && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
            }

            // вращение куба во врем€ прыжка
            if (!isGrounded)
            {
                cubeToRotate.Rotate(Vector3.right, rotationSpeed * Time.deltaTime, Space.Self);
            }
        }
        else if (currentMode == PlayerMode.SHIP)
        {
            float verticalForce = jumpKeyHeld ? 1f : -1f;
            rb.AddForce(Vector3.up * verticalForce * jumpForce * 0.5f, ForceMode.Acceleration);

            UpdateShipTilt();
        }
        else if (currentMode == PlayerMode.BALL)
        {

            HandleBallInput();   // переключение гравитации по нажатию
                                 // примен€ем "гравитацию" в FixedUpdate, чтобы физика велась предсказуемо
            UpdateBallVisuals(); // визуальное кручение
        }
    }
    
    private void SnapRotation()
    {
        float currentX = cubeToRotate.eulerAngles.x;
        float snappedX = Mathf.Round(currentX / 90f) * 90f;
        cubeToRotate.localRotation = Quaternion.Euler(snappedX, 0f, 0f);
    }

    
    // метод реагировани€ на столкновение
    void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;

        if (other.CompareTag("Spike") || other.CompareTag("SpikeLow"))
        {
            Debug.Log($"Player death caused by spike. Pos: {cubeToRotate.position}");            

            LevelManager.Instance.PlayerDied(gameObject, cubeToRotate.position, deathEffectPrefab);
        }
        else if (other.CompareTag("Ground") ||
                 other.CompareTag("Block"))
        {
            ContactPoint contact = collision.contacts[0];
            Vector3 normal = contact.normal;

            /*
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
            */
            bool isHorizontalSurface = Mathf.Abs(normal.y) > Mathf.Abs(normal.x) &&
                                  Mathf.Abs(normal.y) > Mathf.Abs(normal.z);

            if (isHorizontalSurface)
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


    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Block"))
        {
            isGrounded = false;
            Debug.Log("Left ground - isGrounded = false");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
            Debug.Log("Level completed");

            LevelManager.Instance.LevelCompleted();
        }
        else if (other.CompareTag("ShipPortal"))
        {
            Debug.Log("Entered Ship Portal");

            currentMode = PlayerMode.SHIP;
            SwitchToShipMode();            
        }
        else if (other.CompareTag("CubePortal"))
        {
            Debug.Log("Entered Cube Portal");

            currentMode = PlayerMode.CUBE;
            SwitchToCubeMode();            
        }
        else if (other.CompareTag("BallPortal"))
        {
            Debug.Log("Entered Ball Portal");

            currentMode = PlayerMode.BALL;
            SwitchToBallMode();
        }
    }



    private void UpdateShipTilt()
    {
        if (visualModel == null || rb == null) return;

        float verticalVelocity = rb.linearVelocity.y;

        float targetAngle = Mathf.Clamp(-verticalVelocity * tiltFactor, -maxTiltAngle, maxTiltAngle);

        float currentZ = visualModel.localEulerAngles.z;

        float newZ = Mathf.LerpAngle(currentZ, targetAngle, Time.deltaTime * tiltLerpSpeed);

        Vector3 e = visualModel.localEulerAngles;
        visualModel.localEulerAngles = new Vector3(e.x, e.y, newZ);
    }

    private void HandleBallInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0) && isGrounded)
        {
            Debug.Log($"Click in Ball Mode, isGrounded: {isGrounded}");

            gravityInverted = !gravityInverted;
            gravityDirection = gravityInverted ? Vector3.up : Vector3.down;

            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;
        }
    }

    private void UpdateBallVisuals()
    {
        if (currentModel == null || rb == null) return;

        float degreesPerSecond = 360f * ballRotationsPerSecond;

        // ”множаем на скорость дл€ св€зи с движением
        float speedMultiplier = forwardSpeed / 10f; // нормализуем скорость

        float rollAmount = degreesPerSecond * speedMultiplier * Time.deltaTime;

        visualModel.Rotate(rollAmount, 0, 0, Space.World);
    }

    private void SwitchToCubeMode()
    {
        //if (currentMode == PlayerMode.CUBE) return;

        if (currentModel != null)
            Destroy(currentModel);

        playerCube.SetActive(true);

        currentModel = playerCube;
        visualModel = playerCube.transform;

        rb.useGravity = true;
        AlignColliderWithChild();
    }

    private void SwitchToShipMode()
    {
        if (shipPrefab == null) return;
        /*
        if (currentModel != null)
            currentModel.SetActive(false);
        */
        playerCube.SetActive(false);

        currentModel = Instantiate(shipPrefab, currentModel.transform.position, transform.rotation, transform);
        currentModel.SetActive(true);

        visualModel = currentModel.transform;

        rb.useGravity = false;
        isGrounded = false;

        AlignColliderWithChild();
    }

    private void SwitchToBallMode()
    {
        Debug.Log("SwitchToBallMode() . . . ");
        if (ballPrefab == null) return;


        if (currentModel != null)
            currentModel.SetActive(false);

        currentModel = Instantiate(ballPrefab, currentModel.transform.position, transform.rotation, transform);
        currentModel.SetActive(true);

        visualModel = currentModel.transform;

        rb.useGravity = false; // отключаем стандартную гравитацию
        gravityDirection = Vector3.down;
        AlignColliderWithChild();
    }

    void AlignColliderWithChild()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null) return;

        Transform child = transform.Find("PlayerCube");
        if (child == null) return;

        col.center = child.localPosition;

        Vector3 childScale = child.localScale;
        col.size = childScale;
    }
}

