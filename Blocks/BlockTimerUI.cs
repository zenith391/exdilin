using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000E1 RID: 225
	public class BlockTimerUI : BlockAbstractLimitedCounterUI
	{
		// Token: 0x060010ED RID: 4333 RVA: 0x0007545C File Offset: 0x0007385C
		public BlockTimerUI(List<List<Tile>> tiles, int index) : base(tiles, index)
		{
			this.defaultSingleGaf = null;
			this.maxValue = int.MaxValue;
			this.noTileTexture = "Plain";
		}

		// Token: 0x060010EE RID: 4334 RVA: 0x000754AC File Offset: 0x000738AC
		public new static void Register()
		{
			PredicateRegistry.Add<BlockTimerUI>("TimerUI.SetMaxValue", null, (Block b) => new PredicateActionDelegate(((BlockTimerUI)b).SetTimerMaxValue), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockTimerUI>("TimerUI.Flash", null, (Block b) => new PredicateActionDelegate(((BlockAbstractUI)b).Flash), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockTimerUI>("TimerUI.SetText", null, (Block b) => new PredicateActionDelegate(((BlockAbstractCounterUI)b).SetText), new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockTimerUI>("TimerUI.ShowUI", null, (Block b) => new PredicateActionDelegate(((BlockAbstractCounterUI)b).ShowUI), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockTimerUI>("TimerUI.ShowPhysical", null, (Block b) => new PredicateActionDelegate(((BlockAbstractUI)b).ShowPhysical), new Type[]
			{
				typeof(int)
			}, null, null);
		}

		// Token: 0x060010EF RID: 4335 RVA: 0x000755EF File Offset: 0x000739EF
		public override void Play()
		{
			base.Play();
			this.maxValue = int.MaxValue;
			this.timerState = BlockTimerUI.TimerState.STOPPED;
			this.direction = 1;
			this.rect = this.GetRect();
			BlockTimerUI.allTimerBlocks[this.index] = this;
		}

		// Token: 0x060010F0 RID: 4336 RVA: 0x0007562D File Offset: 0x00073A2D
		public override void Stop(bool resetBlock)
		{
			base.Stop(resetBlock);
			BlockTimerUI.allTimerBlocks.Clear();
			base.DestroyTile(ref this.timerTile);
		}

		// Token: 0x060010F1 RID: 4337 RVA: 0x0007564C File Offset: 0x00073A4C
		public bool IsRunning()
		{
			return this.timerState == BlockTimerUI.TimerState.RUNNING;
		}

		// Token: 0x060010F2 RID: 4338 RVA: 0x00075657 File Offset: 0x00073A57
		public int GetDirection()
		{
			return this.direction;
		}

		// Token: 0x060010F3 RID: 4339 RVA: 0x00075660 File Offset: 0x00073A60
		public Rect GetRect()
		{
			float num = 150f;
			float num2 = 50f;
			float x = (float)Screen.width - 200f * NormalizedScreen.scale;
			float y = NormalizedScreen.scale * 20f;
			Rect result = new Rect(x, y, num * NormalizedScreen.scale, num2 * NormalizedScreen.scale);
			return result;
		}

		// Token: 0x060010F4 RID: 4340 RVA: 0x000756B4 File Offset: 0x00073AB4
		public override void Update()
		{
			base.Update();
			if (Blocksworld.CurrentState == State.Play)
			{
				base.UpdateTile(ref this.timerTile);
				if (this.timerTile != null)
				{
					Vector2 vector = new Vector2(this.rect.x - 80f, (float)(NormalizedScreen.height - 80));
					this.timerTile.MoveTo(vector.x, vector.y, 2f);
				}
			}
		}

		// Token: 0x060010F5 RID: 4341 RVA: 0x00075728 File Offset: 0x00073B28
		public override void OnHudMesh()
		{
			base.OnHudMesh();
			if (base.uiVisible && this.isDefined && Blocksworld.CurrentState == State.Play)
			{
				string text;
				if (this.uiPaused)
				{
					text = this.prevTimeText;
				}
				else
				{
					text = string.Empty;
					float secondsRaw = (float)this.currentValue * Blocksworld.fixedDeltaTime;
					TimeTileParameter.CalculateTimeComponents(secondsRaw, this.timeComponents);
					for (int i = 2; i >= 0; i--)
					{
						text += this.timeComponents[i].ToString("D2");
						if (i > 0)
						{
							text += ":";
						}
					}
					if (this.text.Length > 0)
					{
						text = this.text + ": " + text;
					}
				}
				HudMeshOnGUI.Label(ref this.textLabel, this.rect, text, this.textColor, this.style);
				HudMeshOnGUI.Label(ref this.textOutlineLabel, this.rect, text, this.outlineStyle, 0f);
				this.prevTimeText = text;
			}
		}

		// Token: 0x060010F6 RID: 4342 RVA: 0x0007583B File Offset: 0x00073C3B
		public bool CanBePaused()
		{
			return this.timerState != BlockTimerUI.TimerState.STOPPED;
		}

		// Token: 0x060010F7 RID: 4343 RVA: 0x0007584C File Offset: 0x00073C4C
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (this.timerState == BlockTimerUI.TimerState.RUNNING && this.isDefined)
			{
				this.currentValue = Mathf.Clamp(this.currentValue + this.direction, 0, this.maxValue);
			}
			else if (this.timerState == BlockTimerUI.TimerState.PAUSED_SINGLE_FRAME)
			{
				this.timerState = BlockTimerUI.TimerState.RUNNING;
			}
			this.previousValue = this.currentValue;
		}

		// Token: 0x060010F8 RID: 4344 RVA: 0x000758BC File Offset: 0x00073CBC
		public TileResultCode ValueCondition(float floatValue, int condition)
		{
			if (this.isDefined)
			{
				int num = Mathf.RoundToInt(floatValue / Blocksworld.fixedDeltaTime);
				int num2 = this.currentValue + this.extraValue;
				bool flag = false;
				if (condition != 0)
				{
					if (condition != 1)
					{
						if (condition == 2)
						{
							flag = (num2 != num);
						}
					}
					else
					{
						flag = (num2 > num);
					}
				}
				else
				{
					flag = (num2 < num);
				}
				return (!flag) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060010F9 RID: 4345 RVA: 0x0007593C File Offset: 0x00073D3C
		public TileResultCode TimeEquals(float time)
		{
			if (this.isDefined)
			{
				int num = Mathf.RoundToInt(time / Blocksworld.fixedDeltaTime);
				if (num == this.currentValue)
				{
					return TileResultCode.True;
				}
			}
			return TileResultCode.False;
		}

		// Token: 0x060010FA RID: 4346 RVA: 0x00075970 File Offset: 0x00073D70
		public void StartTimer(int dir)
		{
			if (this.isDefined)
			{
				this.direction = dir;
				this.timerState = BlockTimerUI.TimerState.RUNNING;
			}
		}

		// Token: 0x060010FB RID: 4347 RVA: 0x0007598B File Offset: 0x00073D8B
		public void SetTime(float time)
		{
			this.currentValue = Mathf.RoundToInt(time / Blocksworld.fixedDeltaTime);
			this.isDefined = true;
		}

		// Token: 0x060010FC RID: 4348 RVA: 0x000759A8 File Offset: 0x00073DA8
		public void IncrementTime(float time)
		{
			if (this.isDefined)
			{
				int num = Mathf.RoundToInt(time / Blocksworld.fixedDeltaTime);
				this.currentValue = Mathf.Max(0, this.currentValue + num);
			}
		}

		// Token: 0x060010FD RID: 4349 RVA: 0x000759E1 File Offset: 0x00073DE1
		public void PauseOneFrame()
		{
			if (this.timerState == BlockTimerUI.TimerState.RUNNING)
			{
				this.timerState = BlockTimerUI.TimerState.PAUSED_SINGLE_FRAME;
			}
		}

		// Token: 0x060010FE RID: 4350 RVA: 0x000759F8 File Offset: 0x00073DF8
		public TileResultCode SetTimerMaxValue(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int maxValue = Mathf.RoundToInt(Util.GetFloatArg(args, 0, 60f) / Blocksworld.fixedDeltaTime);
			this.SetMaxValue(maxValue);
			return TileResultCode.True;
		}

		// Token: 0x060010FF RID: 4351 RVA: 0x00075A25 File Offset: 0x00073E25
		protected override void SetUIVisible(bool v)
		{
			base.SetUIVisible(v);
			if (this.dirty && this.timerTile != null)
			{
				this.timerTile.Show(v);
			}
		}

		// Token: 0x04000D48 RID: 3400
		private Rect rect;

		// Token: 0x04000D49 RID: 3401
		protected int direction = 1;

		// Token: 0x04000D4A RID: 3402
		private string prevTimeText = string.Empty;

		// Token: 0x04000D4B RID: 3403
		private HudMeshLabel label;

		// Token: 0x04000D4C RID: 3404
		private BlockTimerUI.TimerState timerState;

		// Token: 0x04000D4D RID: 3405
		public static Dictionary<int, BlockTimerUI> allTimerBlocks = new Dictionary<int, BlockTimerUI>();

		// Token: 0x04000D4E RID: 3406
		private TileCustom timerTile;

		// Token: 0x04000D4F RID: 3407
		private int[] timeComponents = new int[3];

		// Token: 0x020000E2 RID: 226
		private enum TimerState
		{
			// Token: 0x04000D56 RID: 3414
			STOPPED,
			// Token: 0x04000D57 RID: 3415
			RUNNING,
			// Token: 0x04000D58 RID: 3416
			PAUSED_SINGLE_FRAME
		}
	}
}
