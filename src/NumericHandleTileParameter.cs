using Blocks;
using Gestures;
using UnityEngine;

public abstract class NumericHandleTileParameter : EditableTileParameter
{
	public Tile handle;

	protected Tile outline;

	public Tile rightSide;

	protected Vector3 outlineOffset;

	protected bool held;

	public bool onlyShowPositive;

	protected int presenterSign = 1;

	protected Vector3 startPositionScreen;

	protected Vector3 screenPos;

	protected float lastScreenPos;

	protected float sensitivity;

	protected const float maxOverShoot = 100f;

	protected const float controlSize = 4.5f;

	protected const float controlOverlap = 0f;

	protected string prefixValueString = string.Empty;

	protected string postfixValueString = string.Empty;

	protected bool canOvershoot = true;

	private const float handleZ = 0f;

	protected float tutorialErrorOffset;

	protected object tutorialTargetValue;

	public NumericHandleTileParameter(float sensitivity = 25f, int parameterIndex = 0, int subParameterCount = 1, bool onlyShowPositive = false, string prefixValueString = "", string postfixValueString = "")
		: base(parameterIndex, useDoubleWidth: true, subParameterCount)
	{
		this.sensitivity = sensitivity / (float)NormalizedScreen.width;
		this.onlyShowPositive = onlyShowPositive;
		this.prefixValueString = prefixValueString;
		this.postfixValueString = postfixValueString;
	}

	public override void HelpSetParameterValueInTutorial(Block block, Tile thisTile, Tile goalTile)
	{
		if (Blocksworld.bw.tileParameterEditor.selectedTile != thisTile || handle == null)
		{
			Tutorial.HelpToggleTile(block, thisTile);
			return;
		}
		float parameterScreenOffsetError = GetParameterScreenOffsetError(thisTile, goalTile);
		HelpDragHandle(thisTile, parameterScreenOffsetError);
		tutorialErrorOffset = parameterScreenOffsetError;
		tutorialTargetValue = goalTile.gaf.Args[base.parameterIndex];
	}

	protected void HelpDragHandle(Tile thisTile, float offset)
	{
		Vector3 vector = handle.tileObject.GetPosition() + new Vector3(40f, 40f, 0f);
		Arrow arrow = Tutorial.arrow1;
		arrow.state = TrackingState.Screen2Screen;
		arrow.screen = vector;
		Vector3 screen = vector;
		screen.x += offset;
		arrow.screen2 = screen;
		arrow.Show(show: true);
		Tutorial.hand1.DragArrow(arrow);
		Tutorial.state = TutorialState.SetParameter;
	}

	public void HoldHandle(Vector2 pos)
	{
		screenPos.Set(pos.x, screenPos.y, screenPos.z);
		if ((ValueAtMaxOrMore() && screenPos.x > lastScreenPos) || (ValueAtMinOrLess() && screenPos.x < lastScreenPos))
		{
			if (canOvershoot)
			{
				screenPos.Set((screenPos.x + lastScreenPos) / 2f, screenPos.y, screenPos.z);
			}
			else
			{
				screenPos.x = lastScreenPos;
			}
		}
		if ((ValueAtMaxOrMore() && screenPos.x - lastScreenPos > 100f * NormalizedScreen.scale) || (ValueAtMinOrLess() && lastScreenPos - screenPos.x > 100f * NormalizedScreen.scale))
		{
			ReleaseHandle();
		}
		else
		{
			held = true;
		}
	}

	public void ReleaseHandle()
	{
		if (held)
		{
			Sound.PlayOneShotSound("Slider Handle Released");
		}
		held = false;
		screenPos = startPositionScreen;
		Vector3 tileOrigPos = GetTileOrigPos();
		handle.MoveTo(tileOrigPos.x, tileOrigPos.y, 0f);
	}

	public void GrabHandle(Vector2 pos)
	{
		SetStep();
		InitializeStartValue();
		held = true;
		startPositionScreen.Set(pos.x, screenPos.y, screenPos.z);
		screenPos = startPositionScreen;
		lastScreenPos = screenPos.x;
		Sound.PlayOneShotSound("Slider Handle Grabbed");
	}

	public override bool HasUIQuit()
	{
		GestureState gestureState = Blocksworld.bw.parameterEditGesture.gestureState;
		if (gestureState != GestureState.Cancelled)
		{
			return gestureState == GestureState.Failed;
		}
		return true;
	}

	public override GameObject SetupUI(Tile tile)
	{
		base.SetupUI(tile);
		if (base.parameterIndex >= tile.gaf.Args.Length)
		{
			BWLog.Error(string.Concat("tile parameter index out of range for tile ", tile, " index ", base.parameterIndex, " gaf: ", tile.gaf.ToString()));
			return null;
		}
		if (handle != null)
		{
			handle.Show(show: false);
		}
		handle = new Tile(Blocksworld.tilePool.GetTileObjectForIcon("Misc/Slider", enabled: true));
		handle.Show(show: true);
		if (outline != null)
		{
			outline.Show(show: false);
		}
		outline = new Tile(Blocksworld.tilePool.GetTileObjectForIcon("Misc/Tile_X2_Outline", enabled: true));
		outline.Show(show: true);
		outlineOffset = Util.CalculateTileOffset(outline) * NormalizedScreen.pixelScale;
		float num = 80f * NormalizedScreen.pixelScale;
		float num2 = (float)Blocksworld.marginTile * NormalizedScreen.pixelScale;
		Vector3 position = tile.tileObject.GetPosition();
		string text = string.Empty;
		TileIconInfo tileInfo = TileIconManager.Instance.GetTileInfo(tile.gaf);
		if (tileInfo != null)
		{
			text = tileInfo.backgroundColorName;
		}
		if (rightSide != null)
		{
			rightSide.Show(show: false);
		}
		rightSide = new Tile(new GAF("Block.PaintTo", text, Vector3.zero));
		rightSide.Show(show: true);
		rightSide.MoveTo(position + Vector3.right * (num + num2));
		if (!Options.EnableOpaqueParameterBackground)
		{
			rightSide.Enable(enabled: false);
		}
		outline.MoveTo(position.x - outlineOffset.x + num + num2 + 2f * NormalizedScreen.pixelScale, position.y - outlineOffset.y + num * 0.5f - 6f * NormalizedScreen.pixelScale, 1f);
		Vector3 tileOrigPos = GetTileOrigPos();
		handle.MoveTo(tileOrigPos.x, tileOrigPos.y, 0f);
		Blocksworld.bw.parameterEditGesture.StartEditing(this);
		held = false;
		SetValueAndStep(tile);
		tutorialErrorOffset = 9999999f;
		tutorialTargetValue = null;
		return null;
	}

	protected Vector3 TileCenterOffset()
	{
		return new Vector3(1f, 1f) * 75f * 0.5f;
	}

	protected Vector3 GetTileOrigPos()
	{
		if (tile != null && tile.IsShowing())
		{
			Vector3 position = tile.tileObject.GetPosition();
			return position + new Vector3(0.5f, -0.7f, 0f) * (80 + Blocksworld.marginTile);
		}
		return Vector3.zero;
	}

	public override void CleanupUI()
	{
		Blocksworld.bw.parameterEditGesture.IsEnabled = false;
		handle.Show(show: false);
		outline.Show(show: false);
		rightSide.Show(show: false);
	}

	public override bool UIUpdate()
	{
		float snappedScreenPos = GetSnappedScreenPos();
		float num = snappedScreenPos - startPositionScreen.x;
		bool flag = false;
		if (held)
		{
			screenPos.x = snappedScreenPos;
			bool flag2 = false;
			if (held && Tutorial.state == TutorialState.SetParameter)
			{
				float f = tutorialErrorOffset - num;
				if (Mathf.Abs(num) > 2f && Mathf.Abs(f) < 15f)
				{
					snappedScreenPos = startPositionScreen.x + tutorialErrorOffset;
					screenPos.x = snappedScreenPos;
					num = snappedScreenPos - startPositionScreen.x;
					flag2 = true;
				}
			}
			flag = UpdateValue();
			if (flag)
			{
				Sound.PlayOneShotSound("Slider Parameter Changed");
				lastScreenPos = screenPos.x;
			}
			if (flag2 && tutorialTargetValue != null)
			{
				base.objectValue = tutorialTargetValue;
			}
		}
		Vector3 vector = GetTileOrigPos() + Vector3.right * num;
		handle.MoveTo(vector.x, vector.y, 0f);
		return flag;
	}

	protected virtual float GetSnappedScreenPos()
	{
		return screenPos.x;
	}

	protected abstract bool UpdateValue();

	protected abstract bool ValueAtMaxOrMore();

	protected abstract bool ValueAtMinOrLess();

	protected abstract void SetValueAndStep(Tile tile);

	protected virtual void SetStep()
	{
	}

	protected abstract void InitializeStartValue();

	protected virtual float GetParameterScreenOffsetError(Tile thisTile, Tile goalTile)
	{
		return 0f;
	}
}
