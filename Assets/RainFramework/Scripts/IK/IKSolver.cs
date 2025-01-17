using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Serialization;

public abstract class IKSolver : MonoBehaviour
{
    //List of transforms that gets turned into bones
    [SerializeField]
    public List<Transform> Joints = new List<Transform>();

    public Transform TargetTransform;
    
    [SerializeField]
    public List<Bone> Bones;
    
    public virtual void Awake()
    {
        if (Joints.Count < 2)
        {
            Debug.LogError("At least 2 joint required to solve IK");
        }

        Bones = new List<Bone>();
        
        for (int i = 0; i < Joints.Count - 1; i++)
        {
            Bone bone = new Bone();
            bone.BoneLength = Vector3.Distance(Joints[i].position, Joints[i + 1].position);
            if (bone.BoneLength == 0)
            {
                Debug.LogError("Joint " + i + " and " + (i + 1) + " are in the same position");
            }
            bone.StartTransform = Joints[i];
            bone.EndTransform = Joints[i + 1];
            Bones.Add(bone);
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < Joints.Count - 1; i++)
        {
            Color color = Color.green;
            color.a = 0.5f;
            
            Gizmos.color = color;
            Gizmos.DrawLine(Joints[i].position, Joints[i + 1].position);
            
            Handles.Label(Joints[i].position, $"Bone {i}");
            
            if (i == Joints.Count - 2)
            {
                Handles.Label(Joints[i + 1].position, $"Bone {i + 1}");
            }
        }

        Gizmos.color = Color.white;
        Gizmos.DrawLine(Joints[Joints.Count-1].position, TargetTransform.position);
    }

    public abstract void UpdateIK(int iterations = 1);

    [System.Serializable]
    public struct Bone
    {
        public Transform StartTransform;
        public Transform EndTransform;
        public float BoneLength;
    }
}
