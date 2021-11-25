using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000090 RID: 144
	public class BlockGaugeUI : BlockAbstractLimitedCounterUI
	{
		// Token: 0x06000BE1 RID: 3041 RVA: 0x000550F4 File Offset: 0x000534F4
		public BlockGaugeUI(List<List<Tile>> tiles, int index) : base(tiles, index)
		{
			this.defaultSingleGaf = new GAF("Block.Create", new object[]
			{
				base.BlockType()
			});
			if (BlockGaugeUI.gaugePrefab == null)
			{
				string str = (!Blocksworld.hd) ? "SD" : "HD";
				string path = "GUI/Gauge " + str;
				UnityEngine.Object @object = Resources.Load(path);
				BlockGaugeUI.gaugePrefab = (@object as GameObject);
			}
			this.gauge = UnityEngine.Object.Instantiate<GameObject>(BlockGaugeUI.gaugePrefab);
			this.gauge.SetActive(false);
			this.gaugeMaterial = this.gauge.GetComponent<MeshRenderer>().material;
		}

		// Token: 0x06000BE2 RID: 3042 RVA: 0x000551AC File Offset: 0x000535AC
		public new static void Register()
		{
			PredicateRegistry.Add<BlockGaugeUI>("GaugeUI.SetMaxValue", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLimitedCounterUI)b).SetMaxValue), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockGaugeUI>("GaugeUI.Flash", null, (Block b) => new PredicateActionDelegate(((BlockAbstractUI)b).Flash), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockGaugeUI>("GaugeUI.SetText", null, (Block b) => new PredicateActionDelegate(((BlockAbstractCounterUI)b).SetText), new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockGaugeUI>("GaugeUI.ShowFractionUI", null, (Block b) => new PredicateActionDelegate(((BlockGaugeUI)b).ShowFractionUI), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockGaugeUI>("GaugeUI.ShowUI", null, (Block b) => new PredicateActionDelegate(((BlockAbstractUI)b).ShowUI), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockGaugeUI>("GaugeUI.ShowPhysical", null, (Block b) => new PredicateActionDelegate(((BlockAbstractUI)b).ShowPhysical), new Type[]
			{
				typeof(int)
			}, null, null);
		}

		// Token: 0x06000BE3 RID: 3043 RVA: 0x00055330 File Offset: 0x00053730
		public override void Play()
		{
			base.Play();
			this.maxValue = 100;
			this.minValue = 0;
			this.showFractionUI = true;
			BlockGaugeUI.allGaugeBlocks[this.index] = this;
			this.gauge.SetActive(true);
			this.gauge.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
			this.gauge.transform.localScale = new Vector3(200f, 1f, 32f);
			this.UpdateGaugeMaterial(this.GetPaint(0));
		}

		// Token: 0x06000BE4 RID: 3044 RVA: 0x000553CC File Offset: 0x000537CC
		public override void Play2()
		{
			base.Play2();
			this.rect = base.GetLayoutRect();
			Vector3 position = new Vector3(this.rect.center.x, (float)NormalizedScreen.height - this.rect.center.y - 20f, 2f);
			this.gauge.transform.position = position;
			this.withinRect = new Rect(position.x - 100f, (float)NormalizedScreen.height - (position.y + 16f), 200f, 32f);
		}

		// Token: 0x06000BE5 RID: 3045 RVA: 0x00055471 File Offset: 0x00053871
		public override void Stop(bool resetBlock)
		{
			base.Stop(resetBlock);
			BlockGaugeUI.allGaugeBlocks.Clear();
			base.DestroyTile(ref this.gaugeTile);
			this.gauge.SetActive(false);
		}

		// Token: 0x06000BE6 RID: 3046 RVA: 0x0005549C File Offset: 0x0005389C
		public TileResultCode ShowFractionUI(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.showFractionUI = Util.GetIntBooleanArg(args, 0, true);
			return TileResultCode.True;
		}

		// Token: 0x06000BE7 RID: 3047 RVA: 0x000554AD File Offset: 0x000538AD
		public override void Destroy()
		{
			base.Destroy();
			if (this.gauge != null)
			{
				UnityEngine.Object.Destroy(this.gauge);
				this.gauge = null;
			}
		}

		// Token: 0x06000BE8 RID: 3048 RVA: 0x000554D8 File Offset: 0x000538D8
		public override void Update()
		{
			base.Update();
			if (Blocksworld.CurrentState == State.Play)
			{
				if (this.dirty)
				{
					this.gaugeMaterial.SetFloat("_Fill", 100f * ((float)this.currentValue / (float)this.maxValue));
				}
				base.UpdateTile(ref this.gaugeTile);
				if (this.gaugeTile != null)
				{
					Vector2 vector = new Vector2(this.rect.x - 40f, (float)NormalizedScreen.height - this.rect.y - 80f);
					this.gaugeTile.MoveTo(vector.x, vector.y, 2f);
				}
			}
		}

		// Token: 0x06000BE9 RID: 3049 RVA: 0x0005558C File Offset: 0x0005398C
		public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex)
		{
			TileResultCode result = base.PaintTo(paint, permanent, meshIndex);
			if (meshIndex == 0)
			{
				this.UpdateGaugeMaterial(paint);
			}
			return result;
		}

		// Token: 0x06000BEA RID: 3050 RVA: 0x000555B4 File Offset: 0x000539B4
		private void UpdateGaugeMaterial(string paint)
		{
			Color[] array;
			if (this.gaugeMaterial != null && Blocksworld.colorDefinitions.TryGetValue(paint, out array))
			{
				this.gaugeMaterial.color = array[0];
			}
		}

		// Token: 0x06000BEB RID: 3051 RVA: 0x000555FC File Offset: 0x000539FC
		public override void OnHudMesh()
		{
			base.OnHudMesh();
			if (base.uiVisible && this.isDefined && Blocksworld.CurrentState == State.Play)
			{
				if (!string.IsNullOrEmpty(this.text))
				{
					string text = this.text;
					HudMeshOnGUI.Label(ref this.textLabel, this.rect, text, this.style, 0f);
					HudMeshOnGUI.Label(ref this.textOutlineLabel, this.rect, text, this.outlineStyle, 0f);
				}
				if (this.showFractionUI)
				{
					string text2 = this.currentValue + " / " + this.maxValue;
					HudMeshOnGUI.Label(ref this.withinLabel, this.withinRect, text2, this.style, 0f);
					HudMeshOnGUI.Label(ref this.withinOutlineLabel, this.withinRect, text2, this.outlineStyle, 0f);
				}
			}
		}

		// Token: 0x06000BEC RID: 3052 RVA: 0x000556E7 File Offset: 0x00053AE7
		protected override void SetUIVisible(bool v)
		{
			base.SetUIVisible(v);
			if (this.dirty && this.gaugeTile != null)
			{
				this.gauge.SetActive(v);
				this.gaugeTile.Show(v);
			}
		}

		// Token: 0x04000975 RID: 2421
		private Rect rect;

		// Token: 0x04000976 RID: 2422
		private Rect withinRect;

		// Token: 0x04000977 RID: 2423
		protected HudMeshLabel withinLabel;

		// Token: 0x04000978 RID: 2424
		protected HudMeshLabel withinOutlineLabel;

		// Token: 0x04000979 RID: 2425
		private TileCustom gaugeTile;

		// Token: 0x0400097A RID: 2426
		private static GameObject gaugePrefab;

		// Token: 0x0400097B RID: 2427
		private GameObject gauge;

		// Token: 0x0400097C RID: 2428
		private Material gaugeMaterial;

		// Token: 0x0400097D RID: 2429
		private const int GAUGE_WIDTH = 200;

		// Token: 0x0400097E RID: 2430
		private const int GAUGE_HEIGHT = 32;

		// Token: 0x0400097F RID: 2431
		protected bool showFractionUI = true;

		// Token: 0x04000980 RID: 2432
		public static Dictionary<int, BlockGaugeUI> allGaugeBlocks = new Dictionary<int, BlockGaugeUI>();
	}
}
