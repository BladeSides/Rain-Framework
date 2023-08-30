using RainFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RainFramework.Art
{
    public class MaterialFader : MonoBehaviour
    {
        private TimerUtility _timer;
        [SerializeField] private MeshRenderer _meshRenderer;
        private Material _material;

        [SerializeField] private float _timeToFade;

        [SerializeField] private bool _fadingIn = true;
        [SerializeField] private bool _fadingOut = false;

        [SerializeField] private Color _startingColor;
        [SerializeField] private Color _endingColor;
        [SerializeField] private Color _currentColor;

        public bool IsFinished
        {
            get
            {
                if (_fadingIn || _fadingOut)
                {
                    if (_timer.IsTimerFinished)
                    {
                        return true;
                    }
                    return false;
                }
                return true;
            }
        }

        private void Start()
        {
            _timer = gameObject.AddComponent<TimerUtility>();
            _timer.SetTotalTime(_timeToFade, true);
            _currentColor = _startingColor;
            _material = _meshRenderer.material;
            _material.color = _startingColor;
        }

        public void StartFadeIn()
        {
            _fadingIn = true;
            _fadingOut = false;
            _timer.RestartTimer();
        }

        public void StartFadeOut()
        {
            _fadingIn = false;
            _fadingOut = true;
            _timer.RestartTimer();
        }

        public void StopFade()
        {
            _fadingIn = false;
            _fadingOut = false;
        }
        /// <summary>
        /// Sets FadingIn to the value passed, and FadingOut to !value
        /// </summary>
        /// <param name="fadeIn"></param>
        public void ResumeFade(bool fadeIn)
        {
            _fadingIn = fadeIn;
            _fadingOut = !fadeIn;
        }

        private void Update()
        {
            if (_timer.IsTimerFinished)
            {
                _fadingIn = false;
                _fadingOut = false;
                return;
            }

            if (_fadingIn)
            {
                _currentColor = Color.Lerp(_startingColor, _endingColor, _timer.PercentCompleted);
            }
            if (_fadingOut)
            {
                _currentColor = Color.Lerp(_endingColor, _startingColor, _timer.PercentCompleted);
            }

            _material.color = _currentColor;
        }
    }
}