using System;
using System.Collections.Generic;
using UnityEngine;

public class IKManager : MonoBehaviour
{
    public List<IKSolver> IKs = new List<IKSolver>();
    public int Iterations = 1;
    private void LateUpdate()
    {
        for (int i = 0; i < IKs.Count; i++)
        {
            UpdateIK(IKs[i], Iterations);    
        }
    }

    void UpdateIK(IKSolver solver, int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            solver.UpdateIK();
        }
    }
}
