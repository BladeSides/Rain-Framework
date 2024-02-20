using RainFramework.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("TypeWriter Audio is Obsolete. Try implementing the Audio with TypeWriter instead.", false)]

public class TypeWriterAudio : TypeWriter
{
    public AudioSource AudioSource;
    public override void Update()
    {
        base.Update();

        if (TimerUtility.IsTimerFinished)
        {
            if (IsTyping)
            {
                if (CurrentText != TargetText)
                {
                    if (!AudioSource.isPlaying) 
                    {
                        AudioSource.Play();
                    }
                }
                else
                {
                    AudioSource.Pause();
                }
            }

            if (IsErasing)
            {
                if (CurrentText != "")
                {
                    if (!AudioSource.isPlaying)
                    {
                        AudioSource.Play();
                    }
                }
                else
                {
                    AudioSource.Pause();
                }
            }
        }
    }
}
