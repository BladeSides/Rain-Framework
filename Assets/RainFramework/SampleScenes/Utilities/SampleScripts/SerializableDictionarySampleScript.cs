using System;
using System.Linq;
using RainFramework.Structures;
using UnityEngine;

public class SerializableDictionarySampleScript : MonoBehaviour
{
    [SerializeField]
    private SerializableDictionary<int, string> _intStringDictionary = new();

    private void Start()
    {
        Debug.LogWarning("Int String Dictionary");
        foreach (var key in _intStringDictionary.Keys)
        {
            Debug.Log($"Key: {key}, Value: {_intStringDictionary[key]}");
        }
        
        _intStringDictionary.Add(2, "I just got added, and now I will delete 0");
        _intStringDictionary.Remove(0);
        
        Debug.LogWarning("Int String Dictionary after Add and Remove");
        foreach (var key in _intStringDictionary.Keys)
        {
            Debug.Log($"Key: {key}, Value: {_intStringDictionary[key]}");
        }
        
        _intStringDictionary.Add(1, "I just got updated");
        
        _intStringDictionary.Remove(999);
        Debug.LogWarning("Dictionary after Update and Removal at non existent item");
        
        foreach (var key in _intStringDictionary.Keys)
        {
            Debug.Log($"Key: {key}, Value: {_intStringDictionary[key]}");
        }
    }
}
