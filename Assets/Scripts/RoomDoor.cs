using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RoomDoor : MonoBehaviour
{
    [Tooltip("Place an empty Transform at the destination room entrance.")]
    public Transform destination;
    public bool requireInteraction;
    private bool playerNearby;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (requireInteraction && playerNearby && Input.GetKeyDown(KeyCode.E))
            MovePlayer();
#endif
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<PlayerController>()) return;
        playerNearby = true;
        if (!requireInteraction) MovePlayer();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>()) playerNearby = false;
    }

    public void Interact() => MovePlayer();

    private void MovePlayer()
    {
        if (destination == null) return;
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null) return;

        CharacterController character = player.GetComponent<CharacterController>();
        if (character != null) character.enabled = false;
        player.transform.SetPositionAndRotation(destination.position, destination.rotation);
        if (character != null) character.enabled = true;
        player.SetMoveInput(Vector2.zero);
        playerNearby = false;
    }
}