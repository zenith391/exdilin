using System;
using UnityEngine;

// Token: 0x02000025 RID: 37
public class Pose3D
{
	// Token: 0x0600012C RID: 300 RVA: 0x00008808 File Offset: 0x00006C08
	public Pose3D()
	{
		this.Position = Vector3.zero;
		this.Orientation = Quaternion.identity;
		this.Matrix = Matrix4x4.identity;
	}

	// Token: 0x0600012D RID: 301 RVA: 0x00008831 File Offset: 0x00006C31
	public Pose3D(Vector3 position, Quaternion orientation)
	{
		this.Set(position, orientation);
	}

	// Token: 0x0600012E RID: 302 RVA: 0x00008841 File Offset: 0x00006C41
	public Pose3D(Matrix4x4 matrix)
	{
		this.Set(matrix);
	}

	// Token: 0x17000033 RID: 51
	// (get) Token: 0x0600012F RID: 303 RVA: 0x00008850 File Offset: 0x00006C50
	// (set) Token: 0x06000130 RID: 304 RVA: 0x00008858 File Offset: 0x00006C58
	public Vector3 Position { get; protected set; }

	// Token: 0x17000034 RID: 52
	// (get) Token: 0x06000131 RID: 305 RVA: 0x00008861 File Offset: 0x00006C61
	// (set) Token: 0x06000132 RID: 306 RVA: 0x00008869 File Offset: 0x00006C69
	public Quaternion Orientation { get; protected set; }

	// Token: 0x17000035 RID: 53
	// (get) Token: 0x06000133 RID: 307 RVA: 0x00008872 File Offset: 0x00006C72
	// (set) Token: 0x06000134 RID: 308 RVA: 0x0000887A File Offset: 0x00006C7A
	public Matrix4x4 Matrix { get; protected set; }

	// Token: 0x17000036 RID: 54
	// (get) Token: 0x06000135 RID: 309 RVA: 0x00008883 File Offset: 0x00006C83
	public Matrix4x4 RightHandedMatrix
	{
		get
		{
			return Pose3D.flipZ * this.Matrix * Pose3D.flipZ;
		}
	}

	// Token: 0x06000136 RID: 310 RVA: 0x0000889F File Offset: 0x00006C9F
	protected void Set(Vector3 position, Quaternion orientation)
	{
		this.Position = position;
		this.Orientation = orientation;
		this.Matrix = Matrix4x4.TRS(position, orientation, Vector3.one);
	}

	// Token: 0x06000137 RID: 311 RVA: 0x000088C4 File Offset: 0x00006CC4
	protected void Set(Matrix4x4 matrix)
	{
		this.Matrix = matrix;
		this.Position = matrix.GetColumn(3);
		this.Orientation = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
	}

	// Token: 0x04000175 RID: 373
	protected static readonly Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1f, 1f, -1f));
}
