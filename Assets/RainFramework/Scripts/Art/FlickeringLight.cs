using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using RainFramework.Utilities;

namespace RainFramework.Art
{
    public class FlickeringLight : MonoBehaviour
    {
        public Light Light;
        public QuakeLightsPresets QuakeLightsPresets;
        public int PresetIndex;
        public float MinimumBrightness;
        public float MaximumBrightness;
        public float TimePerChange = 0.1f;

        private string _lightPreset;
        private int _currentIndex;
        private TimerUtility _timer;
        public bool smoothTransition = false;
        public float TargetLightIntensity;
        public float LerpSpeed;
        private void Awake()
        {
            Light = GetComponent<Light>();

            _lightPreset = QuakeLightsPresets.LightingPresets[PresetIndex];
            _currentIndex = 0;
        }

        private void Start()
        {
            _timer = gameObject.AddComponent<TimerUtility>();
            _timer.SetTotalTime(TimePerChange, false);
            _timer.Owner = this;

            float lightValueLerp = ((float)(_lightPreset[_currentIndex] - 'a') / 25f);
            lightValueLerp *= 2f;
            TargetLightIntensity = Mathf.LerpUnclamped(MinimumBrightness, MaximumBrightness, lightValueLerp);
            Light.intensity = TargetLightIntensity;
        }

        public void Update()
        {
            if (_timer.IsTimerFinished)
            {
                _currentIndex++;
                if (_currentIndex > _lightPreset.Length - 1)
                {
                    _currentIndex = 0;
                }

                float lightValueLerp = ((float)(_lightPreset[_currentIndex] - 'a' + 1) / 26f);
                lightValueLerp *= 2f;
                TargetLightIntensity = Mathf.LerpUnclamped(MinimumBrightness, MaximumBrightness, lightValueLerp);

                if (!smoothTransition)
                {
                    Light.intensity = TargetLightIntensity;
                }

                _timer.RestartTimer();
            }

            if (smoothTransition)
            {
                Light.intensity = Mathf.Lerp(TargetLightIntensity, Light.intensity, Mathf.Pow(0.25f, Time.deltaTime * LerpSpeed));
            }
        }
    }

}
