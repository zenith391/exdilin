using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x0200007A RID: 122
	public class BlockBillboard : Block
	{
		// Token: 0x06000A97 RID: 2711 RVA: 0x0004C274 File Offset: 0x0004A674
		public BlockBillboard(List<List<Tile>> tiles) : base(tiles)
		{
            Vector3 vector = new Vector3(0.4f, 1f, -4f);
			this.offsetCamera = vector.normalized * 300f;
			this.scaleY = 1f;

			this.parameters = this.GetBillboardParameters();
			if (this.parameters.showLensflare)
			{
				if (BlockBillboard.lensFlarePrefab == null)
				{
					BlockBillboard.lensFlarePrefab = (Resources.Load("Lens flares/Lens Flare") as GameObject);
				}
				this.lensFlareGo = UnityEngine.Object.Instantiate<GameObject>(BlockBillboard.lensFlarePrefab);
				this.lensFlare = this.lensFlareGo.GetComponent<LensFlare>();
				this.lensFlare.enabled = true;
				this.lensFlare.color = this.parameters.lensFlareColor;
				this.lensFlare.brightness = this.parameters.lensFlareBrightness;
			}
			Material material = this.go.GetComponent<Renderer>().material;
			if (null != material)
			{
				if (this.parameters.ignoreFog)
				{
					material.SetFloat("_FogInfluence", 0f);
				}
				if (!this.parameters.mirrorInWater)
				{
					material.SetFloat("_WaterLevel", 0.5f);
				}
			}
		}

		// Token: 0x06000A98 RID: 2712 RVA: 0x0004C3B6 File Offset: 0x0004A7B6
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.go.GetComponent<Collider>().enabled = false;
		}

		// Token: 0x06000A99 RID: 2713 RVA: 0x0004C3D0 File Offset: 0x0004A7D0
		public override void Play()
		{
			base.Play();
			this.go.GetComponent<Collider>().enabled = false;
		}

		// Token: 0x06000A9A RID: 2714 RVA: 0x0004C3EC File Offset: 0x0004A7EC
		private void CreateMesh()
		{
			if (this.mirrorGo == null)
			{
				this.mirrorGo = new GameObject(this.go.name + " Mirror");
				MeshFilter meshFilter = this.mirrorGo.AddComponent<MeshFilter>();
				this.mirrorMesh = new Mesh();
				meshFilter.mesh = this.mirrorMesh;
				MeshRenderer meshRenderer = this.mirrorGo.AddComponent<MeshRenderer>();
				this.mirrorMaterial = this.go.GetComponent<MeshRenderer>().sharedMaterial;
				meshRenderer.sharedMaterial = this.mirrorMaterial;
			}
			this.mirrorMesh.Clear();
			Vector3 vector = base.Scale();
			List<Vector3> list = new List<Vector3>();
			List<Vector2> list2 = new List<Vector2>();
			List<int> list3 = new List<int>();
			List<Vector3> list4 = new List<Vector3>();
			float num = 0.5f * vector.x;
			float num2 = 0.5f * vector.y;
			int num3 = 7;
			int num4 = 7;
			Vector3 up = Vector3.up;
			Vector3 right = Vector3.right;
			Vector3 forward = Vector3.forward;
			float num5 = vector.x / (float)num3;
			float num6 = vector.y / (float)num4;
			for (int i = 0; i <= num3; i++)
			{
				float d = (float)i * num5 - num;
				for (int j = 0; j <= num4; j++)
				{
					float d2 = -num6 * (float)j + num2;
					Vector3 item = right * d + up * d2;
					Vector3 v = new Vector2((float)i / (float)num3, (float)j / (float)num4);
					list.Add(item);
					list2.Add(v);
					list4.Add(forward);
				}
			}
			int num7 = num3 + 1;
			for (int k = 0; k < num3; k++)
			{
				for (int l = 0; l < num4; l++)
				{
					list3.AddRange(new int[]
					{
						k + l * num7,
						k + 1 + l * num7,
						k + 1 + (l + 1) * num7,
						k + l * num7,
						k + 1 + (l + 1) * num7,
						k + (l + 1) * num7
					});
				}
			}
			this.mirrorMesh.vertices = list.ToArray();
			this.mirrorMesh.uv = list2.ToArray();
			this.mirrorMesh.triangles = list3.ToArray();
			this.mirrorMesh.normals = list4.ToArray();
		}

		// Token: 0x06000A9B RID: 2715 RVA: 0x0004C668 File Offset: 0x0004AA68
		public override void Destroy()
		{
			base.Destroy();
			if (this.mirrorMesh != null)
			{
				UnityEngine.Object.Destroy(this.mirrorMesh);
			}
			if (this.mirrorGo != null)
			{
				UnityEngine.Object.Destroy(this.mirrorGo);
			}
			if (this.lensFlareGo != null)
			{
				UnityEngine.Object.Destroy(this.lensFlareGo);
			}
		}

		// Token: 0x06000A9C RID: 2716 RVA: 0x0004C6D0 File Offset: 0x0004AAD0
		public override bool MoveTo(Vector3 pos)
		{
			bool result = base.MoveTo(pos);
			BillboardParameters billboardParameters = this.GetBillboardParameters();
			float d = 300f;
			if (billboardParameters != null)
			{
				d = billboardParameters.realDistance;
			}
			this.offsetCamera = pos.normalized * d;
			return result;
		}

		// Token: 0x06000A9D RID: 2717 RVA: 0x0004C71C File Offset: 0x0004AB1C
		public override bool ScaleTo(Vector3 scale, bool recalculateCollider = true, bool forceRescale = false)
		{
			bool result = base.ScaleTo(scale, recalculateCollider, forceRescale);
			this.CreateMesh();
			this.scaleY = scale.y;
			return result;
		}

		// Token: 0x06000A9E RID: 2718 RVA: 0x0004C748 File Offset: 0x0004AB48
		private void UpdateMirror(bool showMirror)
		{
			if (showMirror)
			{
				Transform goT = this.goT;
				Vector3 position = goT.position;
				BlockWater worldOceanBlock = Blocksworld.worldOceanBlock;
				Bounds waterBounds = worldOceanBlock.GetWaterBounds();
				float num = waterBounds.max.y + worldOceanBlock.WaterLevelOffset(position);
				float d = position.y - num;
				Vector3 vector = position - Vector3.up * d * 2f;
				float magnitude = this.offsetCamera.magnitude;
				Vector3 cameraPosition = Blocksworld.cameraPosition;
				Vector3 normalized = (vector - cameraPosition).normalized;
				vector = cameraPosition + normalized * magnitude;
				if (vector.y > num)
				{
					Plane plane = new Plane(Vector3.up, waterBounds.max);
					float d2;
					if (plane.Raycast(new Ray(cameraPosition, normalized), out d2))
					{
						vector = cameraPosition + normalized * d2;
					}
				}
				Transform transform = this.mirrorGo.transform;
				transform.position = vector;
				this.mirrorMaterial.SetFloat("_WaterLevel", num);
			}
		}

		// Token: 0x06000A9F RID: 2719 RVA: 0x0004C868 File Offset: 0x0004AC68
		public override void Update()
		{
			base.Update();
			Transform goT = this.goT;
			bool flag = BlockAbstractWater.CameraWithinAnyWater();
			bool enabled = !flag;
			bool flag2 = this.parameters.mirrorInWater && !flag && Blocksworld.worldOcean != null && Blocksworld.worldOceanBlock.isReflective;
			Vector3 cameraPosition = Blocksworld.cameraPosition;
			goT.LookAt(cameraPosition, Vector3.up);
			if (flag2)
			{
				this.mirrorGo.transform.LookAt(cameraPosition, Vector3.up);
			}
			Vector3 position = cameraPosition + this.offsetCamera;
			if (this.parameters.snapHorizon)
			{
				float num = 0f;
				if (Blocksworld.worldOcean != null)
				{
					num = Blocksworld.worldOcean.transform.position.y + 0.5f;
				}
				position.y = num + this.scaleY * 0.5f;
			}
			if (this.parameters.mirrorInWater && position.y < this.scaleY * 0.5f)
			{
				position.y = this.scaleY * 0.5f;
			}
			goT.position = position;
			if (this.parameters.showLensflare)
			{
				Transform transform = this.lensFlareGo.transform;
				transform.position = cameraPosition + this.offsetCamera * 0.9f;
				Color color = Blocksworld.dynamicLightColor * this.parameters.lensFlareColor;
				this.lensFlare.color = color;
				this.lensFlare.enabled = enabled;
			}
			this.UpdateMirror(flag2);
			this.mirrorGo.GetComponent<Renderer>().enabled = flag2;
		}

		// Token: 0x06000AA0 RID: 2720 RVA: 0x0004CA24 File Offset: 0x0004AE24
		public BillboardParameters GetBillboardParameters()
		{
			BillboardParameters component = this.go.GetComponent<BillboardParameters>();
			if (component == null)
			{
				BWLog.Info("Every billboard block needs a BillboardParameters script component");
			}
			return component;
		}

		// Token: 0x04000856 RID: 2134
		private Vector3 offsetCamera;

		// Token: 0x04000857 RID: 2135
		private GameObject mirrorGo;

		// Token: 0x04000858 RID: 2136
		private Mesh mirrorMesh;

		// Token: 0x04000859 RID: 2137
		private GameObject lensFlareGo;

		// Token: 0x0400085A RID: 2138
		private LensFlare lensFlare;

		// Token: 0x0400085B RID: 2139
		private Renderer lensFlareRenderer;

		// Token: 0x0400085C RID: 2140
		private Material mirrorMaterial;

		// Token: 0x0400085D RID: 2141
		private BillboardParameters parameters;

		// Token: 0x0400085E RID: 2142
		private static GameObject lensFlarePrefab;

		// Token: 0x0400085F RID: 2143
		private float scaleY;
	}
}
