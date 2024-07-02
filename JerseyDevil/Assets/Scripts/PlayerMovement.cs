using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float jumpForce = 5f;

    private Transform cameraTransform;
    private Rigidbody rb;
    private float verticalLookRotation;

    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraTransform = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;

        // Ensure Rigidbody settings
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.freezeRotation = true;
    }

    void Update()
    {
       
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleMouseLook();
    }

    void HandleMovement()
    {
        // Ground check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, LayerMask.GetMask("Ground"));

        // Get movement input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Calculate movement direction
        Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;

        // Apply movement
        Vector3 velocity = moveDirection * moveSpeed;
        velocity.y = rb.velocity.y; // Preserve vertical velocity (gravity)

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = jumpForce;
        }

        rb.velocity = velocity;
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate player horizontally (yaw)
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera vertically (pitch)
        verticalLookRotation -= mouseY;
        cameraTransform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
    }

    private void OnCollisionStay(Collision collision)
    {
        // Update isGrounded status based on collisions with ground
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Reset isGrounded status when leaving ground collision
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
        }
    }
}
