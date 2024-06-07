#if UNITY_2019_3_OR_NEWER
using System.Collections.Generic;
using UnityEditor;

namespace MackySoft.SerializeReferenceExtensions.Editor
{
	public static class SerializedPropertyExtensions
	{
		public static IEnumerable<SerializedProperty> GetChildProperties (this SerializedProperty parent, int depth = 1)
		{
			parent = parent.Copy();

			int depthOfParent = parent.depth;
			var enumerator = parent.GetEnumerator();

			while (enumerator.MoveNext())
			{
#if UNITY_2022_1_OR_NEWER
				if (enumerator.Current is not SerializedProperty childProperty)
#else
				if (!(enumerator.Current is SerializedProperty childProperty))
#endif
				{
					continue;
				}
				if (childProperty.depth > (depthOfParent + depth))
				{
					continue;
				}
				yield return childProperty.Copy();
			}
		}
	}
}
#endif