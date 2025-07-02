using System.Collections.Generic;
using UnityEngine;

internal class ColliderMeshParameters
{
	public ColliderTypes colliderType;

	public List<Collider> ourColliders;

	public float[] normalizedAreaWeights;

	public Mesh colliderMesh;

	public Vector3[] colliderMeshVertices;

	public int[] colliderMeshTriangles;

	public float ourRadius;

	public Vector3 theSize;

	public Vector3 localOffset;
}
