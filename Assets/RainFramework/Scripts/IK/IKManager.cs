using System;
using System.Collections.Generic;
using UnityEngine;

public class IKManager : MonoBehaviour
{
    public List<IKSolver> IKs = new List<IKSolver>();

    private void LateUpdate()
    {
        for (int i = 0; i < IKs.Count; i++)
        {
            IKs[i].UpdateIK();
        }
    }
}
