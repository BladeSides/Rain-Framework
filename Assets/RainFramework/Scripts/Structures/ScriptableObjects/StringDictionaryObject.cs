using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RainFramework.Structures
{
    [CreateAssetMenu]
    public class StringDictionaryObject : ScriptableObject
    {
        public Vector2String[] Dictionary;

        public string GetValue(string key)
        {
            foreach (var item in Dictionary)
            {
                if (item.Key.Equals(key))
                {
                    return(item.Value);
                }
            }

            return null;
        }
    }
}
