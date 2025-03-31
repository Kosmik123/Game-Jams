using UnityEngine;

public class BoxMaterials : MonoBehaviour
{
	[SerializeField]
	protected FacesProperties<MeshRenderer> faces;
	public FacesProperties<MeshRenderer> Faces => faces;

	[SerializeField]
	protected FacesProperties<Material> materials;
	public FacesProperties<Material> Materials => materials;

	[System.Serializable]
	public class FacesProperties<T>
		where T : Object
	{
		public T front;
		public T back;
		public T left;
		public T right;
		public T top;
		public T bottom;
	}

	[ContextMenu("Apply Colors")]
	private void ApplyColors()
	{
		faces.front.sharedMaterial = materials.front;
		faces.back.sharedMaterial = materials.back;
		faces.left.sharedMaterial = materials.left;
		faces.right.sharedMaterial = materials.right;
		faces.top.sharedMaterial = materials.top;
		faces.bottom.sharedMaterial = materials.bottom;
	}
}
