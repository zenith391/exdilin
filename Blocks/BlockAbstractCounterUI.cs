using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000052 RID: 82
	public abstract class BlockAbstractCounterUI : BlockAbstractUI
	{
		// Token: 0x060006C6 RID: 1734 RVA: 0x0002E5A0 File Offset: 0x0002C9A0
		public BlockAbstractCounterUI(List<List<Tile>> tiles, int index) : base(tiles, index)
		{
			this.defaultSingleGaf = new GAF("Block.TextureTo", new object[]
			{
				"Texture Clock Face"
			});
		}

		// Token: 0x060006C7 RID: 1735 RVA: 0x0002E624 File Offset: 0x0002CA24
		protected bool UseDefaultTile(string texture)
		{
			bool flag = this.iconGaf == null && texture == "Plain";
			if (!flag && this.iconGaf != null)
			{
				flag = (this.iconGaf.Predicate == Block.predicateTextureTo && (string)this.iconGaf.Args[0] == "Plain");
			}
			return flag;
		}

		// Token: 0x060006C8 RID: 1736 RVA: 0x0002E692 File Offset: 0x0002CA92
		protected void DestroyTile(ref TileCustom counterTile)
		{
			if (counterTile != null)
			{
				counterTile.Destroy();
				counterTile = null;
			}
		}

		// Token: 0x060006C9 RID: 1737 RVA: 0x0002E6A8 File Offset: 0x0002CAA8
		protected Rect GetLayoutRect()
		{
			float num = 260f;
			float num2 = 65f;
			float num3 = 0.5f;
			float num4 = 0f;
			int count = BlockGaugeUI.allGaugeBlocks.Count;
			int count2 = BlockCounterUI.allCounterBlocks.Count;
			if (this is BlockGaugeUI)
			{
				if (count == 1 && count2 == 0)
				{
					num3 = 0.5f;
				}
				else if (count == 1 && count2 >= 1)
				{
					num3 = 0f;
				}
				else if (count == 2)
				{
					num3 = (float)this.index;
				}
			}
			else if (this is BlockCounterUI)
			{
				if (count == 0)
				{
					if (count2 == 1)
					{
						num3 = 0.5f;
					}
					else
					{
						num3 = (float)this.index;
					}
				}
				else if (count == 1)
				{
					if (count2 == 1 || this.index == 0)
					{
						num3 = 1f;
					}
					else
					{
						num3 = 0.5f;
						num4 = 1f;
					}
				}
				else if (count == 2)
				{
					num4 = 1f;
					num3 = (float)this.index;
				}
			}
			else
			{
				BWLog.Info("No support yet for laying out UI blocks of type '" + base.BlockType() + "'");
			}
			float num5 = (num3 != 0.5f) ? 20f : 0f;
			float num6 = 0.5f * ((float)NormalizedScreen.width - num * 2f - num5);
			float x = num6 + num3 * (num + num5) + 40f;
			float y = (float)NormalizedScreen.height - NormalizedScreen.scale * 100f - num2 * num4;
			Rect result = new Rect(x, y, num * NormalizedScreen.scale, num2 * NormalizedScreen.scale);
			return result;
		}

		// Token: 0x060006CA RID: 1738 RVA: 0x0002E854 File Offset: 0x0002CC54
		protected void UpdateTile(ref TileCustom counterTile)
		{
			if (this.isDefined && Blocksworld.CurrentState == State.Play)
			{
				if (counterTile == null || this.textureDirty)
				{
					GAF gaf = this.iconGaf;
					string texture = base.GetTexture(0);
					if (this.UseDefaultTile(texture))
					{
						gaf = this.defaultSingleGaf;
					}
					GAF gaf2 = (gaf == null) ? ((!(texture == this.noTileTexture)) ? new GAF("Block.TextureTo", new object[]
					{
						texture,
						Vector3.zero
					}) : null) : gaf.Clone();
					if (gaf2 != null)
					{
						this.currentTileSize = this.GetTileSize(gaf2);
						counterTile = this.GetOrCreateTile(gaf2, this.customBackgroundColors);
						counterTile.Show(true);
					}
				}
				this.dirty = false;
				this.textureDirty = false;
			}
		}

		// Token: 0x060006CB RID: 1739 RVA: 0x0002E92D File Offset: 0x0002CD2D
		protected TileCustom GetOrCreateTile(GAF newGaf, Color[] customBackgroundColors)
		{
			return new TileCustom(newGaf, this.ShowBackground(newGaf), customBackgroundColors);
		}

		// Token: 0x060006CC RID: 1740 RVA: 0x0002E940 File Offset: 0x0002CD40
		public override void Play()
		{
			base.Play();
			this.currentValue = 0;
			this.extraValue = 0;
			this.previousValue = 0;
			this.isDefined = false;
			this.textureDirty = true;
			this.iconGaf = null;
			this.style = HudMeshOnGUI.dataSource.GetStyle("Counter");
			this.outlineStyle = HudMeshOnGUI.dataSource.GetStyle("Outline");
			if (BlockAbstractCounterUI.tileInfoPrefab == null)
			{
				BlockAbstractCounterUI.tileInfoPrefab = (Resources.Load("UI Tile Info") as GameObject);
			}
			this.counterMeta = BlockAbstractCounterUI.tileInfoPrefab.GetComponent<ObjectCounterMetaData>();
			this.blockIconOverrides.Clear();
			this.textureIconOverrides.Clear();
			foreach (BlockIconOverride blockIconOverride in this.counterMeta.blockIconOverrides)
			{
				this.blockIconOverrides[blockIconOverride.blockName] = blockIconOverride;
			}
			foreach (TextureIconOverride textureIconOverride in this.counterMeta.textureIconOverrides)
			{
				this.textureIconOverrides[textureIconOverride.textureName] = textureIconOverride;
			}
			this.textColorMeshIndex = this.counterMeta.textColorMeshIndex;
			this.backgroundColorMeshIndex = this.counterMeta.backgroundColorMeshIndex;
		}

		// Token: 0x060006CD RID: 1741 RVA: 0x0002EA87 File Offset: 0x0002CE87
		public override void Stop(bool resetBlock)
		{
			base.Stop(resetBlock);
			this.iconGaf = null;
		}

		// Token: 0x060006CE RID: 1742 RVA: 0x0002EA98 File Offset: 0x0002CE98
		protected bool ShowBackground(GAF gaf)
		{
			if (gaf.Predicate == Block.predicateCreate)
			{
				BlockIconOverride blockIconOverride;
				return this.blockIconOverrides.TryGetValue((string)gaf.Args[0], out blockIconOverride) && blockIconOverride.addBackground;
			}
			if (gaf.Predicate == Block.predicateTextureTo)
			{
				TextureIconOverride textureIconOverride;
				return !this.textureIconOverrides.TryGetValue((string)gaf.Args[0], out textureIconOverride) || !textureIconOverride.removeBackground;
			}
			return gaf.Predicate != Block.predicateUI;
		}

		// Token: 0x060006CF RID: 1743 RVA: 0x0002EB28 File Offset: 0x0002CF28
		public GAF GetCustomIconGAF()
		{
			return this.iconGaf;
		}

		// Token: 0x060006D0 RID: 1744 RVA: 0x0002EB30 File Offset: 0x0002CF30
		public void SetCustomIconGAF(GAF gaf)
		{
			if (this.iconGaf == null || !this.iconGaf.Equals(gaf))
			{
				this.textureDirty = true;
			}
			this.iconGaf = gaf;
		}

		// Token: 0x060006D1 RID: 1745 RVA: 0x0002EB5C File Offset: 0x0002CF5C
		protected CommonIconOverride GetIconOverride(GAF gaf)
		{
			BlockIconOverride blockIconOverride = this.GetBlockIconOverride(gaf);
			if (blockIconOverride != null)
			{
				return blockIconOverride;
			}
			TextureIconOverride textureIconOverride = this.GetTextureIconOverride(gaf);
			if (textureIconOverride != null)
			{
				return textureIconOverride;
			}
			return null;
		}

		// Token: 0x060006D2 RID: 1746 RVA: 0x0002EB8C File Offset: 0x0002CF8C
		public int GetTileSize(GAF gaf)
		{
			int result = 80;
			CommonIconOverride iconOverride = this.GetIconOverride(gaf);
			if (iconOverride != null && iconOverride.tileSize > 0)
			{
				result = iconOverride.tileSize;
			}
			return result;
		}

		// Token: 0x060006D3 RID: 1747 RVA: 0x0002EBC0 File Offset: 0x0002CFC0
		protected BlockIconOverride GetBlockIconOverride(GAF gaf)
		{
			BlockIconOverride result;
			if (gaf.Predicate == Block.predicateCreate && this.blockIconOverrides.TryGetValue((string)gaf.Args[0], out result))
			{
				return result;
			}
			return null;
		}

		// Token: 0x060006D4 RID: 1748 RVA: 0x0002EC00 File Offset: 0x0002D000
		protected TextureIconOverride GetTextureIconOverride(GAF gaf)
		{
			TextureIconOverride result;
			if (gaf.Predicate == Block.predicateTextureTo && this.textureIconOverrides.TryGetValue((string)gaf.Args[0], out result))
			{
				return result;
			}
			return null;
		}

		// Token: 0x060006D5 RID: 1749 RVA: 0x0002EC40 File Offset: 0x0002D040
		public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
		{
			if (meshIndex == 0 && Blocksworld.CurrentState == State.Play)
			{
				string texture2 = base.GetTexture(0);
				TileResultCode result = base.TextureTo(texture, normal, permanent, meshIndex, force);
				this.textureDirty = (texture2 != base.GetTexture(0));
				this.dirty = (this.dirty || this.textureDirty);
				return result;
			}
			return base.TextureTo(texture, normal, permanent, meshIndex, force);
		}

		// Token: 0x060006D6 RID: 1750 RVA: 0x0002ECB4 File Offset: 0x0002D0B4
		public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
		{
			if (meshIndex == this.backgroundColorMeshIndex && paint != this.backgroundColorPaint)
			{
				this.textureDirty = true;
				this.backgroundColorPaint = paint;
				Color[] array;
				if (paint == base.GetDefaultPaint(this.backgroundColorMeshIndex))
				{
					this.customBackgroundColors = null;
				}
				else if (Blocksworld.colorDefinitions.TryGetValue(paint, out array))
				{
					this.customBackgroundColors = array;
				}
			}
			if (meshIndex == this.textColorMeshIndex && paint != this.textColorPaint)
			{
				this.textColorPaint = paint;
				Color[] array2;
				if (paint == base.GetDefaultPaint(this.backgroundColorMeshIndex))
				{
					this.textColor = Color.white;
				}
				else if (Blocksworld.colorDefinitions.TryGetValue(paint, out array2))
				{
					this.textColor = array2[0];
				}
			}
			return base.PaintTo(paint, permanent, meshIndex);
		}

		// Token: 0x060006D7 RID: 1751 RVA: 0x0002EDA0 File Offset: 0x0002D1A0
		public bool ValueEquals(int value)
		{
			return this.isDefined && value == this.currentValue + this.extraValue;
		}

		// Token: 0x060006D8 RID: 1752 RVA: 0x0002EDC0 File Offset: 0x0002D1C0
		public TileResultCode ValueEquals(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			return (!this.ValueEquals(intArg)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x060006D9 RID: 1753 RVA: 0x0002EDEC File Offset: 0x0002D1EC
		public TileResultCode ValueCondition(int value, int condition)
		{
			if (this.isDefined)
			{
				int num = this.currentValue + this.extraValue;
				bool flag = false;
				if (condition != 0)
				{
					if (condition != 1)
					{
						if (condition == 2)
						{
							flag = (num != value);
						}
					}
					else
					{
						flag = (num > value);
					}
				}
				else
				{
					flag = (num < value);
				}
				return (!flag) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060006DA RID: 1754 RVA: 0x0002EE60 File Offset: 0x0002D260
		public TileResultCode ValueCondition(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.isDefined)
			{
				int intArg = Util.GetIntArg(args, 0, 0);
				int intArg2 = Util.GetIntArg(args, 1, 0);
				return this.ValueCondition(intArg, intArg2);
			}
			return TileResultCode.False;
		}

		// Token: 0x060006DB RID: 1755 RVA: 0x0002EE94 File Offset: 0x0002D294
		public virtual void SetValue(int value)
		{
			int num = this.currentValue + this.extraValue;
			this.currentValue = value;
			this.extraValue = 0;
			this.isDefined = true;
			this.dirty = (this.dirty || this.currentValue != num);
		}

		// Token: 0x060006DC RID: 1756 RVA: 0x0002EEE4 File Offset: 0x0002D2E4
		public virtual TileResultCode SetValue(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int value = (args.Length <= 0) ? 0 : ((int)args[0]);
			this.SetValue(value);
			return TileResultCode.True;
		}

		// Token: 0x060006DD RID: 1757 RVA: 0x0002EF11 File Offset: 0x0002D311
		public virtual void IncrementValue(int inc)
		{
			if (this.isDefined)
			{
				this.currentValue += inc;
				this.dirty = true;
			}
		}

		// Token: 0x060006DE RID: 1758 RVA: 0x0002EF34 File Offset: 0x0002D334
		public virtual TileResultCode IncrementValue(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int inc = (args.Length <= 0) ? 1 : ((int)args[0]);
			this.IncrementValue(inc);
			return TileResultCode.True;
		}

		// Token: 0x060006DF RID: 1759 RVA: 0x0002EF64 File Offset: 0x0002D364
		public virtual void Randomize(int min, int max)
		{
			int num = this.currentValue + this.extraValue;
			this.extraValue = 0;
			this.currentValue = UnityEngine.Random.Range(min, max + 1);
			this.dirty = (this.dirty || this.currentValue != num);
			this.isDefined = true;
		}

		// Token: 0x060006E0 RID: 1760 RVA: 0x0002EFBC File Offset: 0x0002D3BC
		public virtual TileResultCode Randomize(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			int intArg2 = Util.GetIntArg(args, 1, 10);
			this.Randomize(intArg, intArg2);
			return TileResultCode.True;
		}

		// Token: 0x040004F1 RID: 1265
		protected bool isDefined;

		// Token: 0x040004F2 RID: 1266
		protected int previousValue;

		// Token: 0x040004F3 RID: 1267
		protected int currentValue;

		// Token: 0x040004F4 RID: 1268
		protected int extraValue;

		// Token: 0x040004F5 RID: 1269
		protected HudMeshLabel textLabel;

		// Token: 0x040004F6 RID: 1270
		protected HudMeshLabel textOutlineLabel;

		// Token: 0x040004F7 RID: 1271
		protected Color textColor = Color.white;

		// Token: 0x040004F8 RID: 1272
		protected GAF iconGaf;

		// Token: 0x040004F9 RID: 1273
		protected string starEnabledIconName = "Misc/Counter_Star_Enabled";

		// Token: 0x040004FA RID: 1274
		protected string starDisabledIconName = "Misc/Counter_Star_Disabled";

		// Token: 0x040004FB RID: 1275
		protected GAF defaultSingleGaf;

		// Token: 0x040004FC RID: 1276
		protected string noTileTexture = string.Empty;

		// Token: 0x040004FD RID: 1277
		protected int textColorMeshIndex;

		// Token: 0x040004FE RID: 1278
		protected string textColorPaint;

		// Token: 0x040004FF RID: 1279
		protected int backgroundColorMeshIndex;

		// Token: 0x04000500 RID: 1280
		protected Color[] customBackgroundColors;

		// Token: 0x04000501 RID: 1281
		protected string backgroundColorPaint;

		// Token: 0x04000502 RID: 1282
		protected bool textureDirty = true;

		// Token: 0x04000503 RID: 1283
		protected HudMeshStyle style;

		// Token: 0x04000504 RID: 1284
		protected HudMeshStyle outlineStyle;

		// Token: 0x04000505 RID: 1285
		protected int currentTileSize = 80;

		// Token: 0x04000506 RID: 1286
		protected Dictionary<string, BlockIconOverride> blockIconOverrides = new Dictionary<string, BlockIconOverride>();

		// Token: 0x04000507 RID: 1287
		protected Dictionary<string, TextureIconOverride> textureIconOverrides = new Dictionary<string, TextureIconOverride>();

		// Token: 0x04000508 RID: 1288
		protected ObjectCounterMetaData counterMeta;

		// Token: 0x04000509 RID: 1289
		private static GameObject tileInfoPrefab;
	}
}
