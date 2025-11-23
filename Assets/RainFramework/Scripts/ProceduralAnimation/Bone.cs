using UnityEngine;

[System.Serializable]
public struct Bone
{
    public string BoneName;
    public Joint StartJoint;
    public Joint EndJoint;
    public float BoneLength;
}
