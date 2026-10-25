using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.IMGUI;

namespace Bipolar.AlternativeHierarchy
{
    public class AlternativeHierarchyWindow : EditorWindow
    {
		private string searchFilter = string.Empty;

		[MenuItem("Window/Bipolar/Alternative Hierarchy")]
		public static void ShowWindow()
		{
			var window = GetWindow<AlternativeHierarchyWindow>("Alternative Hierarchy");
			var minSize = window.minSize;
			minSize.x = 100;
			window.minSize = minSize;
		}

		private void OnFocus()
		{
			var minSize = this.minSize;
			minSize.x = 200;
			this.minSize = minSize;
		}

		private void OnGUI()
		{
			using var scope = new EditorGUILayout.HorizontalScope();
			EditorGUILayout.Space(10);
			searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField);

			var treeview = new HierarchyTreeView();



		}


	}

	public class HierarchyTreeView : TreeView
	{



	}

















	public static class Paths
    {
        public const string Root = "Bipolar/Alternative Hierarchy/";
    }
}
