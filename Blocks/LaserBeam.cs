using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000057 RID: 87
	public class LaserBeam
	{
		// Token: 0x06000730 RID: 1840 RVA: 0x00030E30 File Offset: 0x0002F230
		public LaserBeam(BlockAbstractLaser sender)
		{
			this._sender = sender;
			this._go = new GameObject(sender.go.name + " beam");
			this._mesh = new Mesh();
			this._mesh.name = "Beam Mesh";
			MeshFilter meshFilter = this._go.AddComponent<MeshFilter>();
			meshFilter.mesh = this._mesh;
			this.renderer = this._go.AddComponent<MeshRenderer>();
			this.renderer.material = (Material)Resources.Load("Materials/Laser2");
			Color laserColor = sender.GetLaserColor();
			this.renderer.material.SetColor("_Color", laserColor);
		}

		// Token: 0x06000731 RID: 1841 RVA: 0x00030F20 File Offset: 0x0002F320
		public void Destroy()
		{
			UnityEngine.Object.Destroy(this._go);
			UnityEngine.Object.Destroy(this._mesh);
		}

		// Token: 0x06000732 RID: 1842 RVA: 0x00030F38 File Offset: 0x0002F338
		public void Update(bool paused = false)
		{
			Color laserColor = this._sender.GetLaserColor();
			this.renderer.material.SetColor("_Color", laserColor);
			this.FixedUpdate(true, paused);
			if (this.lastVertices.Count > 0)
			{
				this._mesh.Clear();
				Vector3 b = this.lastOrigin - this._sender.goT.position;
				for (int i = 0; i < this.lastVertices.Count; i++)
				{
					List<Vector3> list;
					int index;
					(list = this.lastVertices)[index = i] = list[index] + b;
				}
				if (this.lastVerticesArr == null || this.lastVerticesArr.Length != this.lastVertices.Count)
				{
					this.lastVerticesArr = this.lastVertices.ToArray();
				}
				else
				{
					int count = this.lastVertices.Count;
					for (int j = 0; j < count; j++)
					{
						this.lastVerticesArr[j] = this.lastVertices[j];
					}
				}
				this._mesh.vertices = this.lastVerticesArr;
				if (this.lastUvsArr == null || this.lastUvsArr.Length != this.lastUvs.Count)
				{
					this.lastUvsArr = this.lastUvs.ToArray();
				}
				else
				{
					int count2 = this.lastUvs.Count;
					for (int k = 0; k < count2; k++)
					{
						this.lastUvsArr[k] = this.lastUvs[k];
					}
				}
				this._mesh.uv = this.lastUvsArr;
				if (this.lastTrianglesArr == null || this.lastTrianglesArr.Length != this.lastTriangles.Count)
				{
					this.lastTrianglesArr = this.lastTriangles.ToArray();
				}
				else
				{
					int count3 = this.lastTriangles.Count;
					for (int l = 0; l < count3; l++)
					{
						this.lastTrianglesArr[l] = this.lastTriangles[l];
					}
				}
				this._mesh.triangles = this.lastTrianglesArr;
			}
		}

		// Token: 0x06000733 RID: 1843 RVA: 0x00031180 File Offset: 0x0002F580
		public void FixedUpdate(bool fromUpdate = false, bool paused = false)
		{
			Transform goT = this._sender.goT;
			this.lastVertices.Clear();
			this.lastUvs.Clear();
			this.lastTriangles.Clear();
			int num = 0;
			Vector3 laserExitOffset = this._sender.laserExitOffset;
			Vector3 right = goT.right;
			Vector3 up = goT.up;
			Vector3 forward = goT.forward;
			Vector3 b = laserExitOffset.x * right + laserExitOffset.y * up + laserExitOffset.z * forward;
			Vector3 vector = goT.position + b;
			Vector3 vector2 = this._sender.GetFireDirectionForward();
			Vector3 vector3 = this._sender.GetFireDirectionUp();
			bool flag;
			do
			{
				Vector3 to = vector + vector2 * 1000f;
				flag = false;
				RaycastHit[] array = Physics.RaycastAll(vector, vector2);
				RaycastHit raycastHit = default(RaycastHit);
				RaycastHit raycastHit2 = default(RaycastHit);
				bool flag2 = false;
				for (int i = 0; i < array.Length; i++)
				{
					float num2 = float.MaxValue;
					int num3 = i;
					for (int j = i; j < array.Length; j++)
					{
						RaycastHit raycastHit3 = array[j];
						float sqrMagnitude = (vector - raycastHit3.point).sqrMagnitude;
						if (sqrMagnitude < num2)
						{
							num2 = sqrMagnitude;
							num3 = j;
						}
					}
					if (!flag2)
					{
						RaycastHit raycastHit4 = array[num3];
						Block block = BWSceneManager.FindBlock(raycastHit4.collider.gameObject, false);
						if (block != null && !(block is BlockSky))
						{
							if (!fromUpdate)
							{
								BlockAbstractLaser.AddHit(this._sender, block);
							}
							string texture = block.GetTexture(0);
							if (!BlockAbstractLaser.IsTransparent(texture))
							{
								to = raycastHit4.point;
								flag2 = true;
								raycastHit2 = raycastHit4;
								if (BlockAbstractLaser.IsReflective(block.GetPaint(0)))
								{
									num++;
									flag = true;
									raycastHit = raycastHit4;
								}
							}
						}
					}
					if (flag2)
					{
						break;
					}
					RaycastHit raycastHit5 = array[num3];
					array[num3] = array[i];
					array[i] = raycastHit5;
				}
				if (fromUpdate)
				{
					float num4 = this._sender.beamStrength * this._sender.beamSizeMultiplier;
					BlockAbstractLaser.DrawLaserLine(vector, to, vector3, this.lastVertices, this.lastUvs, this.lastTriangles, this.sizeMult * num4);
				}
				if (flag2)
				{
					this._sender.UpdateLaserHitParticles(raycastHit2.point, raycastHit2.normal, vector2, false, !flag);
				}
				if (flag)
				{
					vector = raycastHit.point;
					vector2 = Vector3.Reflect(vector2, raycastHit.normal);
					vector -= vector2 * 0.05f;
					vector3 = Vector3.Reflect(vector3, raycastHit.normal);
				}
			}
			while (flag && num < 10);
			this.lastOrigin = goT.position;
			if (!paused)
			{
				this.sizeMult = 1f + 0.15f * Mathf.Sin(Time.time * 20f);
			}
		}

		// Token: 0x0400055E RID: 1374
		private BlockAbstractLaser _sender;

		// Token: 0x0400055F RID: 1375
		private GameObject _go;

		// Token: 0x04000560 RID: 1376
		private Mesh _mesh;

		// Token: 0x04000561 RID: 1377
		private const int kMaxBounces = 10;

		// Token: 0x04000562 RID: 1378
		private const float kNoHitLength = 1000f;

		// Token: 0x04000563 RID: 1379
		public float sizeMult = 1f;

		// Token: 0x04000564 RID: 1380
		private List<Vector3> lastVertices = new List<Vector3>();

		// Token: 0x04000565 RID: 1381
		private List<Vector2> lastUvs = new List<Vector2>();

		// Token: 0x04000566 RID: 1382
		private List<int> lastTriangles = new List<int>();

		// Token: 0x04000567 RID: 1383
		private Vector3 lastOrigin = default(Vector3);

		// Token: 0x04000568 RID: 1384
		private Vector3[] lastVerticesArr;

		// Token: 0x04000569 RID: 1385
		private Vector2[] lastUvsArr;

		// Token: 0x0400056A RID: 1386
		private int[] lastTrianglesArr;

		// Token: 0x0400056B RID: 1387
		private MeshRenderer renderer;
	}
}
