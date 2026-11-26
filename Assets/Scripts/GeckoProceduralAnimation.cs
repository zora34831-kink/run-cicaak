using UnityEngine;

public class GeckoProceduralAnimation : MonoBehaviour
{
    public PlayerController player;
    public Transform body;
    public Transform frontLeftLeg;
    public Transform frontRightLeg;
    public Transform backLeftLeg;
    public Transform backRightLeg;
    public Transform[] tailSegments;

    public float walkCycleSpeed = 10f;
    public float sprintCycleSpeed = 16f;
    public float legSwingDegrees = 24f;
    public float tailSwayDegrees = 18f;
    public float bodyBobHeight = 0.035f;

    private Vector3 bodyStartPosition;
    private Quaternion frontLeftStart, frontRightStart, backLeftStart, backRightStart;
    private Quaternion[] tailStartRotations;

    void Awake()
    {
        if (player == null) player = GetComponent<PlayerController>();
        if (body != null) bodyStartPosition = body.localPosition;

        if (frontLeftLeg != null) frontLeftStart = frontLeftLeg.localRotation;
        if (frontRightLeg != null) frontRightStart = frontRightLeg.localRotation;
        if (backLeftLeg != null) backLeftStart = backLeftLeg.localRotation;
        if (backRightLeg != null) backRightStart = backRightLeg.localRotation;

        if (tailSegments != null)
        {
            tailStartRotations = new Quaternion[tailSegments.Length];
            for (int i = 0; i < tailSegments.Length; i++)
                if (tailSegments[i] != null) tailStartRotations[i] = tailSegments[i].localRotation;
        }
    }

    void LateUpdate()
    {
        if (player == null) return;
        float movement = player.MoveAmount;
        bool moving = movement > 0.05f && !player.IsHidden;
        float cycleSpeed = player.IsSprinting ? sprintCycleSpeed : walkCycleSpeed;
        float phase = Time.time * cycleSpeed;

        if (body != null)
        {
            float bob = moving ? Mathf.Abs(Mathf.Sin(phase)) * bodyBobHeight : 0f;
            body.localPosition = bodyStartPosition + Vector3.up * bob;
        }

        float swing = moving ? Mathf.Sin(phase) * legSwingDegrees : 0f;
        SetLeg(frontLeftLeg, frontLeftStart, swing);
        SetLeg(backRightLeg, backRightStart, swing);
        SetLeg(frontRightLeg, frontRightStart, -swing);
        SetLeg(backLeftLeg, backLeftStart, -swing);

        if (tailSegments != null)
        {
            for (int i = 0; i < tailSegments.Length; i++)
            {
                if (tailSegments[i] == null) continue;
                float sway = Mathf.Sin(phase * 0.65f - i * 0.45f) * tailSwayDegrees * (moving ? 1f : 0.25f);
                tailSegments[i].localRotation = tailStartRotations[i] * Quaternion.Euler(0f, sway, 0f);
            }
        }
    }

    void SetLeg(Transform leg, Quaternion start, float angle)
    {
        if (leg != null) leg.localRotation = start * Quaternion.Euler(angle, 0f, 0f);
    }
}