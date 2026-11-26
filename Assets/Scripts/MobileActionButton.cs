using UnityEngine;
using UnityEngine.EventSystems;

public class MobileActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum ActionType { Sprint, Jump, Hide }
    public ActionType actionType;
    public PlayerController player;
    public HidingSpot nearbyHidingSpot;
    public float hideSearchRadius = 3f;

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
                if (player.IsHidden)
                {
                    HidingSpot[] allSpots = FindObjectsOfType<HidingSpot>();
                    foreach (HidingSpot spot in allSpots)
                    {
                        spot.SetPlayer(player);
                        spot.ToggleHide();
                        break;
                    }
                }
                else
                {
                    nearbyHidingSpot = FindClosestHidingSpot();
                    if (nearbyHidingSpot != null)
                    {
                        nearbyHidingSpot.SetPlayer(player);
                        nearbyHidingSpot.ToggleHide();
                    }
                }
                break;
        }
    }

    HidingSpot FindClosestHidingSpot()
    {
        HidingSpot[] spots = FindObjectsOfType<HidingSpot>();
        HidingSpot closest = null;
        float bestDistance = hideSearchRadius;

        foreach (HidingSpot spot in spots)
        {
            float distance = Vector3.Distance(player.transform.position, spot.transform.position);
            if (distance <= bestDistance)
            {
                bestDistance = distance;
                closest = spot;
            }
        }
        return closest;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (actionType == ActionType.Sprint && player != null)
            player.SetSprint(false);
    }
}