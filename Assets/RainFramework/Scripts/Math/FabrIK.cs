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
        public Transform PoleTarget;

        [Header("Parameters")]

        protected float[] BonesLength;
        protected float CompleteLength;
        protected Transform[] Bones;
        protected Vector3[] Positions;

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

            CompleteLength = 0;

            Transform current = transform;
            for (int i = Bones.Length - 1; i >= 0; i--)
            {
                Bones[i] = current;

                if (i == Bones.Length - 1) //Leaf bone, no length (last bone)
                {
                    BonesLength[i] = 0;
                }
                else
                {
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

            if (BonesLength.Length != BoneCount)
            {
                Initialize();
            }

            //Get positions
            for (int i = 0; i < Bones.Length; i++)
            {
                Positions[i] = Bones[i].position;
            }

            //Calculation


            //Set positions
            for (int i = 0; i < Bones.Length; i++)
            {
                Bones[i].position = Positions[i];
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

