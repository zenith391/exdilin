using Blocks;
using UnityEngine;

public class MeshColliderInfo
{
	public Block block;

	public Mesh mesh;

	public bool convex;

	public PhysicMaterial material;

	public void Restore()
	{
		BoxCollider boxCollider = block.go.GetComponent<Collider>() as BoxCollider;
		if (boxCollider != null)
		{
			Object.Destroy(boxCollider);
		}
		else
		{
			BWLog.Info("Restore() on MeshColliderInfo could not find a BoxCollider to destroy");
		}
		MeshFilter component = block.go.GetComponent<MeshFilter>();
		Mesh sharedMesh = null;
		if (component != null)
		{
			sharedMesh = component.sharedMesh;
			component.sharedMesh = mesh;
		}
		MeshCollider meshCollider = block.go.AddComponent<MeshCollider>();
		meshCollider.sharedMesh = mesh;
		meshCollider.convex = convex;
		meshCollider.material = material;
		if (component != null)
		{
			component.sharedMesh = sharedMesh;
		}
		block.RestoredMeshCollider();
	}
}
