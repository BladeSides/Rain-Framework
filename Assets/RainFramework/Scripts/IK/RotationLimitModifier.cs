using UnityEngine;

public abstract class RotationLimitModifier: MonoBehaviour
{
    public abstract Quaternion ApplyLimitedAngle(Quaternion desiredRotation, Vector3 endPosition, out bool Limited);
}
