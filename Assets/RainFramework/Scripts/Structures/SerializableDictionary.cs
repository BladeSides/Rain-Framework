using System.Collections.Generic;
using UnityEngine;

namespace RainFramework.Structures
{
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue>: Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<KeyValuePair> _serializedKeyValuePairs = new List<KeyValuePair>();
        
        public void OnAfterDeserialize()
        {
            SynchronizeToSerializedData();
        }
        
        public void OnBeforeSerialize()
        {
            // no-op
        }
        public new void Add(TKey _key, TValue _value)
        {
            for (int i = 0; i <_serializedKeyValuePairs.Count; i++)
            {
                var serializedKeyValuePair = _serializedKeyValuePairs[i];
                
                if (serializedKeyValuePair.Key.Equals(_key))
                {
                    _serializedKeyValuePairs.RemoveAt(i);
                        
                    _serializedKeyValuePairs.Add(new KeyValuePair {Key = _key, Value = _value});
                    
                    this[_key] = _value;
                    
                    #if UNITY_EDITOR
                    Debug.LogWarning($"{_key} already exists, updating value to {_value}");
                    #endif
                    
                    SynchronizeToSerializedData();
                    
                    return;
                }
            }
            _serializedKeyValuePairs.Add(new KeyValuePair {Key = _key, Value = _value});
            this[_key] = _value;
        }
        
        public new bool Remove(TKey _key)
        {
            for (int i = 0; i <_serializedKeyValuePairs.Count; i++)
            {
                var serializedKeyValuePair = _serializedKeyValuePairs[i];
                
                if (serializedKeyValuePair.Key.Equals(_key))
                {
                    _serializedKeyValuePairs.RemoveAt(i);
                    return base.Remove(_key);
                }
            }
            
            #if UNITY_EDITOR
            Debug.LogError($"{_key} does not exist in the dictionary");
            #endif
            
            return false;
        }
        
#if UNITY_EDITOR
        
        public void EditorOnlyAdd(TKey InKey, TValue InValue)
        {
            _serializedKeyValuePairs.Add(new KeyValuePair {Key = InKey, Value = InValue});
            this[InKey] = InValue;
        }
        
#endif

        public void SynchronizeToSerializedData()
        {
            this.Clear(); // Clear the dictionary

            if ((_serializedKeyValuePairs != null))
            {
                int numElements = _serializedKeyValuePairs.Count;
                for (int i = 0; i < numElements; i++)
                {
                    this[_serializedKeyValuePairs[i].Key] = _serializedKeyValuePairs[i].Value;
                }
            }
            else
            {
                _serializedKeyValuePairs = new();
            }
        }

        [System.Serializable]
        public struct KeyValuePair
        {
            public TKey Key;
            public TValue Value;
        }
    }
}