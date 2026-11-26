using UnityEngine;

public class GeckoSkinApplier : MonoBehaviour
{
    public Renderer[] targetRenderers;

    void Start()
    {
        if (targetRenderers == null || targetRenderers.Length == 0)
            targetRenderers = GetComponentsInChildren<Renderer>();

        Color color = GeckoShop.Instance != null ? GeckoShop.Instance.GetSelectedColor() : Color.green;
        foreach (Renderer item in targetRenderers)
        {
            if (item == null) continue;
            Material material = item.material;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            else if (material.HasProperty("_Color")) material.color = color;
        }
    }
}