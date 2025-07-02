using UnityEngine;

public struct Triangle
{
	public readonly Vector3 V1;

	public readonly Vector3 V2;

	public readonly Vector3 V3;

	public readonly Plane P;

	public Triangle(Vector3[] points)
	{
		V1 = points[0];
		V2 = points[1];
		V3 = points[2];
		P = new Plane(V1, V2, V3);
	}

	public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
	{
		V1 = v1;
		V2 = v2;
		V3 = v3;
		P = new Plane(V1, V2, V3);
	}

	public Triangle(Vector3 v1, Vector3 v2, Vector3 v3, Plane plane)
	{
		V1 = v1;
		V2 = v2;
		V3 = v3;
		P = plane;
	}
}
