using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x02000339 RID: 825
public class EmissionVisualEffect : VisualEffect
{
	// Token: 0x06002539 RID: 9529 RVA: 0x0010FA51 File Offset: 0x0010DE51
	public EmissionVisualEffect(string name) : base(name)
	{
		this._color = Color.white;
	}

	// Token: 0x0600253A RID: 9530 RVA: 0x0010FA70 File Offset: 0x0010DE70
	public void SetColor(Color color)
	{
		this._color = color;
	}

	// Token: 0x0600253B RID: 9531 RVA: 0x0010FA7C File Offset: 0x0010DE7C
	public Vector3 CalculateRandomHullPosition(Block ourBlock, bool singleBlock, Vector3 pos, Vector3 theSize)
	{
		if (singleBlock)
		{
			ColliderMeshParameters colliderMeshParameters;
			if (!EmissionVisualEffect.colliderParameters.TryGetValue(ourBlock, out colliderMeshParameters))
			{
				List<Collider> colliders = ourBlock.GetColliders();
				if (colliders.Count > 0)
				{
					colliderMeshParameters = new ColliderMeshParameters();
					colliderMeshParameters.ourColliders = colliders;
					Collider collider = colliders[0];
					if (collider is MeshCollider)
					{
						MeshCollider meshCollider = (MeshCollider)collider;
						if (meshCollider != null)
						{
							colliderMeshParameters.colliderType = ColliderTypes.MESH;
							colliderMeshParameters.colliderMesh = meshCollider.sharedMesh;
							colliderMeshParameters.colliderMeshVertices = colliderMeshParameters.colliderMesh.vertices;
							colliderMeshParameters.colliderMeshTriangles = colliderMeshParameters.colliderMesh.triangles;
							float[] surfaceAreas = this.CalculateSurfaceAreas(colliderMeshParameters.colliderMeshVertices, colliderMeshParameters.colliderMeshTriangles);
							colliderMeshParameters.normalizedAreaWeights = this.NormalizeAreaWeights(surfaceAreas);
						}
					}
					else if (collider is BoxCollider)
					{
						BoxCollider boxCollider = (BoxCollider)collider;
						if (boxCollider != null)
						{
							colliderMeshParameters.colliderType = ColliderTypes.BOX;
							colliderMeshParameters.theSize = boxCollider.size;
							colliderMeshParameters.localOffset = boxCollider.center;
						}
					}
					else if (collider is CapsuleCollider)
					{
						CapsuleCollider capsuleCollider = (CapsuleCollider)collider;
						if (capsuleCollider != null)
						{
							colliderMeshParameters.colliderType = ColliderTypes.BOX;
							colliderMeshParameters.theSize = capsuleCollider.bounds.size;
							colliderMeshParameters.localOffset = capsuleCollider.center;
						}
					}
					else if (collider is SphereCollider)
					{
						SphereCollider sphereCollider = (SphereCollider)collider;
						if (sphereCollider != null)
						{
							colliderMeshParameters.colliderType = ColliderTypes.SPHERE;
							colliderMeshParameters.ourRadius = sphereCollider.radius;
						}
					}
					else
					{
						colliderMeshParameters.colliderType = ColliderTypes.NONE;
					}
				}
				EmissionVisualEffect.colliderParameters.Add(ourBlock, colliderMeshParameters);
			}
			switch (colliderMeshParameters.colliderType)
			{
			case ColliderTypes.MESH:
				return this.GenerateRandomPoint(colliderMeshParameters.normalizedAreaWeights, colliderMeshParameters.colliderMeshVertices, colliderMeshParameters.colliderMeshTriangles, ourBlock.goT);
			case ColliderTypes.BOX:
				return this.GetPointOnCube(colliderMeshParameters.theSize, ourBlock.goT.rotation, pos, colliderMeshParameters.localOffset);
			case ColliderTypes.SPHERE:
				return this.GetPointOnSphere(colliderMeshParameters.ourRadius, pos);
			}
			return Vector3.zero;
		}
		return this.GetPointOnCube(theSize, ourBlock.goT.rotation, pos, Vector3.zero);
	}

	// Token: 0x0600253C RID: 9532 RVA: 0x0010FCB3 File Offset: 0x0010E0B3
	private int SampleSide()
	{
		return UnityEngine.Random.Range(0, 6);
	}

	// Token: 0x0600253D RID: 9533 RVA: 0x0010FCBC File Offset: 0x0010E0BC
	private Vector3 GetPointOnCube(Vector3 theSize, Quaternion ourRotation, Vector3 pos, Vector3 localOffset)
	{
		float num = 0.5f;
		float x = UnityEngine.Random.Range(-num, num) * theSize.x;
		float y = UnityEngine.Random.Range(-num, num) * theSize.y;
		float z = UnityEngine.Random.Range(-num, num) * theSize.z;
		int num2 = this.SampleSide();
		Vector3 vector = theSize * 0.5f;
		switch (num2)
		{
		case 0:
			x = vector.x;
			break;
		case 1:
			x = -vector.x;
			break;
		case 2:
			y = vector.y;
			break;
		case 3:
			y = -vector.y;
			break;
		case 4:
			z = vector.z;
			break;
		case 5:
			z = -vector.z;
			break;
		}
		return pos + ourRotation * new Vector3(x, y, z) + this.block.goT.TransformDirection(localOffset);
	}

	// Token: 0x0600253E RID: 9534 RVA: 0x0010FDB8 File Offset: 0x0010E1B8
	private Vector3 GetPointOnSphere(float radius, Vector3 pos)
	{
		Vector3 b = UnityEngine.Random.onUnitSphere * radius;
		return pos + b;
	}

	// Token: 0x0600253F RID: 9535 RVA: 0x0010FDD8 File Offset: 0x0010E1D8
	private Vector3 GenerateRandomPoint(float[] triangleMesh, Vector3[] vertices, int[] triangles, Transform trans)
	{
		int triangleIndex = this.SelectRandomTriangle(UnityEngine.Random.value, triangleMesh);
		Vector3 barycentric = this.GenerateRandomBarycentricCoordinates();
		return this.ConvertToWorldSpace(barycentric, triangleIndex, vertices, triangles, trans);
	}

	// Token: 0x06002540 RID: 9536 RVA: 0x0010FE08 File Offset: 0x0010E208
	private float[] CalculateSurfaceAreas(Vector3[] vertices, int[] triangles)
	{
		int num = triangles.Length / 3;
		float[] array = new float[num];
		for (int i = 0; i < num; i++)
		{
			Vector3 a = vertices[triangles[i * 3]];
			Vector3 vector = vertices[triangles[i * 3 + 1]];
			Vector3 b = vertices[triangles[i * 3 + 2]];
			float magnitude = (a - vector).magnitude;
			float magnitude2 = (a - b).magnitude;
			float magnitude3 = (vector - b).magnitude;
			float num2 = (magnitude + magnitude2 + magnitude3) / 2f;
			array[i] = Mathf.Sqrt(num2 * (num2 - magnitude) * (num2 - magnitude2) * (num2 - magnitude3));
		}
		return array;
	}

	// Token: 0x06002541 RID: 9537 RVA: 0x0010FED4 File Offset: 0x0010E2D4
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

	// Token: 0x06002542 RID: 9538 RVA: 0x0010FF28 File Offset: 0x0010E328
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

	// Token: 0x06002543 RID: 9539 RVA: 0x0010FF60 File Offset: 0x0010E360
	private Vector3 GenerateRandomBarycentricCoordinates()
	{
		Vector3 a = new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
		float d = Mathf.Clamp(a.x + a.y + a.z, 0.0001f, 3f);
		return a / d;
	}

	// Token: 0x06002544 RID: 9540 RVA: 0x0010FFB4 File Offset: 0x0010E3B4
	private Vector3 ConvertToWorldSpace(Vector3 barycentric, int triangleIndex, Vector3[] vertices, int[] triangles, Transform trans)
	{
		Vector3 a = vertices[triangles[triangleIndex * 3]];
		Vector3 a2 = vertices[triangles[triangleIndex * 3 + 1]];
		Vector3 a3 = vertices[triangles[triangleIndex * 3 + 2]];
		return trans.TransformPoint(a * barycentric.x + a2 * barycentric.y + a3 * barycentric.z);
	}

	// Token: 0x06002545 RID: 9541 RVA: 0x00110033 File Offset: 0x0010E433
	public override void Clear()
	{
		base.Clear();
		EmissionVisualEffect.colliderParameters.Clear();
	}

	// Token: 0x04001FD2 RID: 8146
	protected Color _color = Color.white;

	// Token: 0x04001FD3 RID: 8147
	private static Dictionary<Block, ColliderMeshParameters> colliderParameters = new Dictionary<Block, ColliderMeshParameters>();
}
