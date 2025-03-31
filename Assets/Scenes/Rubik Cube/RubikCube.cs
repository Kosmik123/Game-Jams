using System.Collections.Generic;
using UnityEngine;

public class RubikCube : MonoBehaviour
{
	[SerializeField]
	protected Vector3Int dimensions;
	public Vector3Int Dimensions => dimensions;

	protected Vector3Int previousDimensions;

	[SerializeField]
	private SubCube boxPrototype;
	[SerializeField]
	private Material insideMaterial;

	private SubCube[,,] subCubesByIndex;
	private SubCube[] allSubCubes;
	public IReadOnlyList<SubCube> AllSubcubes => allSubCubes;

	private void Awake()
	{
		subCubesByIndex = new SubCube[dimensions.z, dimensions.y, dimensions.x];

		int allSubcubesCount = dimensions.x * dimensions.y * dimensions.z - ((dimensions.x - 2) * (dimensions.y - 2) * (dimensions.z - 2));
		allSubCubes = new SubCube[allSubcubesCount];
	}

	private void Start()
	{
		RefreshCube();
		foreach (var subcube in GetComponentsInChildren<SubCube>(true))
			if (subcube.gameObject.activeSelf == false)
				Destroy(subcube.gameObject);
	}

	public void ResetParents()
	{
		foreach (var subcube in subCubesByIndex)
			if (subcube)
				subcube.transform.parent = transform;
	}	

	public void ResetTransforms()
	{
		int zLength = subCubesByIndex.GetLength(2);
		int yLength = subCubesByIndex.GetLength(1);
		int xLength = subCubesByIndex.GetLength(0);
		for (int z = 0; z < zLength; z++)
		{
			for (int y = 0; y < yLength; y++)
			{
				for (int x = 0; x < xLength; x++)
				{
					var subcube = subCubesByIndex[z, y, x];
					if (subcube)
					{
						Vector3 position = IndexToPosition(x, y, z);
						subcube.transform.localPosition = position;
					}
				}
			}
		}
	}

	public Vector3 IndexToPosition(int x, int y, int z) => IndexToPosition(new Vector3Int(x, y, z));

	public Vector3 IndexToPosition(Vector3Int index) => index - (dimensions - Vector3.one) / 2f;

	public Vector3Int PositionToIndex(Vector3 position)
	{
		Vector3 halfDimensions = (dimensions - Vector3.one) / 2f;
		position += halfDimensions;
		var index = Vector3Int.RoundToInt(position);
		index.Clamp(Vector3Int.zero, dimensions - Vector3Int.one);
		return index;
	}

	private void OnValidate()
	{
		if (previousDimensions != dimensions)
		{
			if (Application.isPlaying == false)
				RefreshCube();

			previousDimensions = dimensions;
		}
	}

	[ContextMenu("Refresh")]
	private void RefreshCube()
	{
		var cubesPool = new List<SubCube>();
		cubesPool.AddRange(GetComponentsInChildren<SubCube>());
		foreach (var subCube in cubesPool)
			subCube.gameObject.SetActive(false);

		var halfDimentions = (dimensions - Vector3.one) / 2f;
		int index = 0;
		for (int y = 0; y < dimensions.y; y++)
		{
			for (int z = 0; z < dimensions.z; z++)
			{
				for (int x = 0; x < dimensions.x; x++)
				{
					if (IsSurface(x, 0) || IsSurface(y, 1) || IsSurface(z, 2))
					{
						var position = new Vector3(x, y, z) - halfDimentions;
						var subCube = GetSubCube();
						subCube.name = $"Subcube ({x}, {y}, {z})";
						subCube.transform.SetLocalPositionAndRotation(position, Quaternion.identity);
						if (Application.isPlaying)
						{
							subCubesByIndex[z, y, x] = subCube;
							allSubCubes[index] = subCube;
							index++;
						}
						var visuals = subCube.BoxMaterials;
						visuals.Faces.bottom.sharedMaterial = (y == 0)
							? visuals.Materials.bottom
							: insideMaterial;
						visuals.Faces.top.sharedMaterial = (y == dimensions.y - 1)
							? visuals.Materials.top
							: insideMaterial;

						visuals.Faces.left.sharedMaterial = (x == 0)
							? visuals.Materials.left
							: insideMaterial;
						visuals.Faces.right.sharedMaterial = (x == dimensions.y - 1)
							? visuals.Materials.right
							: insideMaterial;

						visuals.Faces.back.sharedMaterial = (z == 0)
							? visuals.Materials.back
							: insideMaterial;
						visuals.Faces.front.sharedMaterial = (z == dimensions.z - 1)
							? visuals.Materials.front
							: insideMaterial;
					}
				}
			}
		}

		SubCube GetSubCube()
		{
			if (cubesPool.Count > 0)
			{
				var subCube = cubesPool[0];
				cubesPool.RemoveAt(0);
				if (subCube && subCube.gameObject)
				{
					subCube.gameObject.SetActive(true);
					return subCube;
				}
			}

			return CreateBox();
		}
	}

	private bool IsSurface(int value, int axisIndex)
	{
		return value == 0 || value == dimensions[axisIndex] - 1;
	}

	private SubCube CreateBox()
	{
		if (Application.isPlaying)
			return Instantiate(boxPrototype, transform);
		else
		{
#if UNITY_EDITOR
			return UnityEditor.PrefabUtility.InstantiatePrefab(boxPrototype, transform) as SubCube;
		}
#else
			return null;
#endif
	}

	public IEnumerable<SubCube> GetSubcubesSlice(Vector3Int referenceCubeIndex, int axisIndex)
	{
		int firstAxisIndex = (axisIndex + 1) % 3;
		int secondAxisIndex = (axisIndex + 2) % 3;

		int firstLength = subCubesByIndex.GetLength(firstAxisIndex);
		int secondLength = subCubesByIndex.GetLength(secondAxisIndex);

		Vector3Int subCubeIndex = Vector3Int.zero;
		subCubeIndex[axisIndex] = referenceCubeIndex[axisIndex];
		for (int j = 0; j < secondLength; j++)
		{
			for (int i = 0; i < firstLength; i++)
			{
				subCubeIndex[firstAxisIndex] = i;
				subCubeIndex[secondAxisIndex] = j;
				var subCube = subCubesByIndex[subCubeIndex.z, subCubeIndex.y, subCubeIndex.x];
				if (subCube)
					yield return subCube;
			}
		}
	}

	public void RecalculateIndices()
	{
		foreach (var subcube in allSubCubes)
		{
			Transform subcubeTransform = subcube.transform;
			var index = PositionToIndex(subcubeTransform.localPosition);
			subCubesByIndex[index.z, index.y, index.x] = subcube;

			subcubeTransform.localPosition = IndexToPosition(index);
			subcubeTransform.localScale = Vector3.one;
		}
	}

	public bool IsSolved()
	{
		Quaternion rotation = allSubCubes[0].transform.localRotation;
		foreach (var subcube in allSubCubes)
		{
			var subcubeTransform = subcube.transform;
			if (subcubeTransform.localRotation != rotation)
				return false;
		}

		return true;
	}
}
