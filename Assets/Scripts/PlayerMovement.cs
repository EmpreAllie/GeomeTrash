using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum PlayerMode
{
    CUBE,
    SHIP
}

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;

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

    [Header("Ship Settings")]
    public GameObject shipPrefab;
    public float tiltFactor = 5f;     // множитель скорости -> угол
    public float maxTiltAngle = 30f;  // макс угол (в градусах)
    public float tiltLerpSpeed = 5f;  // скорость интерполяции
    public Transform visualModel;



    public AudioSource levelMusic;

    private PlayerMode currentMode = PlayerMode.CUBE;
    public GameObject currentModel;


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

        cubeToRotate = transform.Find("PlayerCube");
        currentModel = cubeToRotate.gameObject;

        rb = GetComponent<Rigidbody>();
        forwardDirection = Vector3.forward;        
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AlignColliderWithChild();
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
        if (currentMode == PlayerMode.SHIP)
        {
            float verticalForce = jumpKeyHeld ? 1f : -1f;
            rb.AddForce(Vector3.up * verticalForce * jumpForce * 0.5f, ForceMode.Acceleration);

            UpdateShipTilt();

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
            rb.useGravity = false;
            isGrounded = false;
        }
        else if (other.CompareTag("CubePortal"))
        {
            Debug.Log("Entered Cube Portal");
            currentMode = PlayerMode.CUBE;
            rb.useGravity = true;
        }
    }

    private void SwitchToShipMode()
    {
        if (shipPrefab == null) return;

        if (currentModel != null)
            currentModel.SetActive(false);
        
        currentModel = Instantiate(shipPrefab, currentModel.transform.position, transform.rotation, transform);
        currentModel.SetActive(true);
        visualModel = currentModel.transform;

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
}

