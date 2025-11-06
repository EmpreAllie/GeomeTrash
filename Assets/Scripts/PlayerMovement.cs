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
    public float tiltLerpSpeed = 5f;  // скорость интерполяции

    [Header("Ball Settings")]
    public GameObject ballPrefab;           // префаб сферы
    public float ballGravityForce = 20f;    // сила "искусственной" гравитации
    public float ballStickThreshold = 0.7f; // dot нормали для прилипания
    public float ballRollFactor = 5f;       // множитель для визуального кручения

    private Vector3 gravityDirection = Vector3.down;
    private bool gravityInverted = false;
    private bool ballJustToggled = false;   // защита от многократных переключений в кадр


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
        // автоматическое движение вперёд
        transform.position += forwardDirection * forwardSpeed * Time.fixedDeltaTime;

        // гравитация для Ball: используем AddForce в FixedUpdate
        if (currentMode == PlayerMode.BALL)
        {
            rb.AddForce(gravityDirection * ballGravityForce, ForceMode.Acceleration);
        }
    }


    // Update вызывается каждый кадр
    void Update()
    {
        bool jumpKeyHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Mouse0);

        if (PauseManager.Instance.getIsPaused())
            return;

        if (currentMode == PlayerMode.CUBE)
        {
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
        else if (currentMode == PlayerMode.SHIP)
        {
            float verticalForce = jumpKeyHeld ? 1f : -1f;
            rb.AddForce(Vector3.up * verticalForce * jumpForce * 0.5f, ForceMode.Acceleration);

            UpdateShipTilt();
        }
        else if (currentMode == PlayerMode.BALL)
        {
            HandleBallInput();   // переключение гравитации по нажатию
                                 // применяем "гравитацию" в FixedUpdate, чтобы физика велась предсказуемо
            UpdateBallVisuals(); // визуальное кручение
        }
    }

    private void SnapRotation()
    {
        float currentX = cubeToRotate.eulerAngles.x;
        float snappedX = Mathf.Round(currentX / 90f) * 90f;
        cubeToRotate.localRotation = Quaternion.Euler(snappedX, 0f, 0f);
    }

    /*
    // метод реагирования на столкновение
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
    */

    void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        HandleCollision(collision);
    }

    private void HandleCollision(Collision collision)
    {
        GameObject other = collision.gameObject;

        if (other.CompareTag("Spike") || other.CompareTag("SpikeLow"))
        {
            LevelManager.Instance.PlayerDied(gameObject, transform.position, deathEffectPrefab);
            return;
        }

        if (other.CompareTag("Ground") || other.CompareTag("Block"))
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                Vector3 normal = contact.normal;

                // если гравитация вниз — надо иметь нормаль вверх, чтобы считалось полом
                // если гравитация вверх — нужно нормаль вниз (т.е. Vector3.Dot(normal, Vector3.down) близко к 1)
                if (!gravityInverted)
                {
                    if (Vector3.Dot(normal, Vector3.up) > ballStickThreshold)
                    {
                        // стоим на полу
                        isGrounded = true;
                        // зануляем вертикальную скорость, чтобы "прилипнуть"
                        Vector3 v = rb.linearVelocity; v.y = 0f; rb.linearVelocity = v;
                        // немного смещаем игрока на контактную точку, чтобы избежать просеков
                        transform.position += normal * 0.01f;
                    }
                }
                else
                {
                    if (Vector3.Dot(normal, Vector3.down) > ballStickThreshold)
                    {
                        // стоим на потолке (прилипли)
                        isGrounded = true;
                        Vector3 v = rb.linearVelocity; v.y = 0f; rb.linearVelocity = v;
                        transform.position += normal * 0.01f;
                    }
                }
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

    private void SwitchToCubeMode()
    {
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
        // Получаем BoxCollider на PlayerMover
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null) return;

        // Получаем дочерний объект, к которому привязываем коллайдер
        Transform child = transform.Find("PlayerCube");
        if (child == null) return;

        // Привязываем центр коллайдера к локальной позиции дочернего объекта
        col.center = child.localPosition;

        // Привязываем размеры коллайдера к масштабам дочернего объекта
        Vector3 childScale = child.localScale;
        col.size = childScale;
    }


    private void UpdateShipTilt()
    {
        if (visualModel == null || rb == null) return;

        // используем реальную вертикальную скорость
        float verticalVelocity = rb.linearVelocity.y;

        // вычисляем целевой угол (от вертикальной скорости к углу)
        float targetAngle = Mathf.Clamp(-verticalVelocity * tiltFactor, -maxTiltAngle, maxTiltAngle);
        // минус здесь потому что при положительной скорости (вверх) мы хотим наклон вверх или в нужную сторону
        // точную формулу можешь инвертировать под желаемый эффект

        // текущий локальный угол вокруг Z (или X — в зависимости от ориентации модели)
        // используем LerpAngle для корректного перехода через 360->0
        float currentZ = visualModel.localEulerAngles.z;
        // localEulerAngles.z даёт 0..360 — лучше работать через Mathf.DeltaAngle / LerpAngle
        float newZ = Mathf.LerpAngle(currentZ, targetAngle, Time.deltaTime * tiltLerpSpeed);

        // применяем только по нужной оси; не трогаем другие оси
        Vector3 e = visualModel.localEulerAngles;
        visualModel.localEulerAngles = new Vector3(e.x, e.y, newZ);

        // отладка 
        // Debug.Log($"velY={verticalVelocity:F2} target={targetAngle:F1} curZ={currentZ:F1} newZ={newZ:F1}");
    }


    private void HandleBallInput()
    {
        // переключаем гравитацию только при нажатии (один тик) — GetKeyDown
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0))
        {
            gravityInverted = !gravityInverted;
            gravityDirection = gravityInverted ? Vector3.up : Vector3.down;

            // обнуляем вертикальную скорость при переключении,
            // чтобы избежать сильных проходов через геометрию
            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;

            // повернуть визуальную модель на 180 по X/Z чтобы она выглядела "перевернутой"
            if (visualModel != null)
                visualModel.localRotation = Quaternion.Euler(180f, 0f, 0f) * Quaternion.identity; // или плавно анимируй
        }
    }

    private void UpdateBallVisuals()
    {
        if (currentModel == null || rb == null) return;

        // Предполагаем, что шар движется вдоль оси Z
        float forwardSpeed = rb.linearVelocity.z;

        // чем быстрее катится — тем быстрее вращается
        float rollAmount = forwardSpeed * ballRollFactor;

        // вращаем визуальную модель вокруг оси X (катится вперёд)
        visualModel.Rotate(Vector3.right * rollAmount * Time.deltaTime, Space.Self);
    }
}

