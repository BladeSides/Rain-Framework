using RainFramework.Structures;
using RainFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Class for animating sprites on planes using UV-Coordinates
// Written for RainFramework by BladeSides

namespace RainFramework.Art
{
    public class SpriteAnimator : MonoBehaviour
    {
        #region Animation Class

        //Animation Class
        [System.Serializable]
        public class Animation
        {
            public string Name;
            public Material AnimationMaterial;
            public float SpeedPerFrame;
            public Vector2Int RowsAndColumns;
            public Vector3 Size = new(0, 0, 1);

            //Note: Frames start from 0
            public int LastFrame;
            public bool PingPong;
            public bool FinishOnEnd;
        }

        #endregion

        #region Variables
        //Mesh to Animate
        public MeshRenderer AnimationMesh;

        //Put Animations Here
        public Animation[] Animations;
        public int FirstAnimationIndex = 0;

        //Ping Pong -> Play animation in reverse when finished
        public Vector3Bool IsFlipped;

        //Useful Values, can access from other scripts
        public int CurrentFrame;
        public bool IsFinished
        {
            get
            {
                if (currentAnimation.FinishOnEnd && CurrentFrame >= MaxFrame && _timer.IsTimerFinished)
                {
                    return true;
                }
                return false;
            }
        }
        [HideInInspector] public int MaxFrame;
        public Animation currentAnimation;

        //Private variables
        private TimerUtility _timer;
        private GameObject _meshGameObject;
        private Vector2 _UVTileSize;
        private int _currentRow;
        private int _currentColumn;
        private bool _playingInReverse;
        #endregion

        #region Public Methods

        /// <summary>
        /// Change animation to another animation;
        /// </summary>
        /// <param name="Name of the Animation"></param>
        public void ChangeAnimation(string name)
        {
            foreach (Animation animation in Animations)
            {
                if (animation.Name == name && animation != currentAnimation)
                {
                    currentAnimation = animation;
                    AnimationMesh.material = currentAnimation.AnimationMaterial;
                    ResetAnimation();
                }
            }
        }

        //To toggle flips
        public void ToggleFlipX(bool toggle)
        {
            IsFlipped.X = toggle;
            SetScaleAccordingToFlip();
        }
        public void ToggleFlipY(bool toggle)
        {
            IsFlipped.Y = toggle;
            SetScaleAccordingToFlip();
        }
        public void ToggleFlipZ(bool toggle)
        {
            IsFlipped.Z = toggle;
            SetScaleAccordingToFlip();
        }

        #endregion

        #region Private Methods

        private void Start()
        {
            _timer = gameObject.AddComponent<TimerUtility>();
            _timer.Owner = this;

            SetFirstAnimation();

            ResetAnimation();
        }

        private void Update()
        {
            if (currentAnimation.FinishOnEnd && CurrentFrame >= MaxFrame)
            {
                return;
            }
            if (_timer.IsTimerFinished)
            {
                if (!_playingInReverse)
                {
                    MoveToNextFrame();
                }
                else
                {
                    MoveToPreviousFrame();
                }
                SetFrame();
            }
        }



        private void SetFirstAnimation()
        {
            currentAnimation = Animations[FirstAnimationIndex];
            AnimationMesh.material = currentAnimation.AnimationMaterial;
            _meshGameObject = AnimationMesh.gameObject;
        }

        //To Reset The Animation
        public void ResetAnimation()
        {
            if (_timer != null)
            {
                _timer.SetTotalTime(currentAnimation.SpeedPerFrame, true);
            }
            CurrentFrame = 0;
            _currentColumn = 0;
            _currentRow = 0;
            SetScaleAccordingToFlip();

            MaxFrame = currentAnimation.LastFrame;

            if (MaxFrame > currentAnimation.RowsAndColumns.x * currentAnimation.RowsAndColumns.y - 1)
            {
                MaxFrame = currentAnimation.RowsAndColumns.x * currentAnimation.RowsAndColumns.y - 1;
            }

            //Set UV Tile Size
            _UVTileSize = new Vector2(1f / currentAnimation.RowsAndColumns.y, 1f / currentAnimation.RowsAndColumns.x);
            AnimationMesh.material.mainTextureScale = _UVTileSize;

            SetFrame();
        }

        private void SetScaleAccordingToFlip()
        {
            _meshGameObject.transform.localScale = new Vector3(currentAnimation.Size.x * (IsFlipped.X ? -1 : 1),
            currentAnimation.Size.y * (IsFlipped.Y ? -1 : 1), currentAnimation.Size.z * (IsFlipped.Z ? -1 : 1));
        }

        private void MoveToPreviousFrame()
        {
            _timer.RestartTimer();
            CurrentFrame--;
            _currentColumn--;

            if (CurrentFrame < 0)
            {
                //Reset to first frame
                if (!currentAnimation.PingPong)
                {
                    CurrentFrame = MaxFrame;
                    _currentRow = currentAnimation.RowsAndColumns.x - 1;
                    _currentColumn = currentAnimation.RowsAndColumns.y - 1;
                }
                //Ping Pong
                else
                {
                    CurrentFrame = 0;
                    _currentColumn++;
                    _timer.CurrentTime = currentAnimation.SpeedPerFrame;
                    _playingInReverse = false;
                }
            }

            //Move to previous row
            if (_currentColumn < 0)
            {
                _currentColumn = currentAnimation.RowsAndColumns.y - 1;
                _currentRow--;
            }
        }

        private void MoveToNextFrame()
        {
            _timer.RestartTimer();
            CurrentFrame++;
            _currentColumn++;

            if (CurrentFrame > MaxFrame)
            {
                //Reset to first frame
                if (!currentAnimation.PingPong)
                {
                    CurrentFrame = 0;
                    _currentRow = 0;
                    _currentColumn = 0;
                }
                //Ping Pong
                else
                {
                    CurrentFrame = MaxFrame;
                    _currentColumn--;
                    _timer.CurrentTime = currentAnimation.SpeedPerFrame;
                    _playingInReverse = true;
                }
            }

            //Move to next row
            if (_currentColumn > currentAnimation.RowsAndColumns.y - 1)
            {
                _currentColumn = 0;
                _currentRow++;
            }
        }

        private void SetFrame()
        {
            //Set UV Part to use
            AnimationMesh.material.mainTextureOffset = new Vector2(_UVTileSize.x * _currentColumn,
                _UVTileSize.y * (currentAnimation.RowsAndColumns.x - 1) - _UVTileSize.y * _currentRow);
        }
        #endregion
    }

}