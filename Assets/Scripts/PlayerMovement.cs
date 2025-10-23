using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public float forwardSpeed = 10f;
    public float jumpForce = 500f;
    private bool isGrounded;
    private Rigidbody rb;
    private Vector3 forwardDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        forwardDirection = Vector3.forward;
    }

    void FixedUpdate()
    {
        // 1. �������������� �������� ������
        transform.position += forwardDirection * forwardSpeed * Time.fixedDeltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        // 2. ������ ������
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 3. �������� ����� � �����������
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Game Over!");
            // ... (������ ����������� ������ ��� ������)
        }
        else if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}
