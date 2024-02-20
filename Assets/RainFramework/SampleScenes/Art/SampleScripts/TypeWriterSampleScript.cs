using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TypeWriterSampleScript : MonoBehaviour
{
    public TypeWriter TypeWriter;
    public TextMeshProUGUI TMP;

    // Update is called once per frame
    private void Start()
    {
        TMP.text = TypeWriter.TargetText;
    }
    void Update()
    {
        TMP.maxVisibleCharacters = TypeWriter.CurrentCharactersCount;            
    }
}
