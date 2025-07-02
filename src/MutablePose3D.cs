using UnityEngine;

public class MutablePose3D : Pose3D
{
	public new void Set(Vector3 position, Quaternion orientation)
	{
		base.Set(position, orientation);
	}

	public new void Set(Matrix4x4 matrix)
	{
		base.Set(matrix);
	}

	public void SetRightHanded(Matrix4x4 matrix)
	{
		Set(Pose3D.flipZ * matrix * Pose3D.flipZ);
	}
}
