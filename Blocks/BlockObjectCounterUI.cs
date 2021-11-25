using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000B8 RID: 184
	public class BlockObjectCounterUI : BlockAbstractLimitedCounterUI
	{
		// Token: 0x06000E44 RID: 3652 RVA: 0x00060BCB File Offset: 0x0005EFCB
		public BlockObjectCounterUI(List<List<Tile>> tiles, int index) : base(tiles, index)
		{
		}

		// Token: 0x06000E45 RID: 3653 RVA: 0x00060BF0 File Offset: 0x0005EFF0
		public new static void Register()
		{
			PredicateRegistry.Add<BlockObjectCounterUI>("Leaderboard.SetType", null, (Block b) => new PredicateActionDelegate(((BlockObjectCounterUI)b).SetLeaderboardType), new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockObjectCounterUI>("Leaderboard.PauseTime", (Block b) => new PredicateSensorDelegate(((BlockObjectCounterUI)b).IsPauseLeaderboardTime), (Block b) => new PredicateActionDelegate(((BlockObjectCounterUI)b).PauseLeaderboardTime), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockObjectCounterUI>("Leaderboard.AddTime", null, (Block b) => new PredicateActionDelegate(((BlockObjectCounterUI)b).AddLeaderboardTime), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockObjectCounterUI>("Leaderboard.SubtractTime", null, (Block b) => new PredicateActionDelegate(((BlockObjectCounterUI)b).SubtractLeaderboardTime), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockObjectCounterUI>("ObjectCounterUI.SetMaxValue", null, (Block b) => new PredicateActionDelegate(((BlockObjectCounterUI)b).SetMaxValue), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockObjectCounterUI>("ObjectCounterUI.SetText", null, (Block b) => new PredicateActionDelegate(((BlockAbstractCounterUI)b).SetText), new Type[]
			{
				typeof(string)
			}, new string[]
			{
				"Text"
			}, null);
			PredicateRegistry.Add<BlockObjectCounterUI>("ObjectCounterUI.ShowUI", null, (Block b) => new PredicateActionDelegate(((BlockAbstractUI)b).ShowUI), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockObjectCounterUI>("ObjectCounterUI.ShowPhysical", null, (Block b) => new PredicateActionDelegate(((BlockAbstractUI)b).ShowPhysical), new Type[]
			{
				typeof(int)
			}, null, null);
			List<List<Tile>> value = new List<List<Tile>>
			{
				new List<Tile>
				{
					Block.FirstFrameTile(),
					Block.ThenTile(),
					new Tile(new GAF("Leaderboard.SetType", new object[]
					{
						"ShortestTime"
					}))
				},
				new List<Tile>
				{
					Block.FirstFrameTile(),
					Block.ThenTile(),
					new Tile(new GAF("ObjectCounterUI.Equals", new object[]
					{
						0,
						0
					}))
				},
				new List<Tile>
				{
					new Tile(new GAF("ObjectCounterUI.EqualsMax", new object[]
					{
						0
					})),
					Block.ThenTile(),
					new Tile(new GAF("Meta.GameWin", new object[]
					{
						"You win!",
						0f
					}))
				},
				Block.EmptyTileRow()
			};
			Block.defaultExtraTiles["UI Object Counter I"] = value;
		}

		// Token: 0x06000E46 RID: 3654 RVA: 0x00060F3B File Offset: 0x0005F33B
		public override void Play()
		{
			base.Play();
			this.maxValue = 5;
			this.maxFromTreasure = true;
			TreasureHandler.RegisterObjectCounter(this);
		}

		// Token: 0x06000E47 RID: 3655 RVA: 0x00060F58 File Offset: 0x0005F358
		public int GetTileCountLimit(GAF gaf)
		{
			int result = 7;
			CommonIconOverride iconOverride = base.GetIconOverride(gaf);
			if (iconOverride != null && iconOverride.tileCountLimit > 0)
			{
				result = iconOverride.tileCountLimit;
			}
			return result;
		}

		// Token: 0x06000E48 RID: 3656 RVA: 0x00060F8C File Offset: 0x0005F38C
		private void UpdateTileArray()
		{
			if (!base.uiVisible)
			{
				if (this.counterTiles != null)
				{
					for (int i = 0; i < this.counterTiles.Length; i++)
					{
						this.counterTiles[i].Show(false);
					}
				}
				return;
			}
			if (base.uiVisible && this.counterTiles != null)
			{
				if (Blocksworld.UI.SpeechBubble.ActiveCount() > 0)
				{
					this.didHideCounterBehindSpeechBubble = false;
					List<Rect> list = Blocksworld.UI.SpeechBubble.ActiveScreenRects();
					for (int j = 0; j < this.counterTiles.Length; j++)
					{
						Rect rectForTile = this.GetRectForTile(this.counterTiles[j]);
						for (int k = 0; k < list.Count; k++)
						{
							Rect r = list[k];
							bool flag = rectForTile.Intersects(r);
							this.counterTiles[j].Show(!flag);
							this.didHideCounterBehindSpeechBubble = (this.didHideCounterBehindSpeechBubble || flag);
						}
					}
				}
				else if (this.didHideCounterBehindSpeechBubble)
				{
					this.didHideCounterBehindSpeechBubble = false;
					this.textureDirty = true;
				}
			}
			string texture = base.GetTexture(0);
			bool flag2 = this.iconGaf == null && texture == "Plain";
			if (!flag2 && this.iconGaf != null)
			{
				flag2 = (this.iconGaf.Predicate == Block.predicateTextureTo && (string)this.iconGaf.Args[0] == "Plain");
			}
			if (this.counterTiles == null || (this.maxValue > this.currentTileCountLimit && this.counterTiles.Length != 1) || (this.maxValue <= this.currentTileCountLimit && this.counterTiles.Length != this.maxValue) || this.textureDirty)
			{
				int num = this.maxValue;
				GAF gaf = (this.iconGaf == null) ? new GAF("Block.TextureTo", new object[]
				{
					texture,
					Vector3.zero
				}) : this.iconGaf.Clone();
				this.currentTileCountLimit = this.GetTileCountLimit(gaf);
				this.scaledCurrentTileSize = (float)base.GetTileSize(gaf) * NormalizedScreen.pixelScale;
				if (this.maxValue > this.currentTileCountLimit)
				{
					num = 1;
				}
				if (this.counterTiles != null)
				{
					for (int l = 0; l < this.counterTiles.Length; l++)
					{
						TileCustom tileCustom = this.counterTiles[l];
						tileCustom.Destroy();
					}
				}
				this.counterTiles = new TileCustom[num];
				for (int m = 0; m < this.counterTiles.Length; m++)
				{
					if (flag2)
					{
						TileCustom tileCustom2 = new TileCustom(Blocksworld.tilePool.GetTileObjectForIcon(this.starDisabledIconName, false));
						this.counterTiles[m] = tileCustom2;
					}
					else
					{
						GAF newGaf = gaf.Clone();
						TileCustom orCreateTile = base.GetOrCreateTile(newGaf, this.customBackgroundColors);
						orCreateTile.Enable(false);
						this.counterTiles[m] = orCreateTile;
					}
				}
			}
			if (this.dirty || this.textureDirty)
			{
				float num2 = this.scaledCurrentTileSize * 0.15f;
				float num3 = (float)this.counterTiles.Length * this.scaledCurrentTileSize + (float)(this.counterTiles.Length - 1) * num2;
				float num4 = (float)NormalizedScreen.width * 0.5f - num3 * 0.5f;
				float num5 = 140f;
				float y = (float)NormalizedScreen.height - num5 * NormalizedScreen.pixelScale;
				for (int n = 0; n < this.counterTiles.Length; n++)
				{
					TileCustom tileCustom3 = this.counterTiles[n];
					tileCustom3.Show(base.uiVisible);
					if (base.uiVisible)
					{
						bool flag3 = n < this.currentValue + this.extraValue || this.maxValue > this.currentTileCountLimit;
						if (flag2)
						{
							string icon = (!flag3) ? this.starDisabledIconName : this.starEnabledIconName;
							this.counterTiles[n].SetIcon(icon);
							this.counterTiles[n].Enable(true);
						}
						else
						{
							tileCustom3.Enable(flag3);
						}
						tileCustom3.MoveTo(num4, y, 2f);
					}
					num4 += this.scaledCurrentTileSize + num2;
				}
			}
		}

		// Token: 0x06000E49 RID: 3657 RVA: 0x000613FC File Offset: 0x0005F7FC
		private Rect GetRectForTile(TileCustom tile)
		{
			Rect result = new Rect(tile.position.x, tile.position.y, this.scaledCurrentTileSize, this.scaledCurrentTileSize);
			return result;
		}

		// Token: 0x06000E4A RID: 3658 RVA: 0x00061434 File Offset: 0x0005F834
		private void DebugDrawRect(Rect r, Color c)
		{
			float z = 40f;
			Vector3 vector = new Vector3(r.x, r.y, z);
			Vector3 vector2 = new Vector3(r.x + r.width, r.y + r.height, z);
			Vector3 vector3 = new Vector3(r.x, r.y + r.height, z);
			Vector3 vector4 = new Vector3(r.x + r.width, r.y, z);
			Debug.DrawLine(vector, vector3, c);
			Debug.DrawLine(vector3, vector2, c);
			Debug.DrawLine(vector2, vector4, c);
			Debug.DrawLine(vector4, vector, c);
		}

		// Token: 0x06000E4B RID: 3659 RVA: 0x000614E1 File Offset: 0x0005F8E1
		public override void Update()
		{
			base.Update();
			if (this.isDefined && Blocksworld.CurrentState == State.Play)
			{
				this.UpdateTileArray();
				this.dirty = false;
				this.textureDirty = false;
			}
		}

		// Token: 0x06000E4C RID: 3660 RVA: 0x00061514 File Offset: 0x0005F914
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			if (this.counterTiles != null)
			{
				for (int i = 0; i < this.counterTiles.Length; i++)
				{
					TileCustom tileCustom = this.counterTiles[i];
					tileCustom.Destroy();
				}
			}
			this.counterTiles = null;
		}

		// Token: 0x06000E4D RID: 3661 RVA: 0x00061564 File Offset: 0x0005F964
		public override void OnHudMesh()
		{
			base.OnHudMesh();
			if (base.uiVisible && Blocksworld.CurrentState == State.Play)
			{
				if (this.isDefined && this.counterTiles != null && this.counterTiles.Length == 1 && this.maxValue != 1 && this.counterTiles[0].IsShown())
				{
					string text = (this.currentValue + this.extraValue).ToString() + " / " + this.maxValue;
					Rect rectForTile = this.GetRectForTile(this.counterTiles[0]);
					rectForTile.y = (float)Screen.height - NormalizedScreen.scale * (rectForTile.y + rectForTile.height * 0.55f);
					rectForTile.x = NormalizedScreen.scale * (rectForTile.x + rectForTile.width * 1.5f);
					Rect rect = new Rect(rectForTile.x + 2f, rectForTile.y + 2f, rectForTile.width, rectForTile.height);
					HudMeshOnGUI.Label(ref this.counterOutlineLabel, rect, text, this.outlineStyle, 0f);
					HudMeshOnGUI.Label(ref this.counterTextLabel, rectForTile, text, this.textColor, this.style);
				}
				if (!string.IsNullOrEmpty(this.text))
				{
					float num = (float)Screen.width * 0.85f;
					float x = ((float)Screen.width - num) * 0.5f;
					float num2 = 44f;
					float y = num2 * NormalizedScreen.scale;
					Rect rect2 = new Rect(x, y, num, 40f * NormalizedScreen.scale);
					Rect rect3 = new Rect(rect2.x + 2f, rect2.y + 2f, rect2.width, rect2.height);
					HudMeshOnGUI.Label(ref this.textOutlineLabel, rect3, this.text, this.outlineStyle, 0f);
					HudMeshOnGUI.Label(ref this.textLabel, rect2, this.text, this.textColor, this.style);
				}
			}
		}

		// Token: 0x06000E4E RID: 3662 RVA: 0x00061780 File Offset: 0x0005FB80
		public void UpdateTreasureState(int available, int pickedUp)
		{
			if (this.maxFromTreasure)
			{
				this.dirty |= (this.maxValue != available);
				this.maxValue = available;
			}
			int extraValue = this.extraValue;
			this.extraValue = pickedUp;
			int currentValue = this.currentValue;
			base.AdjustCurrentValue();
			this.dirty |= (this.extraValue != extraValue);
			this.dirty |= (currentValue != this.currentValue);
			this.isDefined = true;
		}

		// Token: 0x06000E4F RID: 3663 RVA: 0x0006180C File Offset: 0x0005FC0C
		protected override void SetMaxValue(int newMax)
		{
			base.SetMaxValue(newMax);
			this.maxFromTreasure = false;
		}

		// Token: 0x06000E50 RID: 3664 RVA: 0x0006181C File Offset: 0x0005FC1C
		public TileResultCode SetLeaderboardType(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x06000E51 RID: 3665 RVA: 0x00061820 File Offset: 0x0005FC20
		public TileResultCode IsPauseLeaderboardTime(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool flag = Util.GetIntArg(args, 0, 0) != 0;
			return base.boolToTileResult(flag == Blocksworld.leaderboardData.IsTimePaused());
		}

		// Token: 0x06000E52 RID: 3666 RVA: 0x00061850 File Offset: 0x0005FC50
		public TileResultCode PauseLeaderboardTime(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool pause = Util.GetIntArg(args, 0, 0) != 0;
			Blocksworld.leaderboardData.PauseTime(pause);
			return TileResultCode.True;
		}

		// Token: 0x06000E53 RID: 3667 RVA: 0x00061878 File Offset: 0x0005FC78
		public TileResultCode AddLeaderboardTime(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 0f);
			Blocksworld.leaderboardData.AddTime(floatArg);
			return TileResultCode.True;
		}

		// Token: 0x06000E54 RID: 3668 RVA: 0x000618A0 File Offset: 0x0005FCA0
		public TileResultCode SubtractLeaderboardTime(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 0f);
			Blocksworld.leaderboardData.AddTime(-floatArg);
			return TileResultCode.True;
		}

		// Token: 0x04000B18 RID: 2840
		private const int DEFAULT_TILE_COUNT_LIMIT = 7;

		// Token: 0x04000B19 RID: 2841
		protected int currentTileCountLimit = 7;

		// Token: 0x04000B1A RID: 2842
		protected bool maxFromTreasure = true;

		// Token: 0x04000B1B RID: 2843
		protected float scaledCurrentTileSize = 80f;

		// Token: 0x04000B1C RID: 2844
		private HudMeshLabel counterTextLabel;

		// Token: 0x04000B1D RID: 2845
		private HudMeshLabel counterOutlineLabel;

		// Token: 0x04000B1E RID: 2846
		private bool didHideCounterBehindSpeechBubble;

		// Token: 0x04000B1F RID: 2847
		private TileCustom[] counterTiles;
	}
}
