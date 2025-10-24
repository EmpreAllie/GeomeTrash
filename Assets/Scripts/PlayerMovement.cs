using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public float forwardSpeed = 10f;
    public float jumpForce = 5f;
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
        bool jumpKeyHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Mouse0);
        // 2. ������ ������
        if (jumpKeyHeld && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;

        if (other.CompareTag("Spike"))
        {
            Debug.Log("Game Over!");
            // ... (������ ����������� ������ ��� ������)
        }
        else if (other.CompareTag("Ground") ||
                 other.CompareTag("Block"))
        {
            isGrounded = true;
        }       
    }
}
