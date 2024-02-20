using RainFramework.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypeWriter : MonoBehaviour
{
    public string TargetText;
    public TimerUtility TimerUtility;
    public float SpeedPerCharacter = 0.05f;
    //public float EndWait = 3f;
    public bool IsTyping = true;
    public int CurrentCharactersCount;
    private int _targetCharactersCount;
    public bool IsErasing;
    public bool IsFinished
    {
        get
        {
            if (IsTyping)
            {
                if (CurrentCharactersCount == _targetCharactersCount)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (CurrentCharactersCount == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }

    public void StartTyping()
    {
        IsTyping = true;
        IsErasing = false;
    }
    public void StartErasing()
    {
        IsErasing = true;
        IsTyping = false;
    }

    public void StopTyping()
    {
        IsTyping = false;
    }

    public void StopErasing()
    {
        IsErasing = false;
    }
    private void Start()
    {
        TimerUtility = gameObject.AddComponent<TimerUtility>();
        TimerUtility.SetTotalTime(SpeedPerCharacter, true);
        TimerUtility.RestartOnEnd = false;
        TimerUtility.Owner = this;
        _targetCharactersCount = TargetText.Length;
    }
    public virtual void Update()
    {
        CurrentCharactersCount = Mathf.Clamp(CurrentCharactersCount, 0, _targetCharactersCount);
        if (IsTyping)
        {
            IsErasing = false;
        }

        if (IsErasing)
        {
            IsTyping = false;
        }

        if (TimerUtility.IsTimerFinished)
        {
            if (IsTyping)
            {
                if (CurrentCharactersCount < _targetCharactersCount)
                {
                    CurrentCharactersCount++;
                }
                TimerUtility.RestartTimer();
            }

            if (IsErasing)
            {
                if (CurrentCharactersCount > 0)
                {
                    CurrentCharactersCount--;
                }
                TimerUtility.RestartTimer();
            }
        }
    }
}
