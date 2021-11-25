using System;
using Blocks;
using UnityEngine;

// Token: 0x020001BA RID: 442
public class MeshColliderInfo
{
	// Token: 0x060017E7 RID: 6119 RVA: 0x000A8CB4 File Offset: 0x000A70B4
	public void Restore()
	{
		BoxCollider boxCollider = this.block.go.GetComponent<Collider>() as BoxCollider;
		if (boxCollider != null)
		{
			UnityEngine.Object.Destroy(boxCollider);
		}
		else
		{
			BWLog.Info("Restore() on MeshColliderInfo could not find a BoxCollider to destroy");
		}
		MeshFilter component = this.block.go.GetComponent<MeshFilter>();
		Mesh sharedMesh = null;
		if (component != null)
		{
			sharedMesh = component.sharedMesh;
			component.sharedMesh = this.mesh;
		}
		MeshCollider meshCollider = this.block.go.AddComponent<MeshCollider>();
		meshCollider.sharedMesh = this.mesh;
		meshCollider.convex = this.convex;
		meshCollider.material = this.material;
		if (component != null)
		{
			component.sharedMesh = sharedMesh;
		}
		this.block.RestoredMeshCollider();
	}

	// Token: 0x040012E8 RID: 4840
	public Block block;

	// Token: 0x040012E9 RID: 4841
	public Mesh mesh;

	// Token: 0x040012EA RID: 4842
	public bool convex;

	// Token: 0x040012EB RID: 4843
	public PhysicMaterial material;
}
