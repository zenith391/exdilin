using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000081 RID: 129
	public class BlockCloud : BlockTerrain
	{
		// Token: 0x06000B37 RID: 2871 RVA: 0x0005105C File Offset: 0x0004F45C
		public BlockCloud(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000B38 RID: 2872 RVA: 0x00051078 File Offset: 0x0004F478
		public new static void Register()
		{
			PredicateRegistry.Add<BlockCloud>("Cloud.Intangible", null, (Block b) => new PredicateActionDelegate(((BlockCloud)b).SetTrigger), null, null, null);
			List<List<Tile>> value = new List<List<Tile>>
			{
				new List<Tile>
				{
					Block.ThenTile(),
					new Tile(new GAF("Cloud.Intangible", new object[0]))
				},
				Block.EmptyTileRow()
			};
			Block.defaultExtraTiles["Cloud 1"] = value;
			Block.defaultExtraTiles["Cloud 2"] = value;
		}

		// Token: 0x06000B39 RID: 2873 RVA: 0x00051118 File Offset: 0x0004F518
		public override TileResultCode SetTrigger(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return base.SetTrigger(eInfo, args);
		}

		// Token: 0x06000B3A RID: 2874 RVA: 0x0005112F File Offset: 0x0004F52F
		public override void Play()
		{
			base.Play();
		}

		// Token: 0x06000B3B RID: 2875 RVA: 0x00051138 File Offset: 0x0004F538
		public override void Update()
		{
			base.Update();
			Vector3 cameraPosition = Blocksworld.cameraPosition;
			Collider component = this.go.GetComponent<Collider>();
			if (component != null)
			{
				this.intangible = component.isTrigger;
				Vector3 cameraForward = Blocksworld.cameraForward;
				float num = Util.MaxComponent(this.size);
				Vector3 a = cameraPosition + cameraForward;
				float num2 = this.insideDist;
				if (a + cameraForward * num != -cameraForward)
				{
					RaycastHit raycastHit;
					RaycastHit raycastHit2;
					if (component.Raycast(new Ray(a + cameraForward * num, -cameraForward), out raycastHit, num) && component.Raycast(new Ray(a - cameraForward * num, cameraForward), out raycastHit2, num))
					{
						this.insideDist = (a - raycastHit.point).magnitude;
						this.camWithinCloud = true;
					}
					else
					{
						this.insideDist = 0f;
						this.camWithinCloud = false;
					}
				}
				bool flag = this.camWithinCloud != this.camWasWithinCloud || this.dontKnowWhetherCamWasWithinCloudOrNot;
				this.dontKnowWhetherCamWasWithinCloudOrNot = false;
				if (flag || Mathf.Abs(num2 - this.insideDist) > 0.02f)
				{
					float insideFraction = this.GetInsideFraction();
					Blocksworld.mainCamera.backgroundColor = this.lightTint * (insideFraction * this.lightTint + (1f - insideFraction) * Color.white);
					Blocksworld.worldSky.go.GetComponent<Renderer>().enabled = ((!this.camWithinCloud || !this.intangible) && !Blocksworld.renderingSkybox);
					Blocksworld.UpdateDynamicalLights(true, false);
				}
				if (flag)
				{
					this.go.GetComponent<Renderer>().enabled = !this.camWithinCloud;
				}
			}
			this.camWasWithinCloud = this.camWithinCloud;
		}

		// Token: 0x06000B3C RID: 2876 RVA: 0x00051328 File Offset: 0x0004F728
		public override void Destroy()
		{
			base.Destroy();
			if (Blocksworld.worldSky != null && Blocksworld.worldSky.go != null)
			{
				Blocksworld.worldSky.go.GetComponent<Renderer>().enabled = !Blocksworld.renderingSkybox;
				Blocksworld.UpdateDynamicalLights(true, false);
			}
		}

		// Token: 0x06000B3D RID: 2877 RVA: 0x0005137D File Offset: 0x0004F77D
		public override bool HasDynamicalLight()
		{
			return true;
		}

		// Token: 0x06000B3E RID: 2878 RVA: 0x00051380 File Offset: 0x0004F780
		public override Color GetDynamicalLightTint()
		{
			if (this.camWithinCloud && this.intangible)
			{
				float insideFraction = this.GetInsideFraction();
				return insideFraction * this.lightTint + (1f - insideFraction) * Color.white;
			}
			return Color.white;
		}

		// Token: 0x06000B3F RID: 2879 RVA: 0x000513D4 File Offset: 0x0004F7D4
		public override Color GetFogColorOverride()
		{
			if (this.camWithinCloud && this.intangible)
			{
				float insideFraction = this.GetInsideFraction();
				return insideFraction * this.lightTint + (1f - insideFraction) * BlockSky.GetFogColor();
			}
			return Color.white;
		}

		// Token: 0x06000B40 RID: 2880 RVA: 0x00051426 File Offset: 0x0004F826
		private float GetInsideFraction()
		{
			return Mathf.Min(1f, this.insideDist * 0.25f);
		}

		// Token: 0x06000B41 RID: 2881 RVA: 0x0005143E File Offset: 0x0004F83E
		public override float GetFogMultiplier()
		{
			if (this.camWithinCloud && this.intangible)
			{
				return 1f - this.GetInsideFraction() * 0.85f;
			}
			return 1f;
		}

		// Token: 0x06000B42 RID: 2882 RVA: 0x00051470 File Offset: 0x0004F870
		public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
		{
			TileResultCode result = base.PaintTo(paint, permanent, meshIndex);
			Vector4 a = this.lightTint;
			this.lightTint = this.go.GetComponent<Renderer>().sharedMaterial.color;
			this.lightTint = 0.5f * this.lightTint + 0.5f * Color.white;
			Vector4 b = this.lightTint;
			if (this.camWithinCloud && (a - b).sqrMagnitude > 0.001f)
			{
				Blocksworld.UpdateDynamicalLights(true, false);
			}
			return result;
		}

		// Token: 0x06000B43 RID: 2883 RVA: 0x00051510 File Offset: 0x0004F910
		public override bool IsSolidTerrain()
		{
			return Blocksworld.playFixedUpdateCounter != 0 && !this.intangible;
		}

		// Token: 0x040008DF RID: 2271
		private Color lightTint = Color.white;

		// Token: 0x040008E0 RID: 2272
		private bool camWithinCloud;

		// Token: 0x040008E1 RID: 2273
		private bool camWasWithinCloud;

		// Token: 0x040008E2 RID: 2274
		private bool dontKnowWhetherCamWasWithinCloudOrNot = true;

		// Token: 0x040008E3 RID: 2275
		private bool intangible;

		// Token: 0x040008E4 RID: 2276
		private float insideDist;
	}
}
