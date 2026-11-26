using UnityEngine;
using UnityEngine.EventSystems;

public class MobileActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum ActionType { Sprint, Jump, Hide }
    public ActionType actionType;
    public PlayerController player;
    public HidingSpot nearbyHidingSpot;

    void Awake()
    {
        if (player == null) player = FindObjectOfType<PlayerController>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (player == null) return;

        switch (actionType)
        {
            case ActionType.Sprint:
                player.SetSprint(true);
                break;
            case ActionType.Jump:
                player.RequestJump();
                break;
            case ActionType.Hide:
                if (nearbyHidingSpot != null) nearbyHidingSpot.ToggleHide();
                break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (actionType == ActionType.Sprint && player != null)
            player.SetSprint(false);
    }
}