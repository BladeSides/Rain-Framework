using System;
using UnityEditor;
using UnityEngine;

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
    
    private Vector3 _orientedRotationAxis;

    private bool _initialized;

    [SerializeField]
    private float _angleLimit = 90.0f;

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
    
    _cachedParentRotationAxisDifference = Quaternion.FromToRotation(parentForwardAxis, _cachedLocalRotation * _rotationAxis);
    
    _initialized = true;

    }

    public override Quaternion GetLimitedAngle(Quaternion desiredRotation, Vector3 endPosition, out bool isLimited)
    {
        isLimited = false;

        // Get the current rotation of the parent
        Vector3 currentParentForwardAxis = Vector3.Normalize(transform.position - transform.parent.position);
        
        // Calculate the oriented rotation axis
        _orientedRotationAxis = _cachedParentRotationAxisDifference * currentParentForwardAxis;
        
        if (_rotationAxis == Vector3.zero) return desiredRotation; // Ignore with zero axes
        if (desiredRotation == Quaternion.identity) return desiredRotation; // Assuming initial rotation is in the reachable area
        if (_angleLimit >= 180) return desiredRotation;
        
        
        Vector3 rotatedTargetAxis = desiredRotation * _orientedRotationAxis;
        
        
        // Limit the rotation by clamping to the _angleLimit
        Quaternion swingRotation = Quaternion.FromToRotation(_orientedRotationAxis, rotatedTargetAxis);
        Quaternion limitedSwingRotation = Quaternion.RotateTowards(Quaternion.identity, swingRotation, _angleLimit);
        
        //float _angleRotated = Vector3.SignedAngle(_orientedRotationAxis, rotatedTargetAxis, _arbitraryAxis);
        float _angleRotated = Vector3.Angle(_orientedRotationAxis, rotatedTargetAxis);
        if (_angleRotated < _angleLimit) return desiredRotation;
        Debug.Log(_angleRotated);
        
        Debug.DrawLine(transform.position, transform.position + _orientedRotationAxis * 20, Color.red);
        Debug.DrawLine(transform.position, transform.position + rotatedTargetAxis * 20, Color.green);

    
        isLimited = true;
        Debug.Log("Limiting Rotation");
        // Apply the limited rotation to the original desired rotation
        return limitedSwingRotation * _cachedLocalRotation;
    }

    public void OnDrawGizmosSelected()
    {
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

    }
}
