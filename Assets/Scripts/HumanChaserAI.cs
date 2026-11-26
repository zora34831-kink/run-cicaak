using UnityEngine;

public class HumanChaserAI : MonoBehaviour
{
    public Transform target;
    public float walkSpeed = 2.4f;
    public float catchDistance = 1.25f;
    public float checkRadius = 7f;
    public float searchDuration = 2.5f;

    private PlayerController player;
    private float searchTimer;

    void Start()
    {
        if (target != null) player = target.GetComponent<PlayerController>();
    }

    void Update()
    {
        if (target == null || player == null || player.IsCaught) return;

        if (player.IsHidden)
        {
            searchTimer += Time.deltaTime;
            if (searchTimer >= searchDuration)
                searchTimer = 0f;
            return;
        }

        searchTimer = 0f;
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.magnitude > catchDistance)
        {
            Vector3 step = direction.normalized * walkSpeed * Time.deltaTime;
            transform.position += step;
            if (direction.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 7f * Time.deltaTime);
        }
        else
        {
            player.Catch();
        }
    }
}