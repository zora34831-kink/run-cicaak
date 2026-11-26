using UnityEngine;

public class HidingSpot : MonoBehaviour
{
    public Transform hidePosition;
    public float interactDistance = 2.2f;

    private PlayerController player;

    void Update()
    {
        if (player == null || player.IsCaught) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.E)) ToggleHide();
#endif
    }

    public void SetPlayer(PlayerController target) => player = target;

    public void ToggleHide()
    {
        if (player == null) return;

        if (!player.IsHidden)
        {
            if (Vector3.Distance(player.transform.position, transform.position) > interactDistance)
                return;

            if (hidePosition != null)
                player.transform.position = hidePosition.position;

            player.SetHidden(true);
            GameManager.Instance?.RegisterHide();
        }
        else
        {
            player.SetHidden(false);
        }
    }
}