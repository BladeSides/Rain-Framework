using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RainFramework.Math
{
    // Class for solving IK by using FabrIK algorithm
    // Written for RainFramework by BladeSides
    // Heavily derived from https://www.youtube.com/watch?v=qqOAzn05fvk 's implementation

    public class FabrIK : MonoBehaviour
    {
        /// <summary>
        /// Number of chains
        /// </summary>
        public int BoneCount = 2;

        /// <summary>
        /// The target point of the IK
        /// </summary>
        public Transform Target;

        /// <summary>
        /// Defines what angle the IK will bend towards
        /// </summary>
        public Transform TargetPole;


        protected float[] BonesLength; //Target to Origin
        protected float CompleteLength;
        protected Transform[] Bones;
        protected Vector3[] Positions;

        protected Vector3[] StartDirectionSuccessors;
        protected Quaternion[] StartRotationBones;
        protected Quaternion StartRotationTarget;
        protected Quaternion StartRotationRoot;

        [Header("Parameters")]

        /// <summary>
        /// The amount of times the IK runs
        /// </summary>
        public int Iterations = 10;

        /// <summary>
        /// Distance when the IK stops solving
        /// </summary>
        public float Offset = 0.01f;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            //Initialize arrays
            Bones = new Transform[BoneCount + 1];

            Positions = new Vector3[BoneCount + 1];
            BonesLength = new float[BoneCount];

            StartDirectionSuccessors = new Vector3[BoneCount + 1];
            StartRotationBones = new Quaternion[BoneCount + 1];

            CompleteLength = 0;

            //Init Target
            if (Target == null)
            {
                Target = new GameObject(gameObject.name + "_Target").transform;
                Target.position = transform.position;
            }

            //Init fields
            StartRotationTarget = Target.rotation;
            CompleteLength = 0;

            Transform current = transform;
            for (int i = Bones.Length - 1; i >= 0; i--)
            {
                Bones[i] = current;
                StartRotationBones[i] = current.rotation;

                if (i == Bones.Length - 1) //Leaf bone, no length (last bone)
                {
                    //Do nothing
                }
                else //Mid bones
                {
                    StartDirectionSuccessors[i] = Bones[i + 1].transform.position - current.position;
                    BonesLength[i] = (Bones[i + 1].position - current.position).magnitude;
                    CompleteLength += BonesLength[i];
                }

                current = current.parent;
            }
        }

        private void LateUpdate()
        {
            CalculateIK();
        }

        private void CalculateIK()
        {
            //Early return if no target
            if (Target == null)
            {
                return;
            }

            //Reinitialize if bones length has changed
            if (BonesLength.Length != BoneCount)
            {
                Initialize();
            }

            //Get positions
            for (int i = 0; i < Bones.Length; i++)
            {
                Positions[i] = Bones[i].position;
            }

            Quaternion rootRotation = (Bones[0].parent != null) ? Bones[0].parent.rotation : Quaternion.identity;
            Quaternion rootRotationDifference = rootRotation * Quaternion.Inverse(StartRotationRoot);

            //Calculation

            //Is position possible to reach?
            if ((Target.position - Bones[0].position).sqrMagnitude >= CompleteLength * CompleteLength)
            {
                Vector3 direction = (Target.position - Bones[0].position).normalized;

                for (int i = 1; i < Positions.Length; i++)
                {
                    Positions[i] = Positions[i - 1] + direction * BonesLength[i - 1];
                }
            }

            //Implement FABRIK
            else
            {
                for (int iteration = 0; iteration < Iterations; iteration++)
                {
                    //Back -> Start from last, go to front, ignore 0 to keep root bone at place
                    for (int i = Positions.Length - 1; i > 0; i--)
                    {
                        if (i == Positions.Length - 1)
                        {
                            Positions[i] = Target.position; //Set last position (end effector) to target position
                        }
                        else
                        {
                            //Set bone to be correct distance
                            Positions[i] = Positions[i + 1] + (Positions[i] - Positions[i + 1]).normalized * BonesLength[i];
                        }
                    }

                    //Forward -> Start from root, go to end effector
                    for (int i = 1; i < Positions.Length; i++)
                    {
                        //Set bone to be correct distance
                        Positions[i] = Positions[i - 1] + (Positions[i] - Positions[i - 1]).normalized * BonesLength[i - 1];
                    }

                    //Stop if it's close enough
                    if ((Positions[Positions.Length - 1] - Target.position).sqrMagnitude < Offset * Offset)
                    {
                        break;
                    }
                }
            }

            //Move towards Target Pole
            if (TargetPole != null)
            {
                for (int i = 1; i < Positions.Length - 1; i++)
                {
                    //Create a plane skipping one position which will be moved on that plane

                    Plane plane = new Plane(Positions[i + 1] - Positions[i - 1], Positions[i - 1]);

                    //Project the Pole and Bone onto the plane so they can be moved in a circle
                    Vector3 projectedPole = plane.ClosestPointOnPlane(TargetPole.position);
                    Vector3 projectedBone = plane.ClosestPointOnPlane(Positions[i]);

                    float angle = Vector3.SignedAngle(projectedBone - Positions[i - 1], projectedPole - Positions[i - 1],
                        plane.normal);

                    //Move the position to be on the plane
                    Positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (Positions[i] - Positions[i - 1]) 
                        + Positions[i - 1];
                }
            }

            //Set positions and rotation
            for (int i = 0; i < Bones.Length; i++)
            {
                Bones[i].position = Positions[i];

                if (i == Positions.Length - 1)
                {
                    Bones[i].rotation = Target.rotation * Quaternion.Inverse(StartRotationTarget) * StartRotationBones[i];
                }
                else
                {
                    Bones[i].rotation = Quaternion.FromToRotation(StartDirectionSuccessors[i],
                        Positions[i + 1] - Positions[i]) * StartRotationBones[i];
                }
            }
        }
        private void OnDrawGizmos()
        {
            Transform current = this.transform;
            for (int i = 0; i < BoneCount && current != null && current.parent != null; i++)
            {
                //Size of cubes
                float scale = Vector3.Distance(current.position, current.parent.position) * 0.1f;
                
                //Transform, Rotation, and Scale of Handles
                Handles.matrix = Matrix4x4.TRS(current.position,
                    Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position),
                    new Vector3(scale, Vector3.Distance(current.parent.position, current.position), scale));
                Handles.color = Color.green;
                
                //Multiply by 0.5 to centre them
                Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
                current = current.parent;
            }
    }
    }
}

