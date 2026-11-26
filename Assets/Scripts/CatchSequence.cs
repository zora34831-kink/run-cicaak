using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerController))]
public class CatchSequence : MonoBehaviour
{
    public float launchDuration = 0.9f;
    public float launchHeight = 2.2f;
    public float launchForward = 1.8f;
    public float spinDegrees = 360f;

    private bool playing;

    public void Play()
    {
        if (!playing) StartCoroutine(Launch());
    }

    IEnumerator Launch()
    {
        playing = true;
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float elapsed = 0f;

        while (elapsed < launchDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / launchDuration);
            float arc = Mathf.Sin(t * Mathf.PI);
            transform.position = startPosition
                + Vector3.up * (arc * launchHeight)
                + transform.forward * (t * launchForward);
            transform.rotation = startRotation * Quaternion.Euler(0f, 0f, spinDegrees * t);
            yield return null;
        }

        if (controller != null) controller.enabled = true;
        GameManager.Instance?.EndRun();
    }
}