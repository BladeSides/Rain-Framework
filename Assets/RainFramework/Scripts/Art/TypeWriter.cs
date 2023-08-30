using RainFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TypeWriter : MonoBehaviour
{
    public string CurrentText;
    public string TargetText;
    public TimerUtility TimerUtility;
    public float SpeedPerCharacter = 0.05f;
    public float EndWait = 3f;
    public bool IsTyping = true;
    public bool IsErasing;
    public bool IsFinished
    {
        get
        {
            if (IsTyping)
            {
                if (CurrentText == TargetText)
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
                if (CurrentText == "")
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
    }
    public virtual void Update()
    {
        //Reset current text
        if (!TargetText.Contains(CurrentText))
        {
            CurrentText = "";
        }
        

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
                if (CurrentText != TargetText)
                {
                    CurrentText = TargetText.Substring(0, CurrentText.Length + 1);
                }
                TimerUtility.RestartTimer();
            }

            if (IsErasing)
            {
                if (CurrentText != "")
                {
                    CurrentText = CurrentText.Substring(0, CurrentText.Length - 1);
                }
                TimerUtility.RestartTimer();
            }
        }
    }
}
