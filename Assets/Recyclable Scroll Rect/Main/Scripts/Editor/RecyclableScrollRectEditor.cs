//MIT License
//Copyright (c) 2020 Mohammed Iqubal Hussain
//Website : Polyandcode.com 

using UnityEngine.UI;
using UnityEditor.AnimatedValues;
using UnityEditor;
using UnityEngine;
using Bipolar.UI;

namespace PolyAndCode.UI
{
    [CustomEditor(typeof(RecyclableScrollRect), true)]
    [CanEditMultipleObjects]
    /// <summary>
    /// Custom Editor for the Recyclable Scroll Rect Component which is derived from Scroll Rect.
    /// </summary>

    public class RecyclableScrollRectEditor : Editor
    {
        SerializedProperty contentProperty;
        SerializedProperty movementTypeProperty;
        SerializedProperty elasticityProperty;
        SerializedProperty intertiaProperty;
        SerializedProperty decelerationRateProperty;
        SerializedProperty scrollSensitivityProperty;

        SerializedProperty viewportProperty;
        SerializedProperty scrollbarProperty;

        SerializedProperty onValueChangedProperty;

        //inherited
        SerializedProperty cellTemplateProperty;
        SerializedProperty selfInitializeProperty;
        SerializedProperty directionProperty;

        AnimBool showElasticityAnimBool;
        AnimBool showDecelerationRateAnimBool;

        RecyclableScrollRect recyclableScrollRect;

        protected virtual void OnEnable()
        {
            recyclableScrollRect = (RecyclableScrollRect)target;

            contentProperty = serializedObject.FindProperty("m_Content");
            movementTypeProperty = serializedObject.FindProperty("m_MovementType");
            elasticityProperty = serializedObject.FindProperty("m_Elasticity");
            intertiaProperty = serializedObject.FindProperty("m_Inertia");
            decelerationRateProperty = serializedObject.FindProperty("m_DecelerationRate");
            scrollSensitivityProperty = serializedObject.FindProperty("m_ScrollSensitivity");
            viewportProperty = serializedObject.FindProperty("m_Viewport");
            onValueChangedProperty = serializedObject.FindProperty("m_OnValueChanged");

            //Inherited
            cellTemplateProperty = serializedObject.FindProperty("cellTemplate");
            selfInitializeProperty = serializedObject.FindProperty("SelfInitialize");
            directionProperty = serializedObject.FindProperty("direction");

            showElasticityAnimBool = new AnimBool(Repaint);
            showDecelerationRateAnimBool = new AnimBool(Repaint);
            SetAnimBools(true);
        }

        protected virtual void OnDisable()
        {
            showElasticityAnimBool.valueChanged.RemoveListener(Repaint);
            showDecelerationRateAnimBool.valueChanged.RemoveListener(Repaint);
        }

        void SetAnimBools(bool instant)
        {
            SetAnimBool(showElasticityAnimBool, !movementTypeProperty.hasMultipleDifferentValues && movementTypeProperty.enumValueIndex == (int)ScrollRect.MovementType.Elastic, instant);
            SetAnimBool(showDecelerationRateAnimBool, !intertiaProperty.hasMultipleDifferentValues && intertiaProperty.boolValue, instant);
        }

        void SetAnimBool(AnimBool a, bool value, bool instant)
        {
            if (instant)
                a.value = value;
            else
                a.target = value;
        }

        public override void OnInspectorGUI()
        {
            SetAnimBools(false);
            serializedObject.Update();
            bool isVertical = directionProperty.enumValueIndex == (int)ScrollDirection.Vertical;

            EditorGUILayout.PropertyField(contentProperty);
            EditorGUILayout.PropertyField(directionProperty);

            //EditorGUILayout.PropertyField(_selfInitialize);

            EditorGUILayout.PropertyField(movementTypeProperty);
            if (EditorGUILayout.BeginFadeGroup(showElasticityAnimBool.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(elasticityProperty);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.PropertyField(intertiaProperty);
            if (EditorGUILayout.BeginFadeGroup(showDecelerationRateAnimBool.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(decelerationRateProperty);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.PropertyField(scrollSensitivityProperty);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(viewportProperty);
            string scrollbarPropertyName = isVertical ? "m_VerticalScrollbar" : "m_HorizontalScrollbar";
            scrollbarProperty = serializedObject.FindProperty(scrollbarPropertyName);
            EditorGUILayout.PropertyField(scrollbarProperty);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(cellTemplateProperty);
            string title = isVertical ? "Columns" : "Rows";
            recyclableScrollRect.Segments = EditorGUILayout.IntField(title, recyclableScrollRect.Segments);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(onValueChangedProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}