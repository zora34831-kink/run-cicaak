using UnityEngine;

public class HumanChaserAI : MonoBehaviour
{
    public Transform target;
    public float walkSpeed = 2.4f;
    public float catchDistance = 1.25f;
    public float obstacleProbeDistance = 1.25f;
    public float obstacleProbeRadius = 0.32f;
    public float gravity = -20f;
    public float searchDuration = 2.5f;

    private PlayerController player;
    private CharacterController motor;
    private Vector3 verticalVelocity;
    private float searchTimer;
    private Vector3 lastKnownPosition;

    public bool IsMoving { get; private set; }

    void Awake()
    {
        motor = GetComponent<CharacterController>();
    }

    void Start()
    {
        if (target != null)
        {
            player = target.GetComponent<PlayerController>();
            lastKnownPosition = target.position;
        }
    }

    void Update()
    {
        IsMoving = false;
        if (target == null || player == null || player.IsCaught ||
            (GameManager.Instance != null && GameManager.Instance.RunEnded)) return;

        if (player.IsHidden)
        {
            searchTimer += Time.deltaTime;
            Vector3 toLastSeen = lastKnownPosition - transform.position;
            toLastSeen.y = 0f;

            if (searchTimer < searchDuration && toLastSeen.magnitude > 1.6f)
            {
                Vector3 searchDirection = AvoidObstacles(toLastSeen.normalized);
                IsMoving = searchDirection.sqrMagnitude > 0.01f;
                MoveCharacter(searchDirection * walkSpeed * 0.72f);
            }
            else
            {
                IsMoving = false;
                ApplyGravityOnly();
                transform.Rotate(Vector3.up, 32f * Time.deltaTime, Space.World);
            }
            return;
        }

        searchTimer = 0f;
        lastKnownPosition = target.position;
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        if (distance <= catchDistance)
        {
            player.Catch();
            return;
        }

        Vector3 desiredDirection = toTarget.normalized;
        Vector3 movementDirection = AvoidObstacles(desiredDirection);

        if (movementDirection.sqrMagnitude > 0.01f)
        {
            IsMoving = true;
            Quaternion desiredRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, 7f * Time.deltaTime);
        }

        Vector3 horizontal = movementDirection * walkSpeed;
        MoveCharacter(horizontal);
    }

    Vector3 AvoidObstacles(Vector3 desired)
    {
        if (!IsBlocked(desired)) return desired;

        Vector3 left = Quaternion.Euler(0f, -55f, 0f) * desired;
        Vector3 right = Quaternion.Euler(0f, 55f, 0f) * desired;
        bool leftBlocked = IsBlocked(left);
        bool rightBlocked = IsBlocked(right);

        if (!leftBlocked && rightBlocked) return left;
        if (!rightBlocked && leftBlocked) return right;
        if (!leftBlocked && !rightBlocked)
        {
            float leftDistance = DistanceToObstacle(left);
            float rightDistance = DistanceToObstacle(right);
            return leftDistance >= rightDistance ? left : right;
        }

        Vector3 reverse = Quaternion.Euler(0f, 145f, 0f) * desired;
        return IsBlocked(reverse) ? Vector3.zero : reverse;
    }

    bool IsBlocked(Vector3 direction) => DistanceToObstacle(direction) < obstacleProbeDistance;

    float DistanceToObstacle(Vector3 direction)
    {
        Vector3 origin = transform.position + Vector3.up * 0.65f;
        RaycastHit[] hits = Physics.SphereCastAll(
            origin, obstacleProbeRadius, direction, obstacleProbeDistance,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore
        );

        float nearest = obstacleProbeDistance + 0.1f;
        foreach (RaycastHit hit in hits)
        {
            Transform hitRoot = hit.collider.transform.root;
            if (hitRoot == transform.root || hitRoot == target.root) continue;
            nearest = Mathf.Min(nearest, hit.distance);
        }
        return nearest;
    }

    void MoveCharacter(Vector3 horizontalVelocity)
    {
        if (motor != null)
        {
            if (motor.isGrounded && verticalVelocity.y < 0f) verticalVelocity.y = -2f;
            verticalVelocity.y += gravity * Time.deltaTime;
            motor.Move((horizontalVelocity + verticalVelocity) * Time.deltaTime);
        }
        else
        {
            transform.position += horizontalVelocity * Time.deltaTime;
        }
    }

    void ApplyGravityOnly()
    {
        if (motor == null) return;
        if (motor.isGrounded && verticalVelocity.y < 0f) verticalVelocity.y = -2f;
        verticalVelocity.y += gravity * Time.deltaTime;
        motor.Move(verticalVelocity * Time.deltaTime);
    }
}