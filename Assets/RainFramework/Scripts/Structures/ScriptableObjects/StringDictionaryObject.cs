using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RainFramework.Structures
{
    [CreateAssetMenu(menuName = "RainFramework/Structures/StringDictionaryObject")]
    public class StringDictionaryObject : ScriptableObject
    {
        public SerializableDictionary<string, string> Dictionary;

        public string GetValue(string key)
        {
            return Dictionary[key];
        }
    }
}
