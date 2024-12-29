using System;
using UnityEngine;

namespace Bipolar.ChangableValues
{
	public interface IChangableValue
	{
		event Action<IChangableValue> OnValueChanged;
		object Value { get; }
	}

	public interface IChangableValue<T> : IChangableValue
	{
		new T Value { get; set; }
	}

	[Serializable]
	public abstract class ChangableValueBase<TChangableValue> : Serialized<TChangableValue>, IChangableValue
		where TChangableValue : class, IChangableValue
	{
		public event Action<IChangableValue> OnValueChanged
		{
			add => Value.OnValueChanged += value;
			remove => Value.OnValueChanged -= value;
		}

		object IChangableValue.Value => Value.Value;
	}

	[Serializable]
	public class ChangableValue<T> : ChangableValueBase<IChangableValue<T>>, IChangableValue<T>
	{
		T IChangableValue<T>.Value
		{
			get => Value.Value;
			set => Value.Value = value;
		}
	}

	[Serializable]
	public class ChangableValue : ChangableValueBase<IChangableValue>, IChangableValue
	{ }

	public class SceneChangableValue<T> : MonoBehaviour, IChangableValue<T>
	{
		public event Action<IChangableValue> OnValueChanged;

		[SerializeField]
		private T value;
		public T Value
		{
			get => value;
			set
			{
				this.value = value;
				OnValueChanged.Invoke(this);
			}
		}

		object IChangableValue.Value => Value;

		protected virtual void OnValidate()
		{
			OnValueChanged.Invoke(this);
		}
	}

	public class GlobalChangableValue<T> : ScriptableObject, IChangableValue<T>
	{
		public event Action<IChangableValue> OnValueChanged;

		[SerializeField]
		private T value;
		public T Value
		{
			get => value;
			set
			{
				this.value = value;
				OnValueChanged.Invoke(this);
			}
		}

		object IChangableValue.Value => Value;

		protected virtual void OnValidate()
		{
			OnValueChanged.Invoke(this);
		}
	}

	public class CreateAssetPath
	{
		public const string Root = "Bipolar/Global Changable Values/";
	}

	[CreateAssetMenu(menuName = CreateAssetPath.Root + "Int Value", order = 0)]
	public class GlobalIntValue : GlobalChangableValue<int>
	{ }
	
	[CreateAssetMenu(menuName = CreateAssetPath.Root + "Float Value", order = 1)]
	public class GlobalFloatValue : GlobalChangableValue<float>
	{ }
	
	[CreateAssetMenu(menuName = CreateAssetPath.Root + "Boolean Value")]
	public class GlobalBoolValue : GlobalChangableValue<bool>
	{ }
	
	[CreateAssetMenu(menuName = CreateAssetPath.Root + "Double Value")]
	public class GlobalDoubleValue : GlobalChangableValue<int>
	{ }

	[CreateAssetMenu(menuName = CreateAssetPath.Root + "Vector3 Value")]
	public class GlobalVector3Value : GlobalChangableValue<Vector3>
	{ }
}
