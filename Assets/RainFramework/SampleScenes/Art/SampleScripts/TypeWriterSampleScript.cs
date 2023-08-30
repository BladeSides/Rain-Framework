using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TypeWriterSampleScript : MonoBehaviour
{
    public TypeWriter TypeWriter;
    public TextMeshProUGUI TMP;

    // Update is called once per frame
    void Update()
    {
        TMP.text = TypeWriter.CurrentText;            
    }
}
