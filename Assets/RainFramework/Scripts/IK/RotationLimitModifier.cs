using UnityEngine;

public abstract class RotationLimitModifier: MonoBehaviour
{
    public abstract Quaternion GetLimitedAngle(Quaternion desiredRotation, out bool Limited);
}
