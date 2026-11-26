using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class GameplayHint : MonoBehaviour
{
    public float visibleSeconds = 9f;
    public float fadeSeconds = 2f;

    private Text hint;
    private float elapsed;

    void Awake()
    {
        hint = GetComponent<Text>();
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed <= visibleSeconds) return;

        float alpha = fadeSeconds <= 0f ? 0f : 1f - Mathf.Clamp01((elapsed - visibleSeconds) / fadeSeconds);
        Color color = hint.color;
        color.a = alpha;
        hint.color = color;

        if (alpha <= 0f) gameObject.SetActive(false);
    }
}