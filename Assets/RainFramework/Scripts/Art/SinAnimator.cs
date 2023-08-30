using RainFramework.Structures;
using RainFramework.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Make sin animations add to transform instead of setting the transform
// Currently, having multiple sins might reset each other, you can't have separate sin for separate scale.x,scale.y etc

namespace RainFramework.Art
{
    // Class for animating objects using a sin wave
    // Place the object in an empty transform where this script is attached
    // Written for RainFramework by BladeSides

    public class SinAnimator : MonoBehaviour
    {
        //Whether sine goes to negative or not
        public bool AbsoluteWave;

        //Between 0 and 1
        [Range(0, 1)]
        public float StartingOffset;

        public Vector3Bool IsScaled;
        public Vector3Bool IsRotated;
        public Vector3Bool IsTranslated;

        //How much the value seshould change by
        public float DeltaScaleAmplitude;
        public float DeltaRotationAmplitude;
        public float DeltaTranslationAmplitude;

        //Reference values
        private Vector3 StartingScale;
        private Vector3 StartingRotation;
        private Vector3 StartingPosition;

        public float TimePerOscillation;
        public TimerUtility Timer;



        /// <summary>
        /// Updates the position at centre point of sine wave
        /// </summary>
        /// <param name="newStartingPosition"></param>
        public void UpdateStartingPosition(Vector3 newStartingPosition)
        {
            StartingPosition = newStartingPosition;
        }

        /// <summary>
        /// Updates the scale at centre point of sine wave
        /// </summary>
        /// <param name="newScale"></param>
        public void UpdateStartingScale(Vector3 newScale)
        {
            StartingScale = newScale;
        }

        /// <summary>
        /// Updates the rotation at centre point of sine wave
        /// </summary>
        /// <param name="newStartingRotation"></param>
        public void UpdateStartingRotation(Vector3 newStartingRotation)
        {
            StartingRotation = newStartingRotation;
        }

        public void ResetTransformation()
        {
            transform.localScale = StartingScale;
            transform.SetLocalPositionAndRotation(StartingPosition, Quaternion.Euler(StartingRotation));
        }
        public Vector3 GetStartingScale()
        {
            return StartingScale;
        }

        public Vector3 GetStartingRotation()
        {
            return StartingRotation;
        }
        public Vector3 GetStartingPosition()
        {
            return StartingPosition;
        }

        private void Start()
        {
            SetTimer();

            SetStartingScale();
            SetStartingRotation();
            SetStartingPosition();
        }

        private void SetTimer()
        {
            Timer = gameObject.AddComponent<TimerUtility>();
            Timer.Owner = this;
            Timer.RestartOnEnd = true;
            Timer.SetTotalTime(TimePerOscillation, false);
            Timer.SetCurrentTime(StartingOffset * TimePerOscillation);
        }

        private void SetStartingScale()
        {
            StartingScale = transform.localScale;
        }
        private void SetStartingRotation()
        {
            StartingRotation = new Vector3
                (transform.localRotation.eulerAngles.x, 
                transform.localRotation.eulerAngles.y, 
                transform.localRotation.eulerAngles.z) ;
        }
        private void SetStartingPosition()
        {
            StartingPosition = transform.localPosition;
        }

        // Update is called once per frame
        void Update()
        {
            float SinValue = Mathf.Sin(Timer.PercentCompleted * Mathf.PI * 2);
            if (AbsoluteWave)
            {
                SinValue *= SinValue;
            }

            if (IsScaled.X || IsScaled.Y || IsScaled.Z)
            {
                SetScale(SinValue);
            }

            if (IsRotated.X || IsRotated.Y || IsRotated.Z)
            {
                SetRotation(SinValue);
            }

            if (IsTranslated.X || IsTranslated.Y || IsTranslated.Z)
            {
                SetPosition(SinValue);
            }
        }

        private void SetPosition(float SinValue)
        {
            transform.localPosition = new Vector3(
                                        StartingPosition.x + DeltaTranslationAmplitude * (IsTranslated.X ? SinValue : 0),
                                        StartingPosition.y + DeltaTranslationAmplitude * (IsTranslated.Y ? SinValue : 0),
                                        StartingPosition.z + DeltaTranslationAmplitude * (IsTranslated.Z ? SinValue : 0)
                                        );
        }

        private void SetRotation(float SinValue)
        {
            transform.localRotation = Quaternion.Euler(
                            StartingRotation.x + DeltaRotationAmplitude * (IsRotated.X ? SinValue : 0),
                            StartingRotation.y + DeltaRotationAmplitude * (IsRotated.Y ? SinValue : 0),
                            StartingRotation.z + DeltaRotationAmplitude * (IsRotated.Z ? SinValue : 0)
                            );
        }

        private void SetScale(float SinValue)
        {
            transform.localScale = new Vector3(
                            StartingScale.x + DeltaScaleAmplitude * (IsScaled.X ? SinValue : 0),
                            StartingScale.y + DeltaScaleAmplitude * (IsScaled.Y ? SinValue : 0),
                            StartingScale.z + DeltaScaleAmplitude * (IsScaled.Z ? SinValue : 0)
                            );
        }
    }
}