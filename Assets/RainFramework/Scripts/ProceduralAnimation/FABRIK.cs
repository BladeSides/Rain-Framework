using System.Collections.Generic;
using UnityEngine;

public class FABRIK : IKBase
{
    public Transform Target;
    public Transform Pole;
    private Vector3[] _positions;
    
    public float PositionDelta = 0.01f;

    public override void Init()
    {
        _positions = new Vector3[Joints.Count];
    }
    public override void Iterate()
    {
        if (Target == null)
        {
            Debug.Log("No target set for FABRIK.");
            return;
        }

        for (int i = 0; i < Joints.Count; i++)
        {
            // Cache current pos
            _positions[i] = Joints[i].transform.position;
        }

        // Full Stretch if impossible to reach
        if (Vector3.SqrMagnitude(Target.transform.position - Joints[0].transform.position) >= TotalLength * TotalLength)
        {
            var direction = (Target.position - _positions[0]).normalized;

            for (int i = 1; i < _positions.Length; i++)
            {
                _positions[i] = _positions[i - 1] + direction * Bones[i - 1].BoneLength;
            }
        }

        // If position outside delta
        else if ((_positions[_positions.Length - 1] - Target.position).sqrMagnitude > PositionDelta * PositionDelta)
        {
            // Back
            for (int i = _positions.Length - 1; i > 0; i--)
            {
                if (i == _positions.Length - 1)
                {
                    _positions[i] = Target.position; // Set last position to target position, immediately
                }
                else
                {
                    _positions[i] = _positions[i + 1] + (_positions[i] - _positions[i + 1]).normalized * Bones[i].BoneLength;
                }
            }
            //Forward
            for (int i = 1; i < _positions.Length; i++)
            {
                _positions[i] = _positions[i - 1] + (_positions[i] - _positions[i - 1]).normalized * Bones[i - 1].BoneLength;
            }                
        }

        // Pole
        if (Pole != null)
        {
            for (int i = 1; i < _positions.Length - 1; i++)
            {
                var plane = new Plane(_positions[i + 1] - _positions[i - 1], _positions[i - 1]);
                var projectedPole = plane.ClosestPointOnPlane(Pole.position);
                var projectedBone = plane.ClosestPointOnPlane(_positions[i]);
                var angle = Vector3.SignedAngle(projectedBone - _positions[i - 1], projectedPole - _positions[i - 1], plane.normal);
                _positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (_positions[i] - _positions[i - 1]) + _positions[i - 1];
            }
        }

        // Apply positions
        for (int i = 0; i < Joints.Count; i++)
        {
            Joints[i].transform.position = _positions[i];
        }

        //Solve Rotations
        SolveRotations();

    }

    public void SolveRotations()
    {
        for (int i = 0; i < Joints.Count - 1; i++)
        {
            Joint j = Joints[i];

            Vector3 from = Joints[i].transform.position;
            Vector3 to = Joints[i + 1].transform.position;

            Vector3 targetWorldForward = (to - from).normalized;

            // Current forward based on joint's local forward axis
            Vector3 currentWorldForward = j.transform.TransformDirection(j.LocalForwardAxis);

            // Rotation required to match the vectors
            Quaternion delta = Quaternion.FromToRotation(currentWorldForward, targetWorldForward);

            // Aapply
            j.transform.rotation = delta * j.transform.rotation;
        }
    }

}