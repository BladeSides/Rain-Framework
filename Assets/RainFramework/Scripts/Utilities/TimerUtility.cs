using System;
using UnityEngine;

namespace RainFramework.Utilities
{
    // Class for using a timer
    // Written for RainFramework by BladeSides
    public class TimerUtility : MonoBehaviour
    {
        #region Public Variables
        public MonoBehaviour Owner;
        public float CurrentTime
        {
            get;
            private set;
        }
        public float TotalTime;
        public bool IsTimerFinished;
        public bool IsEnabled = true;
        public bool RestartOnEnd = false;
        #endregion

        public event EventHandler OnTimerStart;
        public event EventHandler OnTimerEnd;

        /// <summary>
        /// Returns a float between 0 and 1 of how much the timer has finished
        /// </summary>
        public float PercentCompleted { get 
            {
                float percent = CurrentTime / TotalTime;
                
                return Mathf.Clamp01(percent);
            }
            }

        #region Public Methods
        /// <summary>
        /// Restarts the timer, setting the current time to zero.
        /// </summary>
        public void RestartTimer()
        {
            CurrentTime = 0;
            IsTimerFinished = false;
            OnTimerStart?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Changes the amount of time the timer takes to finish
        /// </summary>
        /// <param name="newTotalTime"></param>
        /// <param name="restartTimer"></param>
        public void SetTotalTime(float newTotalTime, bool restartTimer)
        {
            TotalTime = newTotalTime;
            if (restartTimer)
            {
                CurrentTime = 0;
                IsTimerFinished = false;
                OnTimerStart?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SetCurrentTime(float newCurrentTime)
        {
            CurrentTime = newCurrentTime;
        }

        #endregion

        #region Private Methods

        public void Awake()
        {
            OnTimerStart?.Invoke(this, EventArgs.Empty);
        }

        private void Update()
        {
            if (RestartOnEnd && CurrentTime > TotalTime)
            {
                CurrentTime -= TotalTime;
            }

            if (IsTimerFinished || !IsEnabled)
            {
                return;
            }

            CurrentTime += Time.deltaTime;
            if (CurrentTime > TotalTime && !RestartOnEnd)
            {
                OnTimerEnd?.Invoke(this, EventArgs.Empty);
                IsTimerFinished = true;
            }
        }

        #endregion
    }
}