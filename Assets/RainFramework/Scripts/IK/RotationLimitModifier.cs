using UnityEngine;

public abstract class RotationLimitModifier: MonoBehaviour
{
    public abstract Quaternion GetLimitedAngle(Quaternion desiredRotation, Vector3 endPosition, out bool Limited);
}
