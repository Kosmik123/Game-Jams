using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ColorValidation
{
    public enum ColorType
    {
        Pink,
        Black,
        Green,
        Red,
        White,
        LightBlue,
        LightSalmon,
    }

    public abstract class ColorValidator<T> : MonoBehaviour  where T : Component
    {
        [SerializeField]
        private ColorNames colorNames;

        [SerializeField]
        private string color;

        protected abstract void ValidateColor(Color color);

        protected void TryValidateColor()
        {
            if (colorNames && colorNames.TryGetColor(this.color, out var color))
                ValidateColor(color);
        }
        private T subject;
        public T Subject
        {
            get
            {
                if (subject == null)
                    subject = GetComponent<T>();
                return subject;
            }
        }

        private void Awake()
        {
            TryValidateColor();
        }

        private void OnValidate()
        {
            TryValidateColor();
        }
    }
#if UNITY_EDITOR

    [CustomEditor(typeof(ColorValidator<>), true)]
    public class ColorValidatorEditor : Editor
    {
        private const string colorSettingsPropertyName = "colorNames";
        private const string colorMappingsPropertyName = "colors";
        private const string colorNamePropertyName = "name";
        private const string colorPropertyName = "color";

        public override void OnInspectorGUI()
        {
            var settingsProperty = serializedObject.FindProperty(colorSettingsPropertyName);
            DisplaySettings(settingsProperty);
            if (settingsProperty != null)
                DisplayColorNamesDropdown(settingsProperty);
            serializedObject.ApplyModifiedProperties();
        }

        private void DisplayColorNamesDropdown(SerializedProperty colorSettingsProperty)
        {
            if (colorSettingsProperty.objectReferenceValue == null)
                return;

            var serializedSettings = new SerializedObject(colorSettingsProperty.objectReferenceValue);
            if (serializedSettings == null)
                return;
            
            var mappingsProperty = serializedSettings.FindProperty(colorMappingsPropertyName);
            if (mappingsProperty == null)
                return;

            int colorsCount = mappingsProperty.arraySize;
            if (colorsCount < 1)
                return;

            var displayedNames = new string[colorsCount];
            for (int i = 0; i < mappingsProperty.arraySize; i++)
            {
                var nameProperty = mappingsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(colorNamePropertyName);
                var name = nameProperty.stringValue;
                displayedNames[i] = name;
            }

            var currentColorNameProperty = serializedObject.FindProperty(colorPropertyName);
            string currentColor = currentColorNameProperty.stringValue;

            int selectedIndex = System.Array.IndexOf(displayedNames, currentColor);
            if (selectedIndex < 0 || selectedIndex >= currentColor.Length)
                selectedIndex = 0;

            int newIndex = EditorGUILayout.Popup("Color", selectedIndex, displayedNames);
            if (newIndex >= 0 && newIndex < colorsCount)
                currentColorNameProperty.stringValue = displayedNames[newIndex];
        }

        protected void DisplaySettings(SerializedProperty settingsProperty)
        {
            EditorGUILayout.PropertyField(settingsProperty);
        }


    }


#endif
}
