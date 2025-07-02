using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockCompoundCollider : Block
{
	private List<Collider> compoundColliders = new List<Collider>();

	public BlockCompoundCollider(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public override void Play()
	{
		base.Play();
		GameObject gameObject = Blocksworld.compoundColliders[BlockType()];
		if (gameObject == null)
		{
			BWLog.Error("Unable to find compound collider resource for Block: " + BlockType());
			return;
		}
		go.GetComponent<Collider>().enabled = false;
		foreach (object item in gameObject.transform)
		{
			Transform transform = (Transform)item;
			BoxCollider boxCollider = (BoxCollider)transform.GetComponent<Collider>();
			if (boxCollider == null)
			{
				BWLog.Error("Compound collider resource does not contain BoxColliders for Block: " + BlockType());
				break;
			}
			Matrix4x4 matrix4x = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
			Vector3[] vertices = new Vector3[8]
			{
				matrix4x.MultiplyPoint3x4(boxCollider.center + 0.5f * new Vector3(boxCollider.size.x, boxCollider.size.y, boxCollider.size.z)),
				matrix4x.MultiplyPoint3x4(boxCollider.center + 0.5f * new Vector3(boxCollider.size.x, boxCollider.size.y, 0f - boxCollider.size.z)),
				matrix4x.MultiplyPoint3x4(boxCollider.center + 0.5f * new Vector3(boxCollider.size.x, 0f - boxCollider.size.y, boxCollider.size.z)),
				matrix4x.MultiplyPoint3x4(boxCollider.center + 0.5f * new Vector3(boxCollider.size.x, 0f - boxCollider.size.y, 0f - boxCollider.size.z)),
				matrix4x.MultiplyPoint3x4(boxCollider.center + 0.5f * new Vector3(0f - boxCollider.size.x, boxCollider.size.y, boxCollider.size.z)),
				matrix4x.MultiplyPoint3x4(boxCollider.center + 0.5f * new Vector3(0f - boxCollider.size.x, boxCollider.size.y, 0f - boxCollider.size.z)),
				matrix4x.MultiplyPoint3x4(boxCollider.center + 0.5f * new Vector3(0f - boxCollider.size.x, 0f - boxCollider.size.y, boxCollider.size.z)),
				matrix4x.MultiplyPoint3x4(boxCollider.center + 0.5f * new Vector3(0f - boxCollider.size.x, 0f - boxCollider.size.y, 0f - boxCollider.size.z))
			};
			Mesh mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.triangles = new int[36]
			{
				0, 1, 2, 3, 1, 2, 0, 2, 4, 6,
				2, 4, 1, 3, 5, 7, 3, 5, 0, 1,
				5, 5, 0, 4, 2, 3, 7, 7, 2, 6,
				5, 7, 6, 4, 5, 6
			};
			ResizeMeshForBlock(mesh);
			GameObject gameObject2 = new GameObject(go.name + " Compound Collider Part");
			gameObject2.transform.parent = go.transform;
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localRotation = Quaternion.identity;
			MeshCollider meshCollider = gameObject2.AddComponent<MeshCollider>();
			meshCollider.convex = true;
			meshCollider.sharedMesh = mesh;
			BWSceneManager.AddBlockMap(gameObject2, this);
			compoundColliders.Add(gameObject2.GetComponent<Collider>());
		}
	}

	public override void Stop(bool resetBlock = true)
	{
		foreach (Collider compoundCollider in compoundColliders)
		{
			BWSceneManager.RemoveBlockMap(compoundCollider.gameObject);
			MeshCollider meshCollider = compoundCollider as MeshCollider;
			if (meshCollider != null)
			{
				Mesh sharedMesh = meshCollider.sharedMesh;
				if (sharedMesh != null)
				{
					Object.Destroy(sharedMesh);
				}
			}
			Object.Destroy(compoundCollider.gameObject);
		}
		compoundColliders.Clear();
		go.GetComponent<Collider>().enabled = true;
		base.Stop(resetBlock);
	}

	public override List<Collider> GetColliders()
	{
		return compoundColliders;
	}
}
