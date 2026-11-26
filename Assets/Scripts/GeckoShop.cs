using UnityEngine;
using System;

[Serializable]
public class GeckoSkin
{
    public string id;
    public string displayName;
    public int price;
    public Color color = Color.green;
}

public class GeckoShop : MonoBehaviour
{
    public static GeckoShop Instance { get; private set; }

    public GeckoSkin[] skins = new GeckoSkin[]
    {
        new GeckoSkin { id = "classic", displayName = "Classic", price = 0, color = new Color(0.25f, 0.72f, 0.28f) },
        new GeckoSkin { id = "forest", displayName = "Forest", price = 150, color = new Color(0.08f, 0.38f, 0.18f) },
        new GeckoSkin { id = "sunset", displayName = "Sunset", price = 300, color = new Color(1f, 0.42f, 0.12f) },
        new GeckoSkin { id = "ocean", displayName = "Ocean", price = 500, color = new Color(0.12f, 0.48f, 0.9f) }
    };

    public string SelectedSkinId => PlayerPrefs.GetString("RUN_CICAAK_SKIN", "classic");

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (PlayerPrefs.GetInt("RUN_CICAAK_SKIN_OWNED_classic", 0) == 0)
        {
            PlayerPrefs.SetInt("RUN_CICAAK_SKIN_OWNED_classic", 1);
            PlayerPrefs.Save();
        }
    }

    public bool IsOwned(string id) => PlayerPrefs.GetInt("RUN_CICAAK_SKIN_OWNED_" + id, 0) == 1;

    public bool PurchaseOrSelect(string id)
    {
        GeckoSkin skin = Array.Find(skins, item => item.id == id);
        if (skin == null) return false;

        if (!IsOwned(id))
        {
            if (GameManager.Instance != null)
            {
                if (!GameManager.Instance.SpendCoins(skin.price)) return false;
            }
            else
            {
                int wallet = PlayerPrefs.GetInt("RUN_CICAAK_WALLET", 0);
                if (wallet < skin.price) return false;
                PlayerPrefs.SetInt("RUN_CICAAK_WALLET", wallet - skin.price);
            }
            PlayerPrefs.SetInt("RUN_CICAAK_SKIN_OWNED_" + id, 1);
        }

        PlayerPrefs.SetString("RUN_CICAAK_SKIN", id);
        PlayerPrefs.Save();
        return true;
    }

    public Color GetSelectedColor()
    {
        GeckoSkin skin = Array.Find(skins, item => item.id == SelectedSkinId);
        return skin != null ? skin.color : Color.green;
    }
}