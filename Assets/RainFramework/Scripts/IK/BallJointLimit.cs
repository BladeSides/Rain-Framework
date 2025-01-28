using System;
using System.Numerics;
using UnityEditor;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class BallJointLimit : RotationLimitModifier
{
    [SerializeField]
    private Vector3 _rotationAxis = Vector3.forward;

    private Vector3 _arbitraryAxis
    {
        get
        {
            return new Vector3(_orientedRotationAxis.y, _orientedRotationAxis.z, _orientedRotationAxis.x);
        }
    }
    
    private Quaternion _cachedLocalRotation;

    private Quaternion _cachedParentRotationAxisDifference;
    private Quaternion _cachedParentArbitraryAxisDifference;
    
    private Vector3 _orientedRotationAxis;

    private bool _initialized;

    [SerializeField, Range(0,180)]
    private float _angleLimit = 90.0f;
    
    [SerializeField, Range(0,180)]
    private float twistLimit = 90;

    private void Awake()
    {
        //force initialize for testing
        Initialize();
        if (!_initialized)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
     _cachedLocalRotation = transform.localRotation;
    
    // Initial oriented rotation axis should be aligned with the parent
    Vector3 parentForwardAxis = Vector3.Normalize(transform.position - transform.parent.position);

    // maybe?
    Vector3 parentUpAxis = transform.parent.up;
    
    _cachedParentRotationAxisDifference = Quaternion.FromToRotation(parentForwardAxis, _cachedLocalRotation * _rotationAxis);
    _cachedParentArbitraryAxisDifference = Quaternion.FromToRotation(parentUpAxis, _cachedLocalRotation * _arbitraryAxis);
    
    _initialized = true;

    }
    private void DecomposeRotation(Quaternion rotation, out Quaternion swing, out Quaternion twist)
    {
        Vector3 twistAxis = _orientedRotationAxis.normalized;

        // Project rotation's axis onto the twist axis
        Vector3 rotationAxis = new Vector3(rotation.x, rotation.y, rotation.z).normalized;
        float dot = Vector3.Dot(rotationAxis, twistAxis);

        // Calculate twist quaternion (ensure it's purely around twistAxis)
        float twistAngle = 2.0f * Mathf.Acos(rotation.w) * Mathf.Rad2Deg;
        float sign = Mathf.Sign(dot);
        twistAngle *= sign;

        // Rebuild twist quaternion using twistAxis and the corrected angle
        twist = Quaternion.AngleAxis(twistAngle, twistAxis);

        // Swing is the remaining rotation after removing twist
        swing = Quaternion.Inverse(twist) * rotation;

        // Ensure swing doesn't contain twist components
        swing.Normalize();
    }

    public override void ApplyRotationConstraints(out bool isLimited)
    {
        // Get delta rotation from initial pose
        Quaternion deltaRotation = transform.localRotation * Quaternion.Inverse(_cachedLocalRotation);

        CalculateOrientedRotationAxis();
        
        // Decompose properly
        Quaternion swing, twist;
        DecomposeRotation(deltaRotation, out swing, out twist);
    
        // Apply limits
        ApplyAngleLimit(ref swing, out bool angleLimited);
        ApplyTwistLimit(ref twist, out bool twistLimited);
    
        // Corrected composition order: twist followed by swing
        transform.localRotation = _cachedLocalRotation * twist * swing;
    
        isLimited = angleLimited || twistLimited;
    }

    private void CalculateOrientedRotationAxis()
    {
        // Get the current rotation of the parent
        Vector3 currentParentForwardAxis = Vector3.Normalize(transform.position - transform.parent.position);
        
        // Calculate the oriented rotation axis
        _orientedRotationAxis = _cachedParentRotationAxisDifference * currentParentForwardAxis;
    }
    
    private void ApplyTwistLimit(ref Quaternion twistRotation, out bool isLimited)
    {
        isLimited = false;
        if (twistLimit >= 180f) return;

        // Extract angle directly from the twist quaternion
        Vector3 twistAxis;
        float angle;
        twistRotation.ToAngleAxis(out angle, out twistAxis);

        // Ensure the axis is aligned with the twist axis (avoid numerical errors)
        float axisAlignment = Vector3.Dot(twistAxis.normalized, _orientedRotationAxis.normalized);
        angle *= Mathf.Sign(axisAlignment); // Adjust sign based on direction

        if (Mathf.Abs(angle) <= twistLimit) return;

        // Clamp angle to the twist limit
        angle = Mathf.Clamp(angle, -twistLimit, twistLimit);
        twistRotation = Quaternion.AngleAxis(angle, _orientedRotationAxis);
        isLimited = true;
    }


    public override void ApplyAngleLimit(ref Quaternion swingRotation, out bool isLimited)
    {
        isLimited = false;
        if (_angleLimit >= 180) return;

        Vector3 currentAxis = swingRotation * _orientedRotationAxis;
        float angle = Vector3.Angle(_orientedRotationAxis, currentAxis);

        if (angle <= _angleLimit) return;

        // Find the limited rotation axis within the cone
        Vector3 limitedAxis = Vector3.RotateTowards(_orientedRotationAxis, currentAxis, _angleLimit * Mathf.Deg2Rad, 0f);
        Quaternion limitedSwing = Quaternion.FromToRotation(_orientedRotationAxis, limitedAxis);

        swingRotation = limitedSwing;
        isLimited = true;
        Debug.Log("Angle Limit Applied");
    }

    public void OnDrawGizmosSelected()
    {
        int coneResolution = 20;
        float coneLength = 1f;

        
        if (Application.isPlaying)
        {
            Debug.DrawLine(transform.position, transform.position + _orientedRotationAxis, Color.red);
            Handles.Label(transform.position + _orientedRotationAxis, "Oriented Rotation Axis");

        }
        else
        {
            Debug.DrawLine(transform.position, transform.position + transform.localRotation * _rotationAxis, Color.red);
            Handles.Label(transform.position + _rotationAxis, "Rotation Axis");
        }
        
        DrawAngleLimit();
        DrawTwistLimit();
    }

private void DrawTwistLimit()
{
    // Clamp twist limit for safety
    twistLimit = Mathf.Clamp(twistLimit, 0, 180);

    if (twistLimit <= 0 || _rotationAxis == Vector3.zero) return;

    // Determine the base axis for drawing
    Vector3 baseAxis = Application.isPlaying ? _orientedRotationAxis.normalized : transform.localRotation * _rotationAxis.normalized;

    // Calculate a perpendicular axis for the twist circle
    Vector3 perpendicularAxis = Vector3.Cross(baseAxis, Vector3.up).normalized;
    if (perpendicularAxis.sqrMagnitude < Mathf.Epsilon) // Handle cases where baseAxis is aligned with Vector3.up
    {
        perpendicularAxis = Vector3.Cross(baseAxis, Vector3.right).normalized;
    }

    // Radius for the twist band
    float bandRadius = 0.5f; // Adjust this for visual clarity
    int circleResolution = 36;

    // Draw two circles for the twist limits
    DrawTwistCircle(baseAxis, perpendicularAxis, bandRadius, twistLimit, Color.yellow, circleResolution);
    DrawTwistCircle(baseAxis, perpendicularAxis, bandRadius, -twistLimit, Color.yellow, circleResolution);

    // Draw connecting lines to indicate the twist range
    Vector3 limitPoint1 = Quaternion.AngleAxis(twistLimit, baseAxis) * perpendicularAxis * bandRadius;
    Vector3 limitPoint2 = Quaternion.AngleAxis(-twistLimit, baseAxis) * perpendicularAxis * bandRadius;

    Debug.DrawLine(transform.position + baseAxis, transform.position + baseAxis + limitPoint1, Color.red);
    Debug.DrawLine(transform.position + baseAxis, transform.position + baseAxis + limitPoint2, Color.red);
}

private void DrawTwistCircle(Vector3 baseAxis, Vector3 perpendicularAxis, float radius, float angle, Color color, int resolution)
{
    Vector3 prevPoint = transform.position + baseAxis + Quaternion.AngleAxis(angle, baseAxis) * (perpendicularAxis * radius);

    for (int i = 1; i <= resolution; i++)
    {
        float segmentAngle = i * 360f / resolution;
        Vector3 rotatedPoint = Quaternion.AngleAxis(segmentAngle, baseAxis) * perpendicularAxis;
        Vector3 currentPoint = transform.position + baseAxis + Quaternion.AngleAxis(angle, baseAxis) * (rotatedPoint * radius);

        Debug.DrawLine(prevPoint, currentPoint, color);
        prevPoint = currentPoint;
    }
}

    private bool DrawAngleLimit()
    {
        Vector3 forwardAxis = Application.isPlaying ? _orientedRotationAxis.normalized : 
            transform.localRotation * _rotationAxis.normalized;
        
        float coneLength = 1f;
        int coneResolution = 20;
        if (forwardAxis == Vector3.zero) return true;

        Vector3 arbitraryAxis = Vector3.Cross(forwardAxis, Vector3.up).normalized;
        if (arbitraryAxis == Vector3.zero)
        {
            arbitraryAxis = Vector3.Cross(forwardAxis, Vector3.right).normalized;
        }

        // Flip the cone if the angle limit is greater than 90 degrees
        if (_angleLimit > 90f)
        {
            forwardAxis = -forwardAxis;
        }

        Vector3 coneTip = transform.position;
        float coneRadius = coneLength * Mathf.Tan(Mathf.Min(_angleLimit, 180 - _angleLimit) * Mathf.Deg2Rad);

        // Draw the cone base
        Vector3 prevPoint = Vector3.zero;
        for (int i = 0; i <= coneResolution; i++)
        {
            float angle = i * 360f / coneResolution;
            Quaternion rotation = Quaternion.AngleAxis(angle, forwardAxis);
            Vector3 pointOnCircle = transform.position + forwardAxis + rotation * (arbitraryAxis * coneRadius);

            Vector3 pointOnCircleDistance = pointOnCircle - transform.position;
            pointOnCircleDistance = pointOnCircleDistance.normalized * coneLength;
            pointOnCircle = transform.position + pointOnCircleDistance;
                
            if (i > 0)
            {
                Debug.DrawLine(prevPoint, pointOnCircle, Color.green); // Circle segment
            }

            prevPoint = pointOnCircle;

            // Draw lines from the base to the cone tip
            if (i < coneResolution)
            {
                Debug.DrawLine(pointOnCircle, coneTip, Color.blue);
            }
        }

        return false;
    }
}
