using UnityEngine;

public class HidingSpot : MonoBehaviour
{
    public Transform hidePosition;
    public float interactDistance = 2.2f;
    private PlayerController player;
    private static HidingSpot activeSpot;

    void Awake()
    {
        if (player == null) player = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        if (player == null || player.IsCaught) return;
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.E) &&
            Vector3.Distance(player.transform.position, transform.position) <= interactDistance)
            ToggleHide();
#endif
    }

    public void SetPlayer(PlayerController target) => player = target;

    public void ToggleHide()
    {
        if (player == null || player.IsCaught) return;

        if (!player.IsHidden)
        {
            if (Vector3.Distance(player.transform.position, transform.position) > interactDistance)
                return;

            if (hidePosition != null)
            {
                CharacterController controller = player.GetComponent<CharacterController>();
                if (controller != null) controller.enabled = false;
                player.transform.position = hidePosition.position;
                if (controller != null) controller.enabled = true;
            }

            player.SetHidden(true);
            activeSpot = this;
            GameManager.Instance?.RegisterHide();
        }
        else
        {
            player.SetHidden(false);
            if (activeSpot == this) activeSpot = null;
        }
    }

    public static void ExitCurrentHide(PlayerController target)
    {
        if (activeSpot == null || target == null || activeSpot.player != target) return;
        activeSpot.ToggleHide();
    }
}