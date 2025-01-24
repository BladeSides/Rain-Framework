using System;
using UnityEngine;

public class CCDIK: IKSolver
{
    public override void Awake()
    {
        base.Awake();
    }

    public override void UpdateIK(int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            CCDStep();
        }
    }

    private void CCDStep()
    {
        // Iterate from the end effector back to the root
        for (int i = Bones.Count - 1; i >= 0; i--)
        {
            Vector3 effectorPosition = Bones[Bones.Count - 1].EndTransform.position;
            
            //CCD-IK Algorithm: https://zalo.github.io/blog/inverse-kinematics/
            Vector3 directionToEffector = effectorPosition - Bones[i].StartTransform.position;
            Vector3 directionToTarget = TargetTransform.position - Bones[i].StartTransform.position;
            
            Quaternion rotation = Quaternion.FromToRotation(directionToEffector, directionToTarget);

            
            Bones[i].StartTransform.rotation = rotation * Bones[i].StartTransform.rotation;

            if (Bones[i].StartTransform.TryGetComponent<RotationLimitModifier>(out var rotationLimitModifier))
            {
                rotationLimitModifier.ApplyLimitedAngle(rotation, Bones[i].EndTransform.position, out bool limited);
            }

            Bones[i].StartTransform.rotation = Quaternion.Normalize(Bones[i].StartTransform.rotation);
        }    
    }
}
