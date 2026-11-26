using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EscapeZone : MonoBehaviour
{
    public string promptLabel = "ESCAPE";
    private bool completed;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (completed || other.GetComponent<PlayerController>() == null) return;
        completed = true;
        GameManager.Instance?.CompleteEscape();
    }
}