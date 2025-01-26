using UnityEngine;

public abstract class RotationLimitModifier: MonoBehaviour
{
    public abstract void ApplyRotationConstraints(out bool isLimited);
    
    public abstract void ApplyAngleLimit(out bool isLimited);
}
