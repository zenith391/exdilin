using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class EmissionVisualEffect : VisualEffect
{
	protected Color _color = Color.white;

	private static Dictionary<Block, ColliderMeshParameters> colliderParameters = new Dictionary<Block, ColliderMeshParameters>();

	public EmissionVisualEffect(string name)
		: base(name)
	{
		_color = Color.white;
	}

	public void SetColor(Color color)
	{
		_color = color;
	}

	public Vector3 CalculateRandomHullPosition(Block ourBlock, bool singleBlock, Vector3 pos, Vector3 theSize)
	{
		if (singleBlock)
		{
			if (!colliderParameters.TryGetValue(ourBlock, out var value))
			{
				List<Collider> colliders = ourBlock.GetColliders();
				if (colliders.Count > 0)
				{
					value = new ColliderMeshParameters();
					value.ourColliders = colliders;
					Collider collider = colliders[0];
					if (collider is MeshCollider)
					{
						MeshCollider meshCollider = (MeshCollider)collider;
						if (meshCollider != null)
						{
							value.colliderType = ColliderTypes.MESH;
							value.colliderMesh = meshCollider.sharedMesh;
							value.colliderMeshVertices = value.colliderMesh.vertices;
							value.colliderMeshTriangles = value.colliderMesh.triangles;
							float[] surfaceAreas = CalculateSurfaceAreas(value.colliderMeshVertices, value.colliderMeshTriangles);
							value.normalizedAreaWeights = NormalizeAreaWeights(surfaceAreas);
						}
					}
					else if (collider is BoxCollider)
					{
						BoxCollider boxCollider = (BoxCollider)collider;
						if (boxCollider != null)
						{
							value.colliderType = ColliderTypes.BOX;
							value.theSize = boxCollider.size;
							value.localOffset = boxCollider.center;
						}
					}
					else if (collider is CapsuleCollider)
					{
						CapsuleCollider capsuleCollider = (CapsuleCollider)collider;
						if (capsuleCollider != null)
						{
							value.colliderType = ColliderTypes.BOX;
							value.theSize = capsuleCollider.bounds.size;
							value.localOffset = capsuleCollider.center;
						}
					}
					else if (collider is SphereCollider)
					{
						SphereCollider sphereCollider = (SphereCollider)collider;
						if (sphereCollider != null)
						{
							value.colliderType = ColliderTypes.SPHERE;
							value.ourRadius = sphereCollider.radius;
						}
					}
					else
					{
						value.colliderType = ColliderTypes.NONE;
					}
				}
				colliderParameters.Add(ourBlock, value);
			}
			return value.colliderType switch
			{
				ColliderTypes.MESH => GenerateRandomPoint(value.normalizedAreaWeights, value.colliderMeshVertices, value.colliderMeshTriangles, ourBlock.goT), 
				ColliderTypes.BOX => GetPointOnCube(value.theSize, ourBlock.goT.rotation, pos, value.localOffset), 
				ColliderTypes.SPHERE => GetPointOnSphere(value.ourRadius, pos), 
				_ => Vector3.zero, 
			};
		}
		return GetPointOnCube(theSize, ourBlock.goT.rotation, pos, Vector3.zero);
	}

	private int SampleSide()
	{
		return Random.Range(0, 6);
	}

	private Vector3 GetPointOnCube(Vector3 theSize, Quaternion ourRotation, Vector3 pos, Vector3 localOffset)
	{
		float num = 0.5f;
		float x = Random.Range(0f - num, num) * theSize.x;
		float y = Random.Range(0f - num, num) * theSize.y;
		float z = Random.Range(0f - num, num) * theSize.z;
		int num2 = SampleSide();
		Vector3 vector = theSize * 0.5f;
		switch (num2)
		{
		case 0:
			x = vector.x;
			break;
		case 1:
			x = 0f - vector.x;
			break;
		case 2:
			y = vector.y;
			break;
		case 3:
			y = 0f - vector.y;
			break;
		case 4:
			z = vector.z;
			break;
		case 5:
			z = 0f - vector.z;
			break;
		}
		return pos + ourRotation * new Vector3(x, y, z) + block.goT.TransformDirection(localOffset);
	}

	private Vector3 GetPointOnSphere(float radius, Vector3 pos)
	{
		Vector3 vector = Random.onUnitSphere * radius;
		return pos + vector;
	}

	private Vector3 GenerateRandomPoint(float[] triangleMesh, Vector3[] vertices, int[] triangles, Transform trans)
	{
		int triangleIndex = SelectRandomTriangle(Random.value, triangleMesh);
		Vector3 barycentric = GenerateRandomBarycentricCoordinates();
		return ConvertToWorldSpace(barycentric, triangleIndex, vertices, triangles, trans);
	}

	private float[] CalculateSurfaceAreas(Vector3[] vertices, int[] triangles)
	{
		int num = triangles.Length / 3;
		float[] array = new float[num];
		for (int i = 0; i < num; i++)
		{
			Vector3 vector = vertices[triangles[i * 3]];
			Vector3 vector2 = vertices[triangles[i * 3 + 1]];
			Vector3 vector3 = vertices[triangles[i * 3 + 2]];
			float magnitude = (vector - vector2).magnitude;
			float magnitude2 = (vector - vector3).magnitude;
			float magnitude3 = (vector2 - vector3).magnitude;
			float num2 = (magnitude + magnitude2 + magnitude3) / 2f;
			array[i] = Mathf.Sqrt(num2 * (num2 - magnitude) * (num2 - magnitude2) * (num2 - magnitude3));
		}
		return array;
	}

	private float[] NormalizeAreaWeights(float[] surfaceAreas)
	{
		float[] array = new float[surfaceAreas.Length];
		float num = 0f;
		for (int i = 0; i < surfaceAreas.Length; i++)
		{
			num += surfaceAreas[i];
		}
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = surfaceAreas[j] / num;
		}
		return array;
	}

	private int SelectRandomTriangle(float triangleSelectionValue, float[] normalizedAreaWeights)
	{
		float num = 0f;
		for (int i = 0; i < normalizedAreaWeights.Length; i++)
		{
			num += normalizedAreaWeights[i];
			if (num >= triangleSelectionValue)
			{
				return i;
			}
		}
		return 0;
	}

	private Vector3 GenerateRandomBarycentricCoordinates()
	{
		Vector3 vector = new Vector3(Random.value, Random.value, Random.value);
		float num = Mathf.Clamp(vector.x + vector.y + vector.z, 0.0001f, 3f);
		return vector / num;
	}

	private Vector3 ConvertToWorldSpace(Vector3 barycentric, int triangleIndex, Vector3[] vertices, int[] triangles, Transform trans)
	{
		Vector3 vector = vertices[triangles[triangleIndex * 3]];
		Vector3 vector2 = vertices[triangles[triangleIndex * 3 + 1]];
		Vector3 vector3 = vertices[triangles[triangleIndex * 3 + 2]];
		return trans.TransformPoint(vector * barycentric.x + vector2 * barycentric.y + vector3 * barycentric.z);
	}

	public override void Clear()
	{
		base.Clear();
		colliderParameters.Clear();
	}
}
