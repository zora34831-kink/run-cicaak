using UnityEngine;

public class HumanProceduralAnimation : MonoBehaviour
{
    public HumanChaserAI chaser;
    public Transform leftArm;
    public Transform rightArm;
    public Transform torso;
    public float armSwingDegrees = 28f;
    public float cycleSpeed = 8f;

    private Quaternion leftStart, rightStart;
    private Vector3 torsoStart;

    void Awake()
    {
        if (chaser == null) chaser = GetComponent<HumanChaserAI>();
        if (leftArm != null) leftStart = leftArm.localRotation;
        if (rightArm != null) rightStart = rightArm.localRotation;
        if (torso != null) torsoStart = torso.localPosition;
    }

    void LateUpdate()
    {
        if (chaser == null) return;
        float swing = chaser.IsMoving ? Mathf.Sin(Time.time * cycleSpeed) * armSwingDegrees : 0f;

        if (leftArm != null) leftArm.localRotation = leftStart * Quaternion.Euler(swing, 0f, 0f);
        if (rightArm != null) rightArm.localRotation = rightStart * Quaternion.Euler(-swing, 0f, 0f);

        if (torso != null)
        {
            float bob = chaser.IsMoving ? Mathf.Abs(Mathf.Sin(Time.time * cycleSpeed)) * 0.025f : 0f;
            torso.localPosition = torsoStart + Vector3.up * bob;
        }
    }
}