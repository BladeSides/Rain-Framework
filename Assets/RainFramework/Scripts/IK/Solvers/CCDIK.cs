using System;
using UnityEngine;
using RainFramework.Math;
public class CCDIK: IKSolver
{
    public float DistanceTolerance = 0.01f;
    public float AngleTolerance = 3f;
    public float TimeToReachEndPosition = 1f;
    
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
        if ((Bones[Bones.Count - 1].EndTransform.position - TargetTransform.position).sqrMagnitude < DistanceTolerance)
        {
            return;
        }

        // Iterate from the end effector back to the root
        for (int i = Bones.Count - 1; i >= 0; i--)
        {
            Vector3 effectorPosition = Bones[Bones.Count - 1].EndTransform.position;
            
            //CCD-IK Algorithm: https://zalo.github.io/blog/inverse-kinematics/
            Vector3 directionToEffector = effectorPosition - Bones[i].StartTransform.position;
            Vector3 directionToTarget = TargetTransform.position - Bones[i].StartTransform.position;
            
            Quaternion rotation = Quaternion.FromToRotation(directionToEffector, directionToTarget);

            
            Bones[i].StartTransform.rotation = 
                MathFunctions.SlerpSmooth(Bones[i].StartTransform.rotation,
                    rotation * Bones[i].StartTransform.rotation, Time.deltaTime, TimeToReachEndPosition, 0.01f);
            
            if (Bones[i].StartTransform.TryGetComponent<RotationLimitModifier>(out var rotationLimitModifier))
            {
                rotationLimitModifier.ApplyRotationConstraints(out bool limited, AngleTolerance);
            }

            Bones[i].StartTransform.rotation = Quaternion.Normalize(Bones[i].StartTransform.rotation);
        }    
    }
}
