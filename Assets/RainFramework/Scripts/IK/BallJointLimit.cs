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
     Quaternion _cachedLocalRotation = transform.localRotation;
    
    // Initial oriented rotation axis should be aligned with the parent
    Vector3 parentForwardAxis = Vector3.Normalize(transform.position - transform.parent.position);

    // maybe?
    Vector3 parentUpAxis = transform.parent.up;
    
    _cachedParentRotationAxisDifference = Quaternion.FromToRotation(parentForwardAxis, _cachedLocalRotation * _rotationAxis);
    _cachedParentArbitraryAxisDifference = Quaternion.FromToRotation(parentUpAxis, _cachedLocalRotation * _arbitraryAxis);
    
    _initialized = true;

    }

    public override void ApplyRotationConstraints(out bool isLimited)
    {
        ApplyAngleLimit(out bool isAngleLimited);
        ApplyTwistLimit(transform.localRotation, out bool isTwistLimited);
        isLimited = isAngleLimited || isTwistLimited;
    }

    public void ApplyTwistLimit(Quaternion localRotation, out bool isLimited)
    {
        isLimited = false;

        // Clamp the twist limit to valid values
        twistLimit = Mathf.Clamp(twistLimit, 0, 180);
        if (twistLimit >= 180) return; // No limit needed

        // Calculate the current twist rotation around the _rotationAxis
        Quaternion twistRotation = Quaternion.FromToRotation(
            _rotationAxis, 
            (localRotation * _rotationAxis).normalized
        );

        // Extract the twist angle
        float twistAngle = Quaternion.Angle(Quaternion.identity, twistRotation);

        // If the twist angle exceeds the limit, clamp it
        if (twistAngle > twistLimit)
        {
            isLimited = true;

            // Clamp the twist rotation to the twist limit
            Quaternion clampedTwistRotation = Quaternion.RotateTowards(
                Quaternion.identity,
                twistRotation,
                twistLimit
            );

            // Apply the clamped rotation to the transform's local rotation
            transform.localRotation = clampedTwistRotation * Quaternion.Inverse(twistRotation) * localRotation;
        }
    }

    public override void ApplyAngleLimit(out bool isLimited)
    {
        isLimited = false;

        Quaternion desiredRotation = transform.localRotation * Quaternion.Inverse(_cachedLocalRotation);
        // Get the current rotation of the parent
        Vector3 currentParentForwardAxis = Vector3.Normalize(transform.position - transform.parent.position);
        
        // Calculate the oriented rotation axis
        _orientedRotationAxis = _cachedParentRotationAxisDifference * currentParentForwardAxis;
        
        if (_rotationAxis == Vector3.zero) return; // Ignore with zero axes
        if (desiredRotation == Quaternion.identity) return; // Assuming initial rotation is in the reachable area
        if (_angleLimit >= 180) return;
        
        
        Vector3 rotatedTargetAxis = desiredRotation * _orientedRotationAxis;
        
        
        // Limit the rotation by clamping to the _angleLimit
        Quaternion swingRotation = Quaternion.FromToRotation(_orientedRotationAxis, rotatedTargetAxis);
        Quaternion limitedSwingRotation = Quaternion.RotateTowards(Quaternion.identity, swingRotation, _angleLimit);
        
        //float _angleRotated = Vector3.SignedAngle(_orientedRotationAxis, rotatedTargetAxis, _arbitraryAxis);
        //float _totalAngleRotated = Vector3.Angle(_orientedRotationAxis, )
        float _angleRotated = Vector3.Angle(_orientedRotationAxis, rotatedTargetAxis);
        if (_angleRotated < _angleLimit) return;
        Debug.Log(_angleRotated);
        
        //Debug.DrawLine(transform.position, transform.position + _orientedRotationAxis * 20, Color.red);
        //Debug.DrawLine(transform.position, transform.position + rotatedTargetAxis * 20, Color.green);

    
        isLimited = true;
        Debug.Log("Limiting Rotation");
        // Apply the limited rotation to the original desired rotation
        transform.localRotation = limitedSwingRotation * _cachedLocalRotation;
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
    if (perpendicularAxis == Vector3.zero) // Handle cases where baseAxis is aligned with Vector3.up
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
