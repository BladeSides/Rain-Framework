using System.Collections.Generic;
using UnityEngine;

public class ProceduralAnimationManager : MonoBehaviour
{
    public List<IKBase> IKs;
    public int IKIterations = 1;

    private void Awake()
    {
        foreach (IKBase IK in IKs)
        {
            IK.Init();
        }
    }
    private void LateUpdate()
    {
        foreach (IKBase IK in IKs)
        {
            IK.Iterate();
        }
    }
}
