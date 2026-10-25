using UnityEditor;
using UnityEngine;

public class Tester : MonoBehaviour
{
	[SerializeField]
	private Nullable<int> number;

}

[System.Serializable]
public struct Nullable<T> where T : struct
{
	public T Value { get; private set; }
	public bool HasValue { get; private set; }

	public Nullable(T value)
	{
		HasValue = true;
		Value = value;
	}

	public override bool Equals(object other)
	{
		if (other is Nullable<T> nullable)
		{
			return nullable.HasValue == HasValue && nullable.Value.Equals(Value);
		}
		return false;
	}

	public override int GetHashCode() => Value.GetHashCode();

	public T GetValueOrDefault() => GetValueOrDefault(default);

	public T GetValueOrDefault(T defaultValue) => HasValue ? Value : defaultValue;

	public override string ToString() => HasValue ? Value.ToString() : "null";

	public static implicit operator Nullable<T>(T value) => new Nullable<T>(value);

}


//[CustomPropertyDrawer(typeof(Nullable<>))]
public class NullablePropertyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var hasValueProp = property.FindPropertyRelative("hasValue");
		var valueProp = property.FindPropertyRelative("value");
		EditorGUI.BeginProperty(position, label, property);
		position.height = EditorGUIUtility.singleLineHeight;
		hasValueProp.boolValue = EditorGUI.Toggle(position, "Has Value", hasValueProp.boolValue);
		if (hasValueProp.boolValue)
		{
			position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.PropertyField(position, valueProp, new GUIContent("Value"));
		}
		EditorGUI.EndProperty();
	}
}