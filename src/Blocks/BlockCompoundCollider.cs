using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000082 RID: 130
	public class BlockCompoundCollider : Block
	{
		// Token: 0x06000B45 RID: 2885 RVA: 0x0005153B File Offset: 0x0004F93B
		public BlockCompoundCollider(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000B46 RID: 2886 RVA: 0x00051550 File Offset: 0x0004F950
		public override void Play()
		{
			base.Play();
			GameObject gameObject = Blocksworld.compoundColliders[base.BlockType()];
			if (gameObject == null)
			{
				BWLog.Error("Unable to find compound collider resource for Block: " + base.BlockType());
				return;
			}
			this.go.GetComponent<Collider>().enabled = false;
			IEnumerator enumerator = gameObject.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					BoxCollider boxCollider = (BoxCollider)transform.GetComponent<Collider>();
					if (boxCollider == null)
					{
						BWLog.Error("Compound collider resource does not contain BoxColliders for Block: " + base.BlockType());
						break;
					}
					Matrix4x4 matrix4x = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
					Vector3[] vertices = new Vector3[]
					{
						matrix4x.MultiplyPoint3x4(boxCollider.center + 0.5f * new Vector3(boxCollider.size.x, boxCollider.size.y, boxCollider.size.z)),
						matrix4x.MultiplyPoint3x4(boxCollider.center + 0.5f * new Vector3(boxCollider.size.x, boxCollider.size.y, -boxCollider.size.z)),
						matrix4x.MultiplyPoint3x4(boxCollider.center + 0.5f * new Vector3(boxCollider.size.x, -boxCollider.size.y, boxCollider.size.z)),
						matrix4x.MultiplyPoint3x4(boxCollider.center + 0.5f * new Vector3(boxCollider.size.x, -boxCollider.size.y, -boxCollider.size.z)),
						matrix4x.MultiplyPoint3x4(boxCollider.center + 0.5f * new Vector3(-boxCollider.size.x, boxCollider.size.y, boxCollider.size.z)),
						matrix4x.MultiplyPoint3x4(boxCollider.center + 0.5f * new Vector3(-boxCollider.size.x, boxCollider.size.y, -boxCollider.size.z)),
						matrix4x.MultiplyPoint3x4(boxCollider.center + 0.5f * new Vector3(-boxCollider.size.x, -boxCollider.size.y, boxCollider.size.z)),
						matrix4x.MultiplyPoint3x4(boxCollider.center + 0.5f * new Vector3(-boxCollider.size.x, -boxCollider.size.y, -boxCollider.size.z))
					};
					Mesh mesh = new Mesh();
					mesh.vertices = vertices;
					mesh.triangles = new int[]
					{
						0,
						1,
						2,
						3,
						1,
						2,
						0,
						2,
						4,
						6,
						2,
						4,
						1,
						3,
						5,
						7,
						3,
						5,
						0,
						1,
						5,
						5,
						0,
						4,
						2,
						3,
						7,
						7,
						2,
						6,
						5,
						7,
						6,
						4,
						5,
						6
					};
					base.ResizeMeshForBlock(mesh);
					GameObject gameObject2 = new GameObject(this.go.name + " Compound Collider Part");
					gameObject2.transform.parent = this.go.transform;
					gameObject2.transform.localPosition = Vector3.zero;
					gameObject2.transform.localRotation = Quaternion.identity;
					MeshCollider meshCollider = gameObject2.AddComponent<MeshCollider>();
					meshCollider.convex = true;
					meshCollider.sharedMesh = mesh;
					BWSceneManager.AddBlockMap(gameObject2, this);
					this.compoundColliders.Add(gameObject2.GetComponent<Collider>());
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}

		// Token: 0x06000B47 RID: 2887 RVA: 0x000519F8 File Offset: 0x0004FDF8
		public override void Stop(bool resetBlock = true)
		{
			foreach (Collider collider in this.compoundColliders)
			{
				BWSceneManager.RemoveBlockMap(collider.gameObject);
				MeshCollider meshCollider = collider as MeshCollider;
				if (meshCollider != null)
				{
					Mesh sharedMesh = meshCollider.sharedMesh;
					if (sharedMesh != null)
					{
						UnityEngine.Object.Destroy(sharedMesh);
					}
				}
				UnityEngine.Object.Destroy(collider.gameObject);
			}
			this.compoundColliders.Clear();
			this.go.GetComponent<Collider>().enabled = true;
			base.Stop(resetBlock);
		}

		// Token: 0x06000B48 RID: 2888 RVA: 0x00051AB4 File Offset: 0x0004FEB4
		public override List<Collider> GetColliders()
		{
			return this.compoundColliders;
		}

		// Token: 0x040008E6 RID: 2278
		private List<Collider> compoundColliders = new List<Collider>();
	}
}
