using UnityEngine;

[DisallowMultipleComponent]
public class BallJoint : RotationLimitModifier
{
    [Tooltip("Local rotation axis around which twisting is allowed")]
    public Vector3 twistAxis = Vector3.forward;

    [Tooltip("Maximum swing angle (cone angle) in degrees")]
    [Range(0, 180)] public float swingLimit = 45f;

    [Tooltip("Maximum twist angle in degrees")]
    [Range(0, 180)] public float twistLimit = 45f;

    private Quaternion m_InitialRotation;
    private Vector3 m_NormalizedAxis;

    void Start()
    {
        m_InitialRotation = transform.localRotation;
        m_NormalizedAxis = twistAxis.normalized;
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

    private void ClampRotation(ref Quaternion swing, ref Quaternion twist)
    {
        // Clamp twist rotation
        float twistAngle;
        Vector3 calculatedAxis;
        twist.ToAngleAxis(out twistAngle, out calculatedAxis);

        // Adjust angle sign based on axis direction
        float dot = Vector3.Dot(calculatedAxis, m_NormalizedAxis);
        if (dot < 0)
        {
            twistAngle *= -1;
            calculatedAxis *= -1;
        }

        twistAngle = NormalizeAngle(twistAngle);
        float clampedTwist = Mathf.Clamp(twistAngle, -twistLimit, twistLimit);
        twist = Quaternion.AngleAxis(clampedTwist, m_NormalizedAxis);

        // Clamp swing rotation
        float swingAngle = Quaternion.Angle(Quaternion.identity, swing);
        if (swingAngle > swingLimit)
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

    public override void ApplyRotationConstraints(out bool isLimited)
    {
        // Calculate delta rotation from initial orientation
        Quaternion currentRotation = transform.localRotation;
        Quaternion deltaRotation = currentRotation * Quaternion.Inverse(m_InitialRotation);

        // Decompose into swing and twist components
        DecomposeSwingTwist(deltaRotation, m_NormalizedAxis, out Quaternion originalSwing, out Quaternion originalTwist);

        // Store original angles
        float originalSwingAngle = Quaternion.Angle(Quaternion.identity, originalSwing);
        originalTwist.ToAngleAxis(out float originalTwistAngle, out Vector3 _);
        originalTwistAngle = Mathf.Abs(NormalizeAngle(originalTwistAngle));

        // Apply angle limits to copies
        Quaternion clampedSwing = originalSwing;
        Quaternion clampedTwist = originalTwist;
        ClampRotation(ref clampedSwing, ref clampedTwist);

        // Check if limits were applied
        isLimited = originalSwingAngle > swingLimit || originalTwistAngle > twistLimit;

        // Recompose and apply clamped rotation
        transform.localRotation = m_InitialRotation * (clampedSwing * clampedTwist);
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
        Quaternion initialRotation = Application.isPlaying ? m_InitialRotation : transform.localRotation;
        Quaternion parentRotation = transform.parent != null ? transform.parent.rotation : Quaternion.identity;
        Quaternion worldRotation = parentRotation * initialRotation;

        // Calculate world space axis using initial orientation
        Vector3 worldAxis = worldRotation * twistAxis.normalized;
        Vector3 position = transform.position;

        // Draw main axis
        Gizmos.color = Color.red;
        Gizmos.DrawLine(position, position + worldAxis * gizmoSize);

        // Draw swing cone
        DrawSwingCone(position, worldAxis, worldRotation);

        // Draw twist arc
        DrawTwistArc(position, worldAxis);
    }

    private void DrawSwingCone(Vector3 position, Vector3 axis, Quaternion worldRotation)
    {
        if (swingLimit <= 0) return;

        Gizmos.color = Color.cyan;
        int segments = 36;
        float angleStep = 360f / segments;

        // Create rotation basis aligned with initial orientation
        Vector3 right = worldRotation * Vector3.right;

        if (Mathf.Abs(Vector3.Dot(Vector3.right.normalized, twistAxis.normalized)) > 0.9f)
        {
            right = worldRotation * Vector3.up;
        }

        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep;
            Vector3 dir = Quaternion.AngleAxis(angle, axis) * Quaternion.AngleAxis(swingLimit, right) * axis;
            Vector3 nextDir = Quaternion.AngleAxis((i+1)*angleStep, axis) * Quaternion.AngleAxis(swingLimit, right) * axis;
            
            Gizmos.DrawLine(position, position + dir * gizmoSize);
            Gizmos.DrawLine(position + dir * gizmoSize, position + nextDir * gizmoSize);
            
            if (i==0 || i==segments-1)
                Gizmos.DrawLine(position, position + nextDir * gizmoSize);
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