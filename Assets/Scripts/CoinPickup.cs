using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CoinPickup : MonoBehaviour
{
    public int coinValue = 1;
    public int xpValue = 5;
    public float rotateSpeed = 100f;
    public float bobHeight = 0.15f;
    public float bobSpeed = 2f;

    private Vector3 startPosition;
    private bool collected;

    void Awake()
    {
        startPosition = transform.position;
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Update()
    {
        if (collected) return;
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
        transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected || !other.GetComponent<PlayerController>()) return;
        collected = true;
        GameManager.Instance?.AddCoins(coinValue);
        ProgressionManager.Instance?.AddXP(xpValue);
        RunCicaakAudio.PlayCoin();
        Destroy(gameObject);
    }
}
