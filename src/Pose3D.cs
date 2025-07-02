using UnityEngine;

public class Pose3D
{
	protected static readonly Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1f, 1f, -1f));

	public Vector3 Position { get; protected set; }

	public Quaternion Orientation { get; protected set; }

	public Matrix4x4 Matrix { get; protected set; }

	public Matrix4x4 RightHandedMatrix => flipZ * Matrix * flipZ;

	public Pose3D()
	{
		Position = Vector3.zero;
		Orientation = Quaternion.identity;
		Matrix = Matrix4x4.identity;
	}

	public Pose3D(Vector3 position, Quaternion orientation)
	{
		Set(position, orientation);
	}

	public Pose3D(Matrix4x4 matrix)
	{
		Set(matrix);
	}

	protected void Set(Vector3 position, Quaternion orientation)
	{
		Position = position;
		Orientation = orientation;
		Matrix = Matrix4x4.TRS(position, orientation, Vector3.one);
	}

	protected void Set(Matrix4x4 matrix)
	{
		Matrix = matrix;
		Position = matrix.GetColumn(3);
		Orientation = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
	}
}
