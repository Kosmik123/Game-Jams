#if UNITY_2022_1_OR_NEWER
#define HAS_CSHARP_8
#endif

using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Bipolar.Interactions
{
	public interface IInteractionType
	{ }

	public interface IInteractionTypeHolder
	{
#if HAS_CSHARP_8
		System.Type GetInteractionType();
#endif
	}

	public interface IInteractionTypeHolder<TType> : IInteractionTypeHolder
		where TType : IInteractionType
	{
#if HAS_CSHARP_8
		System.Type IInteractionTypeHolder.GetInteractionType() => typeof(TType);
#endif
	}

	public interface IObjectInteraction : IInteractionTypeHolder
	{
	}

	public interface IObjectInteraction<TType> : IObjectInteraction, IInteractionTypeHolder<TType>
		where TType : IInteractionType
	{ }

	public interface IInteractorInteraction : IInteractionTypeHolder
	{ }

	public interface IInteractorInteraction<TType> : IInteractorInteraction, IInteractionTypeHolder<TType>
		where TType : IInteractionType
	{ }

	public class SceneInteractorInteraction : MonoBehaviour, IInteractorInteraction<IInteractionType>
	{
	}

	public class Interactor : MonoBehaviour
	{
		[SerializeField]
		private List<IInteractorInteraction> interactions;

		private readonly Dictionary<System.Type, IInteractorInteraction> interactionsByType = new Dictionary<System.Type, IInteractorInteraction>();

		private void Awake()
		{
			RefreshDictionary();
		}

		private void RefreshDictionary()
		{
			interactionsByType.Clear();
			for (int i = interactions.Count - 1; i >= 0; i--)
			{
				var interaction = interactions[i];
				if (interaction == null)
				{
					interactions.RemoveAt(i);
					continue;
				}

				var type = interaction.GetInteractionType();
				if (type == null || interactionsByType.ContainsKey(type))
				{
					interactions.RemoveAt(i);
					continue;
				}

				interactionsByType.Add(type, interaction);
			}
		}
	}

#if !HAS_CSHARP_8
	public static class GetInteractionTypeExtension
	{
		public static System.Type GetInteractionType(this IInteractionTypeHolder interaction)
		{
			var genericInterface = interaction.GetType()
				.GetInterfaces()
				.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IInteractionTypeHolder<>));

			return genericInterface?.GetGenericArguments().FirstOrDefault();
		}
	}
#endif
}




