using System;
using UnityEngine;

public class CCDIK: IKSolver
{
    public override void Awake()
    {
        base.Awake();
    }

    public override void UpdateIK()
    {
        // Iterate from the end effector back to the root
        for (int i = Bones.Count - 1; i >= 0; i--)
        {
            Vector3 effectorPosition = Bones[Bones.Count - 1].EndTransform.position;
            
            //CCD-IK Algorithm: https://zalo.github.io/blog/inverse-kinematics/
            Vector3 directionToEffector = effectorPosition - Bones[i].StartTransform.position;
            Vector3 directionToTarget = TargetTransform.position - Bones[i].StartTransform.position;

            Quaternion rotation = Quaternion.FromToRotation(directionToEffector, directionToTarget);
            
            if (Bones[i].StartTransform.TryGetComponent<RotationLimitModifier>(out var rotationLimitModifier))
            {
                rotation = rotationLimitModifier.GetLimitedAngle(rotation, out bool limited);
                if (limited)
                {
                    Bones[i].StartTransform.localRotation = rotation;
                }
                else
                {
                    Bones[i].StartTransform.localRotation = rotation * Bones[i].StartTransform.localRotation;
                }
            }
            else
            {
                Bones[i].StartTransform.localRotation = rotation * Bones[i].StartTransform.localRotation;
            }
            
            // Immediately clamp after applying the rotation
            if (Bones[i].StartTransform.TryGetComponent<RotationLimitModifier>(out var rotationLimitModifierAfter))
            {
                // Recheck the rotation after applying and clamp immediately
                Bones[i].StartTransform.localRotation = rotationLimitModifierAfter.GetLimitedAngle(Bones[i].StartTransform.localRotation, out bool _);
            }
        }

    }
}
