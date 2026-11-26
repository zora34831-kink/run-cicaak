using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4.5f;
    public float sprintMultiplier = 1.65f;
    public float jumpHeight = 1.5f;
    public float gravity = -18f;
    public Transform cameraTransform;

    [Header("State")]
    public bool IsHidden { get; private set; }
    public bool IsCaught { get; private set; }
    public float MoveAmount => moveInput.magnitude;
    public bool IsSprinting => sprinting;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 verticalVelocity;
    private bool sprinting;
    private bool jumpRequested;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (IsCaught || (GameManager.Instance != null && GameManager.Instance.RunEnded)) return;
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

#if UNITY_EDITOR || UNITY_STANDALONE
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (Input.GetKeyDown(KeyCode.Space)) RequestJump();
        sprinting = Input.GetKey(KeyCode.LeftShift);
#endif

        Vector3 forward = cameraTransform ? cameraTransform.forward : Vector3.forward;
        Vector3 right = cameraTransform ? cameraTransform.right : Vector3.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 move = (forward * moveInput.y + right * moveInput.x);
        if (move.sqrMagnitude > 1f) move.Normalize();

        if (move.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move), 12f * Time.deltaTime);

        float speed = moveSpeed * (sprinting ? sprintMultiplier : 1f);
        controller.Move(move * speed * (IsHidden ? 0f : 1f) * Time.deltaTime);

        if (controller.isGrounded && verticalVelocity.y < 0f)
            verticalVelocity.y = -2f;

        if (jumpRequested && controller.isGrounded && !IsHidden)
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        jumpRequested = false;
        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    public void SetMoveInput(Vector2 input) => moveInput = Vector2.ClampMagnitude(input, 1f);
    public void SetSprint(bool active) => sprinting = active;
    public void RequestJump() => jumpRequested = true;
    public void SetHidden(bool hidden) => IsHidden = hidden;

    public void Catch()
    {
        IsCaught = true;
        moveInput = Vector2.zero;
        sprinting = false;
        CatchSequence sequence = GetComponent<CatchSequence>();
        if (sequence != null) sequence.Play();
        else GameManager.Instance?.EndRun();
    }
}