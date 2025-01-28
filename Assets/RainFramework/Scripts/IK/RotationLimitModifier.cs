using UnityEngine;

public abstract class RotationLimitModifier: MonoBehaviour
{
    public abstract void ApplyRotationConstraints(out bool isLimited);
    
    public abstract void ApplyAngleLimit(ref Quaternion swingRotation, out bool isLimited);
}
