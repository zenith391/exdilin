using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000BF RID: 191
	public class BlockProceduralCollider : Block
	{
		// Token: 0x06000EB7 RID: 3767 RVA: 0x00062EF1 File Offset: 0x000612F1
		public BlockProceduralCollider(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000EB8 RID: 3768 RVA: 0x00062F2E File Offset: 0x0006132E
		public static void ClearReuseableMeshes()
		{
			BlockProceduralCollider.reusableMeshes.Clear();
		}

		// Token: 0x06000EB9 RID: 3769 RVA: 0x00062F3C File Offset: 0x0006133C
		private List<Vector2> GetInitCoordList()
		{
			List<Vector2> list = new List<Vector2>();
			int num = 4;
			for (int i = 0; i < num; i++)
			{
				float x = (float)i / ((float)num - 1f);
				float y = this.Evaluate(x);
				list.Add(new Vector2(x, y));
			}
			return list;
		}

		// Token: 0x06000EBA RID: 3770 RVA: 0x00062F88 File Offset: 0x00061388
		private float GetAbsError(Vector2 left, Vector2 right)
		{
			float x = 0.5f * (right.x + left.x);
			float num = this.Evaluate(x);
			float num2 = 0.5f * (right.y + left.y);
			return Mathf.Abs(num2 - num);
		}

		// Token: 0x06000EBB RID: 3771 RVA: 0x00062FD4 File Offset: 0x000613D4
		private bool Split(List<Vector2> list, float errorLimit)
		{
			float num = 0f;
			int num2 = 0;
			for (int i = 0; i < list.Count - 1; i++)
			{
				Vector2 left = list[i];
				Vector2 right = list[i + 1];
				float num3 = Mathf.Abs(left.x - right.x);
				if (num3 > 0.05f)
				{
					float absError = this.GetAbsError(left, right);
					if (absError > num)
					{
						num2 = i;
						num = absError;
					}
				}
			}
			if (num > errorLimit)
			{
				float x = (list[num2].x + list[num2 + 1].x) * 0.5f;
				Vector2 item = new Vector2(x, this.Evaluate(x));
				list.Insert(num2 + 1, item);
				return true;
			}
			return false;
		}

		// Token: 0x06000EBC RID: 3772 RVA: 0x000630A0 File Offset: 0x000614A0
		public override void Play()
		{
			base.Play();
			this.go.GetComponent<Collider>().enabled = false;
			Vector3 vector = base.Scale();
			string key = base.BlockType() + vector.ToString();
			List<GameObject> list = null;
			if (BlockProceduralCollider.reusableMeshes.ContainsKey(key))
			{
				list = BlockProceduralCollider.reusableMeshes[key];
			}
			this.goColliderParent = new GameObject(this.go.name + " Concave collider parent");
			this.goColliderParent.transform.position = this.goT.position;
			this.goColliderParent.transform.rotation = this.goT.rotation;
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(list[i]);
					gameObject.transform.parent = this.goColliderParent.transform;
					gameObject.transform.localPosition = Vector3.zero;
					gameObject.transform.localRotation = Quaternion.identity;
					MeshCollider component = gameObject.GetComponent<MeshCollider>();
					this.goColliders.Add(gameObject);
					this.goMeshes.Add(component.sharedMesh);
					BWSceneManager.AddBlockMap(gameObject, this);
				}
			}
			else
			{
				float x = vector.x;
				float y = vector.y;
				float z = vector.z;
				int num = Mathf.RoundToInt(Mathf.Min(Mathf.Max(this.scaleFactorZ * vector.z, this.scaleFactorY * vector.y), (float)this.maxSplits));
				List<Vector2> initCoordList = this.GetInitCoordList();
				for (int j = 0; j < num; j++)
				{
					if (!this.Split(initCoordList, 0.05f))
					{
						break;
					}
				}
				for (int k = 0; k < initCoordList.Count - 1; k++)
				{
					List<Vector3> list2 = new List<Vector3>();
					List<int> list3 = new List<int>();
					Mesh mesh = new Mesh();
					mesh.Clear();
					GameObject gameObject2 = new GameObject(this.go.name + " Concave Collider Part");
					gameObject2.transform.parent = this.goColliderParent.transform;
					gameObject2.transform.localPosition = Vector3.zero;
					gameObject2.transform.localRotation = Quaternion.identity;
					MeshCollider meshCollider = gameObject2.AddComponent<MeshCollider>();
					Vector2 vector2 = initCoordList[k];
					Vector2 vector3 = initCoordList[k + 1];
					float num2 = vector2.x;
					float num3 = vector2.y;
					float num4 = vector3.x;
					float num5 = vector3.y;
					num2 -= 0.5f;
					num4 -= 0.5f;
					num3 -= 0.5f;
					num5 -= 0.5f;
					int count = list2.Count;
					list2.AddRange(new Vector3[]
					{
						new Vector3(-0.5f, num3, -num2),
						new Vector3(0.5f, num3, -num2),
						new Vector3(0.5f, num5, -num4),
						new Vector3(-0.5f, num5, -num4),
						new Vector3(-0.5f, -0.5f, -num2),
						new Vector3(0.5f, -0.5f, -num2),
						new Vector3(0.5f, -0.5f, -num4),
						new Vector3(-0.5f, -0.5f, -num4)
					});
					for (int l = 0; l < list2.Count; l++)
					{
						Vector3 value = list2[l];
						value.Scale(new Vector3(x, y, z));
						list2[l] = value;
					}
					this.AddQuad(list3, count, count + 1, count + 2, count + 3);
					this.AddQuad(list3, count, count + 3, count + 7, count + 4);
					this.AddQuad(list3, count + 2, count + 1, count + 3, count + 6);
					list3.Reverse();
					mesh.vertices = list2.ToArray();
					mesh.triangles = list3.ToArray();
					meshCollider.convex = true;
					meshCollider.sharedMesh = mesh;
					this.goColliders.Add(gameObject2);
					this.goMeshes.Add(mesh);
					BWSceneManager.AddBlockMap(gameObject2, this);
				}
				BlockProceduralCollider.reusableMeshes[key] = this.goColliders;
			}
			this.goColliderParent.transform.parent = this.goT;
		}

		// Token: 0x06000EBD RID: 3773 RVA: 0x00063571 File Offset: 0x00061971
		private void AddQuad(List<int> triangles, int i1, int i2, int i3, int i4)
		{
			triangles.AddRange(new int[]
			{
				i1,
				i2,
				i3
			});
			triangles.AddRange(new int[]
			{
				i1,
				i3,
				i4
			});
		}

		// Token: 0x06000EBE RID: 3774 RVA: 0x000635A6 File Offset: 0x000619A6
		protected virtual float Evaluate(float x)
		{
			return 1f;
		}

		// Token: 0x06000EBF RID: 3775 RVA: 0x000635AD File Offset: 0x000619AD
		public override void Stop(bool resetBlock = true)
		{
			this.DestroyColliders();
			base.Stop(resetBlock);
		}

		// Token: 0x06000EC0 RID: 3776 RVA: 0x000635BC File Offset: 0x000619BC
		public override void Destroy()
		{
			this.DestroyColliders();
			base.Destroy();
		}

		// Token: 0x06000EC1 RID: 3777 RVA: 0x000635CC File Offset: 0x000619CC
		private void DestroyColliders()
		{
			BlockProceduralCollider.reusableMeshes.Clear();
			for (int i = 0; i < this.goColliders.Count; i++)
			{
				GameObject gameObject = this.goColliders[i];
				Mesh mesh = this.goMeshes[i];
				if (mesh != null)
				{
					UnityEngine.Object.Destroy(mesh);
				}
				BWSceneManager.RemoveBlockMap(gameObject);
				UnityEngine.Object.Destroy(gameObject);
			}
			UnityEngine.Object.Destroy(this.goColliderParent);
			this.goMeshes.Clear();
			this.goColliders.Clear();
			if (this.go != null && this.go.GetComponent<Collider>() != null)
			{
				this.go.GetComponent<Collider>().enabled = true;
			}
		}

		// Token: 0x06000EC2 RID: 3778 RVA: 0x00063690 File Offset: 0x00061A90
		public void SetLayer(Layer layerEnum)
		{
			for (int i = 0; i < this.goColliders.Count; i++)
			{
				GameObject gameObject = this.goColliders[i];
				gameObject.layer = (int)layerEnum;
			}
			if (this.goColliderParent != null)
			{
				this.goColliderParent.layer = (int)layerEnum;
			}
			this.go.layer = (int)layerEnum;
		}

		// Token: 0x04000B61 RID: 2913
		private List<GameObject> goColliders = new List<GameObject>();

		// Token: 0x04000B62 RID: 2914
		private List<Mesh> goMeshes = new List<Mesh>();

		// Token: 0x04000B63 RID: 2915
		private static Dictionary<string, List<GameObject>> reusableMeshes = new Dictionary<string, List<GameObject>>();

		// Token: 0x04000B64 RID: 2916
		private GameObject goColliderParent;

		// Token: 0x04000B65 RID: 2917
		protected int maxSplits = 10;

		// Token: 0x04000B66 RID: 2918
		protected float scaleFactorZ = 0.25f;

		// Token: 0x04000B67 RID: 2919
		protected float scaleFactorY = 0.25f;
	}
}
