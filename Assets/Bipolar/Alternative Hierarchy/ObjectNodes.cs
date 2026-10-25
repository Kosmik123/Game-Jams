using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Bipolar.AlternativeHierarchy
{
    [DisallowMultipleComponent]
	[AddComponentMenu(Paths.Root + "Alternative Hierarchy Nodes")]
    public class ObjectNodes : MonoBehaviour
    {
        [SerializeField]
        private ObjectNodes parent;
    }

	[CreateAssetMenu(menuName = Paths.Root + "Hierarchy Type")]
	public class HierarchyType : ScriptableObject, IEquatable<HierarchyType>
    {
        [SerializeField, HideInInspector]
        private int index = -1;
        public int Index => index;

        private void Reset() => Initialize();
        private void Awake() => Initialize();

		private void Initialize()
		{
#if UNITY_EDITOR
            index = HierarchyTypes.Add(this);
#endif
        }

		public bool Equals(HierarchyType other)
		{
            return index == other.index;
		}
	}

    public class HierarchyTypes : ScriptableSingleton<HierarchyTypes>
    {
        [SerializeField]
        private List<HierarchyType> typesList = new List<HierarchyType>();

        public static int Add(HierarchyType type) => instance.AddType(type);

        private int AddType(HierarchyType type)
		{
			int index = typesList.IndexOf(type);
			if (index >= 0)
				return index;
			
			typesList.Add(type);
			return typesList.Count;
		}
	}
}
