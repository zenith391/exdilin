using System;
using UnityEngine;

// Token: 0x02000026 RID: 38
public class MutablePose3D : Pose3D
{
	// Token: 0x0600013A RID: 314 RVA: 0x00008938 File Offset: 0x00006D38
	public new void Set(Vector3 position, Quaternion orientation)
	{
		base.Set(position, orientation);
	}

	// Token: 0x0600013B RID: 315 RVA: 0x00008942 File Offset: 0x00006D42
	public new void Set(Matrix4x4 matrix)
	{
		base.Set(matrix);
	}

	// Token: 0x0600013C RID: 316 RVA: 0x0000894B File Offset: 0x00006D4B
	public void SetRightHanded(Matrix4x4 matrix)
	{
		this.Set(Pose3D.flipZ * matrix * Pose3D.flipZ);
	}
}
