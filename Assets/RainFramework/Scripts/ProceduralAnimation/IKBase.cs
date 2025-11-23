using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class IKBase : MonoBehaviour
{
    // Joints exist per transform
    public List<Joint> Joints;
    [InfoBox("Bones are set up using the Set Up IK Button", EInfoBoxType.Normal)]
    // One bone has two joints
    public List<Bone> Bones;

    // Sum of all bone lengths
    public float TotalLength;

    // Should this be called per awake? At least need a validation.

    [Button("Set Up IK")]
    public void SetUpIk()
    {
        SetUpBones();
        CalculateTotalLength();
        CalculateForwardAxes();
    }

    private void CalculateForwardAxes()
    {
        for (int i = 0; i < Joints.Count; i++)
        {
            if (!Joints[i].AutoCalculateForwardAxis)
                continue;

            if (i == Joints.Count - 1)
            {
                //Default value for last joint since no child exists
                Joints[i].LocalForwardAxis = Vector3.forward;
            }
            else
            {
                Vector3 forward = (Joints[i + 1].transform.position - Joints[i].transform.position);
                forward = Joints[i].transform.InverseTransformDirection(forward).normalized;
                Joints[i].LocalForwardAxis = forward;
            }
        }
    }


    private void CalculateTotalLength()
    {
        TotalLength = 0;
        foreach (Bone bone in Bones)
        {
            TotalLength += bone.BoneLength;
        }
    }

    private void SetUpBones()
    {
        Bones = new List<Bone>();

        for (int i = 0; i < Joints.Count - 1; i++)
        {
            Joint start = Joints[i];
            Joint end = Joints[i + 1];

            Bone bone = new Bone();

            bone.StartJoint = start;
            bone.EndJoint = end;
            bone.BoneName = Joints[i].name + "_" + Joints[i+1].name;
            bone.BoneLength = Vector3.Distance(Joints[i].transform.position, Joints[i + 1].transform.position);
            
            Bones.Add(bone);
        }
    }

    public virtual void Init()
    {
        // no op
    }

    private void Awake()
    {
    }

    public virtual void OnDrawGizmos()
    {
        if (Bones == null)
        {
            Debug.Log("Bones not set");
            return;
        }
        for (int i = 0; i < Bones.Count; i++)
        {
            var scale = Bones[i].BoneLength;
            Handles.matrix = Matrix4x4.TRS(Bones[i].EndJoint.transform.position,
                Quaternion.FromToRotation(Vector3.up, Bones[i].StartJoint.transform.position - Bones[i].EndJoint.transform.position),
                new Vector3(0.5f, scale, 0.5f));
            Handles.color = Color.green;
            Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
            Handles.matrix = Matrix4x4.identity;
        }
    }

    public abstract void Iterate();
}