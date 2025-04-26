using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class BallJoint : RotationLimitModifier
{
    [FormerlySerializedAs("twistAxis")] [Tooltip("Local rotation axis around which twisting is allowed")]
    public Vector3 rotationAxis = Vector3.forward;

    [FormerlySerializedAs("overridenRotationAxis")] [FormerlySerializedAs("overrideTwistAxis")] [Tooltip("Local starting position of bone on the twist axis")]
    public Vector3 overridenSwingAxis = Vector3.forward;

    [FormerlySerializedAs("OverrideRotationAxis")] [FormerlySerializedAs("OverrideStartingTwistAxis")] public bool OverrideStartingSwingAxis = false;

    [Tooltip("Maximum swing angle (cone angle) in degrees")]
    [Range(0, 180)] public float swingLimit = 45f;

    [Tooltip("Maximum twist angle in degrees")]
    [Range(0, 180)] public float twistLimit = 45f;

    private Quaternion InitialRotation;
    public Quaternion overridenSwingAxisDifference = Quaternion.identity;
    private Vector3 NormalizedAxis;

    void Start()
    {
        InitialRotation = transform.localRotation;
        NormalizedAxis = rotationAxis.normalized;
        if (OverrideStartingSwingAxis)
        {
            if (Vector3.Angle(overridenSwingAxis, rotationAxis) > swingLimit)
            {
                Debug.LogWarning("Overriden swing axis is not within the allowed swing limit.");
            }

            overridenSwingAxis = overridenSwingAxis.normalized;
            overridenSwingAxisDifference = Quaternion.FromToRotation(overridenSwingAxis, rotationAxis);
        }
    }

    private void DecomposeSwingTwist(Quaternion q, Vector3 axis, out Quaternion swing, out Quaternion twist)
    {
        Vector3 normalizedAxis = axis.normalized;
        Vector3 qv = new Vector3(q.x, q.y, q.z);
        
        // Project vector part onto twist axis
        float projectionScalar = Vector3.Dot(qv, normalizedAxis);
        Vector3 twistVector = projectionScalar * normalizedAxis;
        
        // Calculate twist quaternion
        twist = new Quaternion(twistVector.x, twistVector.y, twistVector.z, q.w);
        float twistLength = Mathf.Sqrt(twistVector.sqrMagnitude + q.w * q.w);
        if (twistLength > Mathf.Epsilon)
        {
            float invTwistLength = 1f / twistLength;
            twist.x *= invTwistLength;
            twist.y *= invTwistLength;
            twist.z *= invTwistLength;
            twist.w *= invTwistLength;
        }
        else
        {
            twist = Quaternion.identity;
        }
        
        swing = q * Quaternion.Inverse(twist);
    }
    
    /*private void DecomposeSwingTwist(Quaternion q, Vector3 axis, out Quaternion swing, out Quaternion twist)
    {
        // Ensure the twist axis is normalized
        twistAxis.Normalize();
        Vector3 rotationAxis = new Vector3(q.x, q.y, q.z);
    
        // Project rotation axis onto the twist axis
        Vector3 twistProjection = Vector3.Dot(rotationAxis, twistAxis) * twistAxis;
    
        // Reconstruct twist quaternion
        twist = new Quaternion(twistProjection.x, twistProjection.y, twistProjection.z, q.w);
        twist = NormalizeQuaternion(twist); // Handle normalization
    
        // Swing = rotation * inverse(twist)
        swing = q * Quaternion.Inverse(twist);
    }
    */

    private Quaternion NormalizeQuaternion(Quaternion q)
    {
        float magnitude = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
        if (magnitude < Mathf.Epsilon) return Quaternion.identity;
        return new Quaternion(q.x/magnitude, q.y/magnitude, q.z/magnitude, q.w/magnitude);
    }

    private void ClampRotation(ref Quaternion swing, ref Quaternion twist, float angleTolerance)
    {
        // Clamp twist rotation
        float twistAngle;
        Vector3 calculatedAxis;
        twist.ToAngleAxis(out twistAngle, out calculatedAxis);

        // Adjust angle sign based on axis direction
        float dot = Vector3.Dot(calculatedAxis, NormalizedAxis);
        if (dot < 0)
        {
            twistAngle *= -1;
            calculatedAxis *= -1;
        }

        twistAngle = NormalizeAngle(twistAngle);
        float clampedTwist = twistAngle;
        if (Mathf.Abs(twistAngle) > angleTolerance + twistLimit)
        {
            clampedTwist = Mathf.Clamp(twistAngle, -twistLimit, twistLimit);
        }

        twist = Quaternion.AngleAxis(clampedTwist, NormalizedAxis);

        // Clamp swing rotation, if override swing axis, use the initial calculated difference
        float swingAngle = Quaternion.Angle(OverrideStartingSwingAxis ? overridenSwingAxisDifference : Quaternion.identity, swing);
        if (swingAngle > swingLimit + angleTolerance)
        {
            float t = swingLimit / swingAngle;
            swing = Quaternion.Slerp(Quaternion.identity, swing, t);
        }
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }

    public override void ApplyRotationConstraints(out bool isLimited, float angleTolerance)
    {
        // Calculate delta rotation from initial orientation
        Quaternion currentRotation = transform.localRotation;
        Quaternion deltaRotation = currentRotation * Quaternion.Inverse(InitialRotation);

        // Decompose into swing and twist components
        DecomposeSwingTwist(deltaRotation, NormalizedAxis, out Quaternion originalSwing, out Quaternion originalTwist);

        // Store original angles
        float originalSwingAngle = Quaternion.Angle(Quaternion.identity, originalSwing);
        originalTwist.ToAngleAxis(out float originalTwistAngle, out Vector3 _);
        originalTwistAngle = Mathf.Abs(NormalizeAngle(originalTwistAngle));

        // Apply angle limits to copies
        Quaternion clampedSwing = originalSwing;
        Quaternion clampedTwist = originalTwist;
        ClampRotation(ref clampedSwing, ref clampedTwist, angleTolerance);

        // Check if limits were applied
        isLimited = originalSwingAngle > swingLimit * angleTolerance || originalTwistAngle > twistLimit * angleTolerance;

        // Recompose and apply clamped rotation
        transform.localRotation = InitialRotation * (clampedSwing * clampedTwist);
    }
    
    [Header("Visualization")]
    [Tooltip("Whether to draw limits in the editor")]
    public bool drawGizmos = true;
    
    [Tooltip("Size of gizmo elements")]
    public float gizmoSize = 1f;
    
    [Tooltip("Radius for twist arc visualization")]
    public float twistArcRadius = 0.3f;

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        // Get initial rotation state
        Quaternion initialRotation = Application.isPlaying ? InitialRotation : transform.localRotation;
        Quaternion parentRotation = transform.parent != null ? transform.parent.rotation : Quaternion.identity;
        Quaternion worldRotation = parentRotation * initialRotation;

        // Calculate world space axis using initial orientation
        Vector3 worldAxis = worldRotation * rotationAxis.normalized;
        Vector3 position = transform.position;

        // Draw main axis
        Gizmos.color = Color.red;
        Gizmos.DrawLine(position, position + worldAxis * gizmoSize);
        
        // Draw overridden twist axis, if enabled
        if (OverrideStartingSwingAxis)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(position, position + worldRotation * overridenSwingAxis.normalized * gizmoSize);
        }
        

        // Draw swing cone
        DrawSwingCone(position, worldAxis, worldRotation);

        // Draw twist arc
        DrawTwistArc(position, worldAxis);
    }

    private void DrawSwingCone(Vector3 position, Vector3 axis, Quaternion worldRotation)
    {
        if (swingLimit <= 0) return;

        Gizmos.color = Color.cyan;

        int segments = 36; // smoothness
        float angleStep = 360f / segments;

        // Find a stable orthogonal vector
        Vector3 ortho = OrthoNormalVector(axis.normalized);
        Vector3 basis = Vector3.Cross(axis, ortho).normalized;
        Vector3 up = Vector3.Cross(basis, axis).normalized;

        Vector3 prevEnd = Vector3.zero; // <--- added this!

        for (int i = 0; i <= segments; i++)
        {
            float angleAroundAxis = i * angleStep;
            Quaternion rotAroundAxis = Quaternion.AngleAxis(angleAroundAxis, axis);

            Vector3 swingDirection = Quaternion.AngleAxis(swingLimit, basis) * axis;
            swingDirection = rotAroundAxis * swingDirection;
        
            Vector3 worldDir = worldRotation * swingDirection;

            Vector3 start = position;
            Vector3 end = position + worldDir * gizmoSize;

            // Draw radial lines
            Gizmos.DrawLine(start, end);

            // Optionally connect the rim (arc lines)
            if (i > 0)
            {
                Gizmos.DrawLine(prevEnd, end);
            }

            prevEnd = end;
        }
    }

// Support function
    private Vector3 OrthoNormalVector(Vector3 v)
    {
        if (Mathf.Abs(v.x) < Mathf.Abs(v.y))
        {
            if (Mathf.Abs(v.x) < Mathf.Abs(v.z))
                return Vector3.Cross(v, Vector3.right);
            else
                return Vector3.Cross(v, Vector3.forward);
        }
        else
        {
            if (Mathf.Abs(v.y) < Mathf.Abs(v.z))
                return Vector3.Cross(v, Vector3.up);
            else
                return Vector3.Cross(v, Vector3.forward);
        }
    }


    private void DrawTwistArc(Vector3 position, Vector3 axis)
    {
        if (twistLimit <= 0) return;

        Gizmos.color = Color.yellow;
        int segments = 20;
        float angleStep = (2 * twistLimit) / segments;

        // Find perpendicular vector
        Vector3 perpendicular = Vector3.Cross(axis, Vector3.up).normalized;
        if (perpendicular.magnitude < Mathf.Epsilon)
            perpendicular = Vector3.Cross(axis, Vector3.right).normalized;

        Vector3 prevPoint = position + Quaternion.AngleAxis(-twistLimit, axis) * perpendicular * twistArcRadius;
        
        for (int i = 0; i <= segments; i++)
        {
            float angle = -twistLimit + i * angleStep;
            Vector3 point = position + Quaternion.AngleAxis(angle, axis) * perpendicular * twistArcRadius;
            if (i > 0) Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
    
}