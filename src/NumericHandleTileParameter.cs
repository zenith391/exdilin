using System;
using Blocks;
using Gestures;
using UnityEngine;

// Token: 0x02000159 RID: 345
public abstract class NumericHandleTileParameter : EditableTileParameter
{
	// Token: 0x060014F6 RID: 5366 RVA: 0x000913EC File Offset: 0x0008F7EC
	public NumericHandleTileParameter(float sensitivity = 25f, int parameterIndex = 0, int subParameterCount = 1, bool onlyShowPositive = false, string prefixValueString = "", string postfixValueString = "") : base(parameterIndex, true, subParameterCount)
	{
		this.sensitivity = sensitivity / (float)NormalizedScreen.width;
		this.onlyShowPositive = onlyShowPositive;
		this.prefixValueString = prefixValueString;
		this.postfixValueString = postfixValueString;
	}

	// Token: 0x060014F7 RID: 5367 RVA: 0x0009144C File Offset: 0x0008F84C
	public override void HelpSetParameterValueInTutorial(Block block, Tile thisTile, Tile goalTile)
	{
		if (Blocksworld.bw.tileParameterEditor.selectedTile != thisTile || this.handle == null)
		{
			Tutorial.HelpToggleTile(block, thisTile);
			return;
		}
		float parameterScreenOffsetError = this.GetParameterScreenOffsetError(thisTile, goalTile);
		this.HelpDragHandle(thisTile, parameterScreenOffsetError);
		this.tutorialErrorOffset = parameterScreenOffsetError;
		this.tutorialTargetValue = goalTile.gaf.Args[base.parameterIndex];
	}

	// Token: 0x060014F8 RID: 5368 RVA: 0x000914B4 File Offset: 0x0008F8B4
	protected void HelpDragHandle(Tile thisTile, float offset)
	{
		Vector3 vector = this.handle.tileObject.GetPosition() + new Vector3(40f, 40f, 0f);
		Arrow arrow = Tutorial.arrow1;
		arrow.state = TrackingState.Screen2Screen;
		arrow.screen = vector;
		Vector3 screen = vector;
		screen.x += offset;
		arrow.screen2 = screen;
		arrow.Show(true, 0);
		Tutorial.hand1.DragArrow(arrow);
		Tutorial.state = TutorialState.SetParameter;
	}

	// Token: 0x060014F9 RID: 5369 RVA: 0x00091534 File Offset: 0x0008F934
	public void HoldHandle(Vector2 pos)
	{
		this.screenPos.Set(pos.x, this.screenPos.y, this.screenPos.z);
		if ((this.ValueAtMaxOrMore() && this.screenPos.x > this.lastScreenPos) || (this.ValueAtMinOrLess() && this.screenPos.x < this.lastScreenPos))
		{
			if (this.canOvershoot)
			{
				this.screenPos.Set((this.screenPos.x + this.lastScreenPos) / 2f, this.screenPos.y, this.screenPos.z);
			}
			else
			{
				this.screenPos.x = this.lastScreenPos;
			}
		}
		if ((this.ValueAtMaxOrMore() && this.screenPos.x - this.lastScreenPos > 100f * NormalizedScreen.scale) || (this.ValueAtMinOrLess() && this.lastScreenPos - this.screenPos.x > 100f * NormalizedScreen.scale))
		{
			this.ReleaseHandle();
			return;
		}
		this.held = true;
	}

	// Token: 0x060014FA RID: 5370 RVA: 0x00091670 File Offset: 0x0008FA70
	public void ReleaseHandle()
	{
		if (this.held)
		{
			Sound.PlayOneShotSound("Slider Handle Released", 1f);
		}
		this.held = false;
		this.screenPos = this.startPositionScreen;
		Vector3 tileOrigPos = this.GetTileOrigPos();
		this.handle.MoveTo(tileOrigPos.x, tileOrigPos.y, 0f);
	}

	// Token: 0x060014FB RID: 5371 RVA: 0x000916D0 File Offset: 0x0008FAD0
	public void GrabHandle(Vector2 pos)
	{
		this.SetStep();
		this.InitializeStartValue();
		this.held = true;
		this.startPositionScreen.Set(pos.x, this.screenPos.y, this.screenPos.z);
		this.screenPos = this.startPositionScreen;
		this.lastScreenPos = this.screenPos.x;
		Sound.PlayOneShotSound("Slider Handle Grabbed", 1f);
	}

	// Token: 0x060014FC RID: 5372 RVA: 0x00091744 File Offset: 0x0008FB44
	public override bool HasUIQuit()
	{
		GestureState gestureState = Blocksworld.bw.parameterEditGesture.gestureState;
		return gestureState == GestureState.Cancelled || gestureState == GestureState.Failed;
	}

	// Token: 0x060014FD RID: 5373 RVA: 0x00091770 File Offset: 0x0008FB70
	public override GameObject SetupUI(Tile tile)
	{
		base.SetupUI(tile);
		if (base.parameterIndex >= tile.gaf.Args.Length)
		{
			BWLog.Error(string.Concat(new object[]
			{
				"tile parameter index out of range for tile ",
				tile,
				" index ",
				base.parameterIndex,
				" gaf: ",
				tile.gaf.ToString()
			}));
			return null;
		}
		if (this.handle != null)
		{
			this.handle.Show(false);
		}
		this.handle = new Tile(Blocksworld.tilePool.GetTileObjectForIcon("Misc/Slider", true));
		this.handle.Show(true);
		if (this.outline != null)
		{
			this.outline.Show(false);
		}
		this.outline = new Tile(Blocksworld.tilePool.GetTileObjectForIcon("Misc/Tile_X2_Outline", true));
		this.outline.Show(true);
		this.outlineOffset = Util.CalculateTileOffset(this.outline) * NormalizedScreen.pixelScale;
		float num = 80f * NormalizedScreen.pixelScale;
		float num2 = (float)Blocksworld.marginTile * NormalizedScreen.pixelScale;
		Vector3 position = tile.tileObject.GetPosition();
		string text = string.Empty;
		TileIconInfo tileInfo = TileIconManager.Instance.GetTileInfo(tile.gaf);
		if (tileInfo != null)
		{
			text = tileInfo.backgroundColorName;
		}
		if (this.rightSide != null)
		{
			this.rightSide.Show(false);
		}
		this.rightSide = new Tile(new GAF("Block.PaintTo", new object[]
		{
			text,
			Vector3.zero
		}));
		this.rightSide.Show(true);
		this.rightSide.MoveTo(position + Vector3.right * (num + num2), false);
		if (!Options.EnableOpaqueParameterBackground)
		{
			this.rightSide.Enable(false);
		}
		this.outline.MoveTo(position.x - this.outlineOffset.x + num + num2 + 2f * NormalizedScreen.pixelScale, position.y - this.outlineOffset.y + num * 0.5f - 6f * NormalizedScreen.pixelScale, 1f);
		Vector3 tileOrigPos = this.GetTileOrigPos();
		this.handle.MoveTo(tileOrigPos.x, tileOrigPos.y, 0f);
		Blocksworld.bw.parameterEditGesture.StartEditing(this);
		this.held = false;
		this.SetValueAndStep(tile);
		this.tutorialErrorOffset = 9999999f;
		this.tutorialTargetValue = null;
		return null;
	}

	// Token: 0x060014FE RID: 5374 RVA: 0x000919FF File Offset: 0x0008FDFF
	protected Vector3 TileCenterOffset()
	{
		return new Vector3(1f, 1f) * 75f * 0.5f;
	}

	// Token: 0x060014FF RID: 5375 RVA: 0x00091A24 File Offset: 0x0008FE24
	protected Vector3 GetTileOrigPos()
	{
		if (this.tile != null && this.tile.IsShowing())
		{
			Vector3 position = this.tile.tileObject.GetPosition();
			return position + new Vector3(0.5f, -0.7f, 0f) * (float)(80 + Blocksworld.marginTile);
		}
		return Vector3.zero;
	}

	// Token: 0x06001500 RID: 5376 RVA: 0x00091A8D File Offset: 0x0008FE8D
	public override void CleanupUI()
	{
		Blocksworld.bw.parameterEditGesture.IsEnabled = false;
		this.handle.Show(false);
		this.outline.Show(false);
		this.rightSide.Show(false);
	}

	// Token: 0x06001501 RID: 5377 RVA: 0x00091AC4 File Offset: 0x0008FEC4
	public override bool UIUpdate()
	{
		float num = this.GetSnappedScreenPos();
		float num2 = num - this.startPositionScreen.x;
		bool flag = false;
		if (this.held)
		{
			this.screenPos.x = num;
			bool flag2 = false;
			if (this.held && Tutorial.state == TutorialState.SetParameter)
			{
				float f = this.tutorialErrorOffset - num2;
				if (Mathf.Abs(num2) > 2f && Mathf.Abs(f) < 15f)
				{
					num = this.startPositionScreen.x + this.tutorialErrorOffset;
					this.screenPos.x = num;
					num2 = num - this.startPositionScreen.x;
					flag2 = true;
				}
			}
			flag = this.UpdateValue();
			if (flag)
			{
				Sound.PlayOneShotSound("Slider Parameter Changed", 1f);
				this.lastScreenPos = this.screenPos.x;
			}
			if (flag2 && this.tutorialTargetValue != null)
			{
				base.objectValue = this.tutorialTargetValue;
			}
		}
		Vector3 vector = this.GetTileOrigPos() + Vector3.right * num2;
		this.handle.MoveTo(vector.x, vector.y, 0f);
		return flag;
	}

	// Token: 0x06001502 RID: 5378 RVA: 0x00091BF3 File Offset: 0x0008FFF3
	protected virtual float GetSnappedScreenPos()
	{
		return this.screenPos.x;
	}

	// Token: 0x06001503 RID: 5379
	protected abstract bool UpdateValue();

	// Token: 0x06001504 RID: 5380
	protected abstract bool ValueAtMaxOrMore();

	// Token: 0x06001505 RID: 5381
	protected abstract bool ValueAtMinOrLess();

	// Token: 0x06001506 RID: 5382
	protected abstract void SetValueAndStep(Tile tile);

	// Token: 0x06001507 RID: 5383 RVA: 0x00091C00 File Offset: 0x00090000
	protected virtual void SetStep()
	{
	}

	// Token: 0x06001508 RID: 5384
	protected abstract void InitializeStartValue();

	// Token: 0x06001509 RID: 5385 RVA: 0x00091C02 File Offset: 0x00090002
	protected virtual float GetParameterScreenOffsetError(Tile thisTile, Tile goalTile)
	{
		return 0f;
	}

	// Token: 0x04001076 RID: 4214
	public Tile handle;

	// Token: 0x04001077 RID: 4215
	protected Tile outline;

	// Token: 0x04001078 RID: 4216
	public Tile rightSide;

	// Token: 0x04001079 RID: 4217
	protected Vector3 outlineOffset;

	// Token: 0x0400107A RID: 4218
	protected bool held;

	// Token: 0x0400107B RID: 4219
	public bool onlyShowPositive;

	// Token: 0x0400107C RID: 4220
	protected int presenterSign = 1;

	// Token: 0x0400107D RID: 4221
	protected Vector3 startPositionScreen;

	// Token: 0x0400107E RID: 4222
	protected Vector3 screenPos;

	// Token: 0x0400107F RID: 4223
	protected float lastScreenPos;

	// Token: 0x04001080 RID: 4224
	protected float sensitivity;

	// Token: 0x04001081 RID: 4225
	protected const float maxOverShoot = 100f;

	// Token: 0x04001082 RID: 4226
	protected const float controlSize = 4.5f;

	// Token: 0x04001083 RID: 4227
	protected const float controlOverlap = 0f;

	// Token: 0x04001084 RID: 4228
	protected string prefixValueString = string.Empty;

	// Token: 0x04001085 RID: 4229
	protected string postfixValueString = string.Empty;

	// Token: 0x04001086 RID: 4230
	protected bool canOvershoot = true;

	// Token: 0x04001087 RID: 4231
	private const float handleZ = 0f;

	// Token: 0x04001088 RID: 4232
	protected float tutorialErrorOffset;

	// Token: 0x04001089 RID: 4233
	protected object tutorialTargetValue;
}
