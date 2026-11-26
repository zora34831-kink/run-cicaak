using UnityEngine;
using UnityEngine.EventSystems;

public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("References")]
    public RectTransform background;
    public RectTransform handle;
    public PlayerController player;
    [Range(0.2f, 1f)] public float handleRange = 0.65f;

    private Vector2 input;

    void Awake()
    {
        if (background == null) background = transform as RectTransform;
        if (player == null) player = FindObjectOfType<PlayerController>();
    }

    public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

    public void OnDrag(PointerEventData eventData)
    {
        if (background == null || handle == null) return;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background, eventData.position, eventData.pressEventCamera, out Vector2 localPoint)) return;

        Vector2 halfSize = background.rect.size * 0.5f;
        input = new Vector2(localPoint.x / halfSize.x, localPoint.y / halfSize.y);
        input = Vector2.ClampMagnitude(input, 1f);
        handle.anchoredPosition = input * (Mathf.Min(halfSize.x, halfSize.y) * handleRange);
        if (player != null) player.SetMoveInput(input);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        input = Vector2.zero;
        if (handle != null) handle.anchoredPosition = Vector2.zero;
        if (player != null) player.SetMoveInput(Vector2.zero);
    }
}