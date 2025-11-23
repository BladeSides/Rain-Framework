using UnityEngine;

public class Joint : MonoBehaviour
{
    public bool AutoCalculateForwardAxis = true;
    //Forward Axis in Local Space
    public Vector3 LocalForwardAxis = new Vector3 (0,0,1);

    private void OnDrawGizmos()
    {
        Debug.DrawLine(transform.position, transform.position + transform.TransformDirection(LocalForwardAxis));
    }
}
