using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Bipolar.ChangableValues
{
	public class ChangableValueDisplay : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text display;
		
		[SerializeField, Tooltip("Standard C# formatting syntax.")]
		private string format = "{0}";

		[SerializeField]
		private List<IChangableValue> changableValues;
		private readonly List<object> values = new List<object>();

		private void OnEnable()
		{
			foreach (var changableValue in changableValues)
				changableValue.OnValueChanged += ChangableValue_OnValueChanged;
		}

		private void ChangableValue_OnValueChanged(IChangableValue sender)
		{
			values.Clear();
			foreach (var changableValue in changableValues)
				values.Add(changableValue.Value);

			string message = string.Format(format, values.ToArray());
		}

		private void OnDisable()
		{
			foreach (var changableValue in changableValues)
				changableValue.OnValueChanged -= ChangableValue_OnValueChanged;
		}
	}
}
