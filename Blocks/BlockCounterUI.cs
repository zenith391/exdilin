using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000083 RID: 131
	public class BlockCounterUI : BlockAbstractLimitedCounterUI
	{
		// Token: 0x06000B49 RID: 2889 RVA: 0x00051ABC File Offset: 0x0004FEBC
		public BlockCounterUI(List<List<Tile>> tiles, int index) : base(tiles, index)
		{
			this.defaultSingleGaf = new GAF("Block.Create", new object[]
			{
				base.BlockType()
			});
			this.maxValue = 1000000000;
			this.minValue = -1000000000;
			this.displayType = 1;
			this.flashDurations = new List<float[]>
			{
				new float[]
				{
					1f,
					-0.1f
				},
				new float[]
				{
					0.1f,
					-0.1f,
					0.1f,
					-0.1f,
					0.1f,
					-0.1f,
					0.1f,
					-0.1f,
					0.1f,
					-0.1f,
					0.5f
				}
			};
		}

		// Token: 0x06000B4A RID: 2890 RVA: 0x00051B84 File Offset: 0x0004FF84
		public new static void Register()
		{
			PredicateRegistry.Add<BlockCounterUI>("CounterUI.SetDisplayType", null, (Block b) => new PredicateActionDelegate(((BlockCounterUI)b).SetDisplayType), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockCounterUI>("CounterUI.Flash", null, (Block b) => new PredicateActionDelegate(((BlockAbstractUI)b).Flash), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockCounterUI>("CounterUI.SetMaxValue", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLimitedCounterUI)b).SetMaxValue), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockCounterUI>("CounterUI.SetMinValue", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLimitedCounterUI)b).SetMinValue), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockCounterUI>("CounterUI.SetText", null, (Block b) => new PredicateActionDelegate(((BlockAbstractUI)b).SetText), new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockCounterUI>("CounterUI.ShowUI", null, (Block b) => new PredicateActionDelegate(((BlockAbstractUI)b).ShowUI), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockCounterUI>("CounterUI.ShowPhysical", null, (Block b) => new PredicateActionDelegate(((BlockAbstractUI)b).ShowPhysical), new Type[]
			{
				typeof(int)
			}, null, null);
		}

		// Token: 0x06000B4B RID: 2891 RVA: 0x00051D44 File Offset: 0x00050144
		public override void Play()
		{
			base.Play();
			this.maxValue = 1000000000;
			this.minValue = -1000000000;
			BlockCounterUI.allCounterBlocks[this.index] = this;
			this.displayType = 1;
			this.animatedScores.Clear();
			this.animatedBlocks.Clear();
		}

		// Token: 0x06000B4C RID: 2892 RVA: 0x00051D9C File Offset: 0x0005019C
		public override void Play2()
		{
			base.Play2();
			this.rect = base.GetLayoutRect();
			this.scoreRect = new Rect(this.rect);
			this.scoreRect.center = this.rect.center + Vector2.up * 30f;
		}

		// Token: 0x06000B4D RID: 2893 RVA: 0x00051DF6 File Offset: 0x000501F6
		public override void Stop(bool resetBlock)
		{
			base.Stop(resetBlock);
			BlockCounterUI.allCounterBlocks.Clear();
			base.DestroyTile(ref this.counterTile);
			this.animatedScores.Clear();
			this.animatedBlocks.Clear();
		}

		// Token: 0x06000B4E RID: 2894 RVA: 0x00051E2C File Offset: 0x0005022C
		public override void Update()
		{
			base.Update();
			if (Blocksworld.CurrentState == State.Play)
			{
				base.UpdateTile(ref this.counterTile);
				if (this.counterTile != null)
				{
					Vector2 vector = new Vector2(this.rect.x - 40f, (float)NormalizedScreen.height - this.rect.y - 80f);
					this.counterTile.MoveTo(vector.x, vector.y, 2f);
				}
			}
		}

		// Token: 0x06000B4F RID: 2895 RVA: 0x00051EB0 File Offset: 0x000502B0
		public static string GetCounterString(string b, int value, bool addPositiveSign = false)
		{
			int num = Mathf.Abs(value);
			string text = string.Empty;
			do
			{
				int num2 = num % 1000;
				num = Mathf.FloorToInt((float)(num / 1000));
				text = ((num <= 0) ? num2.ToString() : num2.ToString("D3")) + ((text.Length != 0) ? b : string.Empty) + text;
			}
			while (num > 0);
			return ((value >= 0) ? ((!addPositiveSign) ? string.Empty : "+") : "-") + text;
		}

		// Token: 0x06000B50 RID: 2896 RVA: 0x00051F58 File Offset: 0x00050358
		public override void OnHudMesh()
		{
			base.OnHudMesh();
			if (base.uiVisible && Blocksworld.CurrentState == State.Play)
			{
				if (this.isDefined)
				{
					int num = 0;
					for (int i = this.animatedScores.Count - 1; i >= 0; i--)
					{
						BlockCounterUI.AnimatedScore animatedScore = this.animatedScores[i];
						bool flag = animatedScore.Update(this.displayType, this.style, this.outlineStyle);
						if (flag)
						{
							this.animatedScores.RemoveAt(i);
						}
						else
						{
							num += animatedScore.GetDelayedScore();
						}
					}
					int value = this.currentValue - num;
					string text;
					if (this.displayType == 0 || this.displayType < 0 || this.displayType >= BlockCounterUI.betweenStrings.Length)
					{
						text = value.ToString();
					}
					else
					{
						text = BlockCounterUI.GetCounterString(BlockCounterUI.betweenStrings[this.displayType], value, false);
					}
					Vector2 textDimension = this.GetTextDimension(text);
					Rect rect = new Rect(this.scoreRect);
					if (this.alignScoreLeft)
					{
						Vector2 center = this.scoreRect.center;
						float x = center.x - 0.5f * (rect.width - textDimension.x) + 40f;
						center.x = x;
						rect.center = center;
					}
					HudMeshOnGUI.Label(ref this.textLabel, rect, text, this.textColor, this.style);
					HudMeshOnGUI.Label(ref this.textOutlineLabel, rect, text, this.outlineStyle, 0f);
				}
				if (this.text.Length > 0)
				{
					HudMeshOnGUI.Label(ref this.textAboveLabel, this.rect, this.text, this.textColor, this.style);
					HudMeshOnGUI.Label(ref this.textAboveOutlineLabel, this.rect, this.text, this.outlineStyle, 0f);
				}
			}
		}

		// Token: 0x06000B51 RID: 2897 RVA: 0x00052140 File Offset: 0x00050540
		private Vector2 GetTextDimension(string txt)
		{
			if (this.textDims.Count > 100)
			{
				this.textDims.Clear();
			}
			Vector2 vector;
			if (!this.textDims.TryGetValue(txt, out vector))
			{
				vector = HudMeshUtils.CalcSize(this.style, txt);
				this.textDims[txt] = vector;
			}
			return vector;
		}

		// Token: 0x06000B52 RID: 2898 RVA: 0x00052198 File Offset: 0x00050598
		public TileResultCode SetDisplayType(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.displayType = Util.GetIntArg(args, 0, 0);
			return TileResultCode.True;
		}

		// Token: 0x06000B53 RID: 2899 RVA: 0x000521A9 File Offset: 0x000505A9
		protected override int AdjustCurrentLongValue(long currentLongValue)
		{
			if (currentLongValue < (long)this.minValue)
			{
				currentLongValue = (long)this.minValue;
			}
			else if (currentLongValue > (long)this.maxValue)
			{
				currentLongValue = (long)this.maxValue;
			}
			return (int)currentLongValue;
		}

		// Token: 0x06000B54 RID: 2900 RVA: 0x000521E0 File Offset: 0x000505E0
		public void BlockIncrementValue(Block b, int inc)
		{
			BlockCounterUI.AnimatedScore animatedScore = null;
			BlockCounterUI.AnimatedBlockScore animatedBlockScore;
			if (this.animatedBlocks.TryGetValue(b, out animatedBlockScore))
			{
				if (animatedBlockScore.animationType != -1)
				{
					Vector3 position = b.go.transform.position;
					float num = 10f * Mathf.Log10((float)Mathf.Max(1, Mathf.Abs(inc)));
					Vector3 vector = Util.WorldToScreenPoint(position, true);
					float num2 = Mathf.Sign(vector.z);
					Vector2 startPos = new Vector2(num2 * vector.x, num2 * vector.y);
					if (startPos.x < num)
					{
						startPos.x = num;
					}
					else if (startPos.x > (float)NormalizedScreen.width - num)
					{
						startPos.x = (float)NormalizedScreen.width - num;
					}
					float num3 = 10f;
					if (startPos.y < num3)
					{
						startPos.y = num3;
					}
					else if (startPos.y > (float)NormalizedScreen.height - num3)
					{
						startPos.y = (float)NormalizedScreen.height - num3;
					}
					startPos.y = (float)Screen.height - startPos.y;
					animatedScore = new BlockCounterUI.AnimatedScore(this, startPos, this.rect.center, inc, animatedBlockScore);
					this.animatedScores.Add(animatedScore);
				}
				inc *= ((inc <= 0) ? (animatedBlockScore.negativeScoreMultiplier * this.negativeScoreMultiplier) : (animatedBlockScore.positiveScoreMultiplier * this.positiveScoreMultiplier));
			}
			this.IncrementValue(inc);
			if (animatedScore != null && (this.currentValue == this.maxValue || this.currentValue == this.minValue))
			{
				animatedScore.canDelay = false;
			}
		}

		// Token: 0x06000B55 RID: 2901 RVA: 0x00052386 File Offset: 0x00050786
		protected override void SetUIVisible(bool v)
		{
			base.SetUIVisible(v);
			if (this.dirty && this.counterTile != null)
			{
				this.counterTile.Show(v);
			}
		}

		// Token: 0x06000B56 RID: 2902 RVA: 0x000523B4 File Offset: 0x000507B4
		public void SetScoreAnimatedBlock(Block b, int animationType)
		{
			BlockCounterUI.AnimatedBlockScore animatedBlockScore;
			if (!this.animatedBlocks.TryGetValue(b, out animatedBlockScore))
			{
				animatedBlockScore = new BlockCounterUI.AnimatedBlockScore();
				this.animatedBlocks[b] = animatedBlockScore;
			}
			animatedBlockScore.animationType = animationType;
		}

		// Token: 0x06000B57 RID: 2903 RVA: 0x000523F0 File Offset: 0x000507F0
		public void SetScoreMultiplierBlock(Block b, int multiplier, bool positiveOnly = true)
		{
			BlockCounterUI.AnimatedBlockScore animatedBlockScore;
			if (!this.animatedBlocks.TryGetValue(b, out animatedBlockScore))
			{
				animatedBlockScore = new BlockCounterUI.AnimatedBlockScore();
				this.animatedBlocks[b] = animatedBlockScore;
			}
			animatedBlockScore.negativeScoreMultiplier = ((!positiveOnly) ? multiplier : 1);
			animatedBlockScore.positiveScoreMultiplier = multiplier;
		}

		// Token: 0x06000B58 RID: 2904 RVA: 0x0005243D File Offset: 0x0005083D
		public void SetGlobalScoreMultiplier(int multiplier, bool positiveOnly = true)
		{
			this.negativeScoreMultiplier = ((!positiveOnly) ? multiplier : 1);
			this.positiveScoreMultiplier = multiplier;
		}

		// Token: 0x040008E7 RID: 2279
		private Rect rect;

		// Token: 0x040008E8 RID: 2280
		private Rect scoreRect;

		// Token: 0x040008E9 RID: 2281
		public static Dictionary<int, BlockCounterUI> allCounterBlocks = new Dictionary<int, BlockCounterUI>();

		// Token: 0x040008EA RID: 2282
		private TileCustom counterTile;

		// Token: 0x040008EB RID: 2283
		private int positiveScoreMultiplier = 1;

		// Token: 0x040008EC RID: 2284
		private int negativeScoreMultiplier = 1;

		// Token: 0x040008ED RID: 2285
		public const int MAX_PARAMETER_VALUE = 10000000;

		// Token: 0x040008EE RID: 2286
		public const int DEFAULT_COUNTER_MAX_VALUE = 1000000000;

		// Token: 0x040008EF RID: 2287
		private const int DEFAULT_DISPLAY_TYPE = 1;

		// Token: 0x040008F0 RID: 2288
		private int displayType = 1;

		// Token: 0x040008F1 RID: 2289
		private bool alignScoreLeft;

		// Token: 0x040008F2 RID: 2290
		protected HudMeshLabel textAboveLabel;

		// Token: 0x040008F3 RID: 2291
		protected HudMeshLabel textAboveOutlineLabel;

		// Token: 0x040008F4 RID: 2292
		private Dictionary<Block, BlockCounterUI.AnimatedBlockScore> animatedBlocks = new Dictionary<Block, BlockCounterUI.AnimatedBlockScore>();

		// Token: 0x040008F5 RID: 2293
		private List<BlockCounterUI.AnimatedScore> animatedScores = new List<BlockCounterUI.AnimatedScore>();

		// Token: 0x040008F6 RID: 2294
		public static string[] betweenStrings = new string[]
		{
			string.Empty,
			",",
			".",
			" "
		};

		// Token: 0x040008F7 RID: 2295
		private Dictionary<string, Vector2> textDims = new Dictionary<string, Vector2>();

		// Token: 0x02000084 RID: 132
		private class AnimatedBlockScore
		{
			// Token: 0x040008FF RID: 2303
			public int animationType = -1;

			// Token: 0x04000900 RID: 2304
			public int positiveScoreMultiplier = 1;

			// Token: 0x04000901 RID: 2305
			public int negativeScoreMultiplier = 1;
		}

		// Token: 0x02000085 RID: 133
		private class AnimatedScore
		{
			// Token: 0x06000B62 RID: 2914 RVA: 0x00052534 File Offset: 0x00050934
			public AnimatedScore(BlockCounterUI counter, Vector2 startPos, Vector2 targetPos, int score, BlockCounterUI.AnimatedBlockScore blockScore)
			{
				this.counter = counter;
				this.score = score;
				this.startTime = Time.time;
				this.blockScore = blockScore;
				this.duration = BlockCounterUI.AnimatedScore.animTypeDurations[blockScore.animationType % BlockCounterUI.AnimatedScore.animTypeDurations.Length];
				if (blockScore.animationType == 0)
				{
					this.animCurveX = new AnimationCurve(new Keyframe[]
					{
						new Keyframe(0f, startPos.x, 0f, 0f),
						new Keyframe(0.2f, startPos.x, 0f, 0f),
						new Keyframe(0.25f, startPos.x, 0f, 1f),
						new Keyframe(1f, targetPos.x, 1f, 1f)
					});
					this.animCurveY = new AnimationCurve(new Keyframe[]
					{
						new Keyframe(0f, startPos.y, 0f, 0f),
						new Keyframe(0.2f, startPos.y - 20f, 0f, 0f),
						new Keyframe(0.25f, startPos.y - 25f, 0f, 1f),
						new Keyframe(1f, targetPos.y, 1f, 1f)
					});
				}
				else
				{
					this.animCurveX = new AnimationCurve(new Keyframe[]
					{
						new Keyframe(0f, startPos.x, 0f, 0f),
						new Keyframe(0.8f, startPos.x, 0f, 0f),
						new Keyframe(1f, startPos.x, 0f, 1f)
					});
					this.animCurveY = new AnimationCurve(new Keyframe[]
					{
						new Keyframe(0f, startPos.y, 0f, 0f),
						new Keyframe(0.8f, startPos.y - 20f, 0f, 0f),
						new Keyframe(1f, startPos.y - 25f, 0f, 1f)
					});
				}
			}

			// Token: 0x06000B63 RID: 2915 RVA: 0x00052840 File Offset: 0x00050C40
			public int GetDelayedScore()
			{
				if (this.blockScore.animationType == 0 && this.canDelay)
				{
					return this.score * ((this.score <= 0) ? (this.blockScore.negativeScoreMultiplier * this.counter.negativeScoreMultiplier) : (this.blockScore.positiveScoreMultiplier * this.counter.positiveScoreMultiplier));
				}
				return 0;
			}

			// Token: 0x06000B64 RID: 2916 RVA: 0x000528B0 File Offset: 0x00050CB0
			public bool Update(int displayType, HudMeshStyle style, HudMeshStyle outlineStyle)
			{
				float num = (Time.time - this.startTime) / this.duration;
				this.position = new Vector2(this.animCurveX.Evaluate(num), this.animCurveY.Evaluate(num));
				this.scoreRect.center = this.position;
				string text = BlockCounterUI.GetCounterString(BlockCounterUI.betweenStrings[displayType], this.score, true);
				if (this.score > 0 && this.blockScore.positiveScoreMultiplier * this.counter.positiveScoreMultiplier > 1)
				{
					text = text + " X" + this.blockScore.positiveScoreMultiplier * this.counter.positiveScoreMultiplier;
				}
				else if (this.score < 0 && this.blockScore.negativeScoreMultiplier * this.counter.negativeScoreMultiplier > 1)
				{
					text = text + " X" + this.blockScore.negativeScoreMultiplier * this.counter.negativeScoreMultiplier;
				}
				HudMeshOnGUI.Label(ref this.label, this.scoreRect, text, style, 0f);
				HudMeshOnGUI.Label(ref this.outlineLabel, this.scoreRect, text, outlineStyle, 0f);
				return num >= 1f;
			}

			// Token: 0x04000902 RID: 2306
			private int score;

			// Token: 0x04000903 RID: 2307
			private BlockCounterUI.AnimatedBlockScore blockScore;

			// Token: 0x04000904 RID: 2308
			private HudMeshLabel label;

			// Token: 0x04000905 RID: 2309
			private HudMeshLabel outlineLabel;

			// Token: 0x04000906 RID: 2310
			public bool canDelay;

			// Token: 0x04000907 RID: 2311
			private float duration = 0.5f;

			// Token: 0x04000908 RID: 2312
			private float startTime;

			// Token: 0x04000909 RID: 2313
			private Vector2 position;

			// Token: 0x0400090A RID: 2314
			private Rect scoreRect = new Rect(0f, 0f, 200f, 50f);

			// Token: 0x0400090B RID: 2315
			private AnimationCurve animCurveX;

			// Token: 0x0400090C RID: 2316
			private AnimationCurve animCurveY;

			// Token: 0x0400090D RID: 2317
			private BlockCounterUI counter;

			// Token: 0x0400090E RID: 2318
			private static float[] animTypeDurations = new float[]
			{
				0.5f,
				0.3f
			};
		}
	}
}
