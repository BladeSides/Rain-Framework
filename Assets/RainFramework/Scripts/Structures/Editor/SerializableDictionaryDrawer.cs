using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace RainFramework.Structures.Editor
{
    public class SerializableDictionaryConvertor<TKey, TValue>: UxmlAttributeConverter<SerializableDictionary<TKey,TValue>>
    {
        static string ValueToString(object inValue)
        {
            return System.Convert.ToString(inValue, CultureInfo.InvariantCulture);
        }

        public override string ToString(SerializableDictionary<TKey, TValue> inSource)
        {
            var DataBuilder = new StringBuilder();

            foreach (var keyValuePair in inSource)
            {
                DataBuilder.Append($"{ValueToString(keyValuePair.Key)}|{ValueToString(keyValuePair.Value)},");
            }

            return DataBuilder.ToString();
        }

        public override SerializableDictionary<TKey, TValue> FromString(string inValue)
        {
            var outputDictionary = new SerializableDictionary<TKey, TValue>();
            var keyValuePairs = inValue.Split(',');
            foreach (var keyValuePair in keyValuePairs)
            {
                var keyValue = keyValuePair.Split('|');
                if (keyValue.Length == 2)
                {
                    var key = (TKey) System.Convert.ChangeType(keyValue[0], typeof(TKey), CultureInfo.InvariantCulture);
                    var value = (TValue) System.Convert.ChangeType(keyValue[1], typeof(TValue), CultureInfo.InvariantCulture);
                    outputDictionary.EditorOnlyAdd(key, value);
                }
            }

            outputDictionary.SynchronizeToSerializedData();

            return outputDictionary;
        }
    }
    
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
    public class SerializableDictionaryDrawer: PropertyDrawer
    {
        private SerializedProperty LinkedProperty;
        private SerializedProperty LinkedKeyValuePairs;

        public override VisualElement CreatePropertyGUI(SerializedProperty inProperty)
        {
            LinkedProperty = inProperty;
            LinkedKeyValuePairs = inProperty.FindPropertyRelative("_serializedKeyValuePairs");

            var containerUI = new Foldout()
            {
                text = inProperty.displayName,
                
                //Remember whether things are collapsed or not
                //https://discussions.unity.com/t/how-to-remember-the-state-value-of-foldouts-when-closing-and-reopening-a-editorwindow/894874#post-9417359
                viewDataKey = $"{inProperty.serializedObject.targetObject.GetInstanceID()}.{inProperty.name}"
            };

            var contentUI = new ListView()
            {
                showAddRemoveFooter = true,
                showBorder = true,
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                showFoldoutHeader = false,
                showBoundCollectionSize = false,
                reorderable = true,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight, //dynamic height
                headerTitle = inProperty.displayName,
                bindingPath = LinkedKeyValuePairs.propertyPath,
                bindItem = BindListItem,
                overridingAddButtonBehavior = OnAddButton,
                onRemove = OnRemove,
            };
            
            containerUI.Add(contentUI);

            var RemoveDuplicatesButton = new Button()
            {
                text = "Remove Duplicates"
            };
            RemoveDuplicatesButton.clicked += OnRemoveDuplicates;
            
            
            containerUI.Add(RemoveDuplicatesButton);

            return containerUI;
        }

        private void OnRemove(BaseListView inListView)
        {
            if (LinkedKeyValuePairs.arraySize > 0)
            {
                int indexToRemove;
                if (inListView.selectedIndex >= 0
                    && inListView.selectedIndex < LinkedKeyValuePairs.arraySize)
                {
                    indexToRemove = inListView.selectedIndex;    
                }
                else
                {
                    indexToRemove = LinkedKeyValuePairs.arraySize - 1;
                }
                
                LinkedKeyValuePairs.DeleteArrayElementAtIndex(indexToRemove);
                
                LinkedProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        private void OnRemoveDuplicates()
        {
            List<int> duplicateIndices = new List<int>();
            
            for (int i = 0; i < LinkedKeyValuePairs.arraySize; i++)
            {
                var keyValueProperty = LinkedKeyValuePairs.GetArrayElementAtIndex(i);
                
                for (int j = i+1; j < LinkedKeyValuePairs.arraySize; j++)
                {
                    var otherKeyValueProperty = LinkedKeyValuePairs.GetArrayElementAtIndex(j);
                    if (keyValueProperty.FindPropertyRelative("Key").boxedValue.Equals(otherKeyValueProperty.FindPropertyRelative("Key").boxedValue)
                        && !duplicateIndices.Contains(j))
                    {
                        duplicateIndices.Add(j);
                    }
                }
            }
            
            for (int i = duplicateIndices.Count - 1; i >= 0; i--)
            {
                LinkedKeyValuePairs.DeleteArrayElementAtIndex(duplicateIndices[i]);
            }

            LinkedProperty.serializedObject.ApplyModifiedProperties();
        }

        void BindListItem(VisualElement inItemUI, int inIndex)
        {
            inItemUI.Clear();
            inItemUI.Unbind(); //Unbind, else TrackPropertyValue will execute even if we delete and readd new element
            
            var keyValueProperty = LinkedKeyValuePairs.GetArrayElementAtIndex(inIndex);

            var keyValueUI = new PropertyField(keyValueProperty) { label = $"<b>{keyValueProperty.FindPropertyRelative("Key").boxedValue}, {keyValueProperty.FindPropertyRelative("Value").boxedValue}" };
            
            inItemUI.Add(keyValueUI);
            
            var WarningUI = new Label("<b>Error: Duplicate keys. Refresh if issue has been fixed.");
            WarningUI.style.color = Color.red;
            
            inItemUI.Add(WarningUI);
            

            WarningUI.visible = AreDuplicateKeys(keyValueProperty, inIndex);

            inItemUI.TrackPropertyValue(keyValueProperty, (SerializedProperty inKeyValueProp) =>
            {
                WarningUI.visible = AreDuplicateKeys(keyValueProperty, inIndex);
                keyValueUI.label = $"<b>{inKeyValueProp.FindPropertyRelative("Key").boxedValue}, {inKeyValueProp.FindPropertyRelative("Value").boxedValue}";
            });
            
            
            inItemUI.Bind(LinkedProperty.serializedObject);
        }

        private bool AreDuplicateKeys(SerializedProperty keyValueProperty, int inIndex)
        {
            // Extract the key from the current keyValueProperty
            var currentKeyValue = keyValueProperty.FindPropertyRelative("Key").boxedValue;

            for (int i = 0; i < LinkedKeyValuePairs.arraySize; i++)
            {
                // Skip the same index
                if (i == inIndex)
                    continue;

                var otherKeyValueProperty = LinkedKeyValuePairs.GetArrayElementAtIndex(i);
                var otherKeyValue = otherKeyValueProperty.FindPropertyRelative("Key").boxedValue;

                if (Equals(currentKeyValue, otherKeyValue))
                {
                    return true;
                }
            }

            return false;
        }


        void OnAddButton(BaseListView inListView, Button inButton)
        {
            LinkedKeyValuePairs.InsertArrayElementAtIndex(LinkedKeyValuePairs.arraySize);
            LinkedProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}
