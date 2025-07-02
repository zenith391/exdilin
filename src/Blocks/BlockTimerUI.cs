using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockTimerUI : BlockAbstractLimitedCounterUI
{
	private enum TimerState
	{
		STOPPED,
		RUNNING,
		PAUSED_SINGLE_FRAME
	}

	private Rect rect;

	protected int direction = 1;

	private string prevTimeText = string.Empty;

	private HudMeshLabel label;

	private TimerState timerState;

	public static Dictionary<int, BlockTimerUI> allTimerBlocks = new Dictionary<int, BlockTimerUI>();

	private TileCustom timerTile;

	private int[] timeComponents = new int[3];

	public BlockTimerUI(List<List<Tile>> tiles, int index)
		: base(tiles, index)
	{
		defaultSingleGaf = null;
		maxValue = int.MaxValue;
		noTileTexture = "Plain";
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockTimerUI>("TimerUI.SetMaxValue", null, (Block b) => ((BlockTimerUI)b).SetTimerMaxValue, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockTimerUI>("TimerUI.Flash", null, (Block b) => ((BlockAbstractUI)b).Flash, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockTimerUI>("TimerUI.SetText", null, (Block b) => ((BlockAbstractCounterUI)b).SetText, new Type[1] { typeof(string) });
		PredicateRegistry.Add<BlockTimerUI>("TimerUI.ShowUI", null, (Block b) => ((BlockAbstractCounterUI)b).ShowUI, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockTimerUI>("TimerUI.ShowPhysical", null, (Block b) => ((BlockAbstractUI)b).ShowPhysical, new Type[1] { typeof(int) });
	}

	public override void Play()
	{
		base.Play();
		maxValue = int.MaxValue;
		timerState = TimerState.STOPPED;
		direction = 1;
		rect = GetRect();
		allTimerBlocks[index] = this;
	}

	public override void Stop(bool resetBlock)
	{
		base.Stop(resetBlock);
		allTimerBlocks.Clear();
		DestroyTile(ref timerTile);
	}

	public bool IsRunning()
	{
		return timerState == TimerState.RUNNING;
	}

	public int GetDirection()
	{
		return direction;
	}

	public Rect GetRect()
	{
		float num = 150f;
		float num2 = 50f;
		float x = (float)Screen.width - 200f * NormalizedScreen.scale;
		float y = NormalizedScreen.scale * 20f;
		return new Rect(x, y, num * NormalizedScreen.scale, num2 * NormalizedScreen.scale);
	}

	public override void Update()
	{
		base.Update();
		if (Blocksworld.CurrentState == State.Play)
		{
			UpdateTile(ref timerTile);
			if (timerTile != null)
			{
				Vector2 vector = new Vector2(rect.x - 80f, NormalizedScreen.height - 80);
				timerTile.MoveTo(vector.x, vector.y, 2f);
			}
		}
	}

	public override void OnHudMesh()
	{
		base.OnHudMesh();
		if (!base.uiVisible || !isDefined || Blocksworld.CurrentState != State.Play)
		{
			return;
		}
		string text;
		if (uiPaused)
		{
			text = prevTimeText;
		}
		else
		{
			text = string.Empty;
			float secondsRaw = (float)currentValue * Blocksworld.fixedDeltaTime;
			TimeTileParameter.CalculateTimeComponents(secondsRaw, timeComponents);
			for (int num = 2; num >= 0; num--)
			{
				text += timeComponents[num].ToString("D2");
				if (num > 0)
				{
					text += ":";
				}
			}
			if (base.text.Length > 0)
			{
				text = base.text + ": " + text;
			}
		}
		HudMeshOnGUI.Label(ref textLabel, rect, text, textColor, style);
		HudMeshOnGUI.Label(ref textOutlineLabel, rect, text, outlineStyle);
		prevTimeText = text;
	}

	public bool CanBePaused()
	{
		return timerState != TimerState.STOPPED;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (timerState == TimerState.RUNNING && isDefined)
		{
			currentValue = Mathf.Clamp(currentValue + direction, 0, maxValue);
		}
		else if (timerState == TimerState.PAUSED_SINGLE_FRAME)
		{
			timerState = TimerState.RUNNING;
		}
		previousValue = currentValue;
	}

	public TileResultCode ValueCondition(float floatValue, int condition)
	{
		if (isDefined)
		{
			int num = Mathf.RoundToInt(floatValue / Blocksworld.fixedDeltaTime);
			int num2 = currentValue + extraValue;
			bool flag = false;
			switch (condition)
			{
			case 2:
				flag = num2 != num;
				break;
			case 1:
				flag = num2 > num;
				break;
			case 0:
				flag = num2 < num;
				break;
			}
			if (flag)
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

	public TileResultCode TimeEquals(float time)
	{
		if (isDefined)
		{
			int num = Mathf.RoundToInt(time / Blocksworld.fixedDeltaTime);
			if (num == currentValue)
			{
				return TileResultCode.True;
			}
		}
		return TileResultCode.False;
	}

	public void StartTimer(int dir)
	{
		if (isDefined)
		{
			direction = dir;
			timerState = TimerState.RUNNING;
		}
	}

	public void SetTime(float time)
	{
		currentValue = Mathf.RoundToInt(time / Blocksworld.fixedDeltaTime);
		isDefined = true;
	}

	public void IncrementTime(float time)
	{
		if (isDefined)
		{
			int num = Mathf.RoundToInt(time / Blocksworld.fixedDeltaTime);
			currentValue = Mathf.Max(0, currentValue + num);
		}
	}

	public void PauseOneFrame()
	{
		if (timerState == TimerState.RUNNING)
		{
			timerState = TimerState.PAUSED_SINGLE_FRAME;
		}
	}

	public TileResultCode SetTimerMaxValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int num = Mathf.RoundToInt(Util.GetFloatArg(args, 0, 60f) / Blocksworld.fixedDeltaTime);
		SetMaxValue(num);
		return TileResultCode.True;
	}

	protected override void SetUIVisible(bool v)
	{
		base.SetUIVisible(v);
		if (dirty && timerTile != null)
		{
			timerTile.Show(v);
		}
	}
}
