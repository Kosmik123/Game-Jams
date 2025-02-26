using UnityEditor;
using UnityEngine;

namespace Bipolar.Editor
{
    public abstract class ButtonAttributePropertyDrawer : PropertyDrawer
    {
        protected static readonly GUIContent incorrectTypeLabelText = new GUIContent("It is not a UnityEngine.Object!");
        protected const float buttonWidth = 42;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.LabelField(position, label, incorrectTypeLabelText);
                return;
            }

            var objectRect = position;
            objectRect.xMax -= buttonWidth + 1;

            var buttonRect = position;
            buttonRect.xMin = objectRect.xMax + 1;
            buttonRect.width = buttonWidth;

            EditorGUI.ObjectField(objectRect, property);
            if (GUI.Button(buttonRect, ButtonText, EditorStyles.miniButton))
            {
                OnClick();
            }
        }

        protected abstract void OnClick();

        protected abstract string ButtonText { get; }
    }
}
