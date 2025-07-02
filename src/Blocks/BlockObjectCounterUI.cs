using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockObjectCounterUI : BlockAbstractLimitedCounterUI
{
	private const int DEFAULT_TILE_COUNT_LIMIT = 7;

	protected int currentTileCountLimit = 7;

	protected bool maxFromTreasure = true;

	protected float scaledCurrentTileSize = 80f;

	private HudMeshLabel counterTextLabel;

	private HudMeshLabel counterOutlineLabel;

	private bool didHideCounterBehindSpeechBubble;

	private TileCustom[] counterTiles;

	public BlockObjectCounterUI(List<List<Tile>> tiles, int index)
		: base(tiles, index)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockObjectCounterUI>("Leaderboard.SetType", null, (Block b) => ((BlockObjectCounterUI)b).SetLeaderboardType, new Type[1] { typeof(string) });
		PredicateRegistry.Add<BlockObjectCounterUI>("Leaderboard.PauseTime", (Block b) => ((BlockObjectCounterUI)b).IsPauseLeaderboardTime, (Block b) => ((BlockObjectCounterUI)b).PauseLeaderboardTime, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockObjectCounterUI>("Leaderboard.AddTime", null, (Block b) => ((BlockObjectCounterUI)b).AddLeaderboardTime, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockObjectCounterUI>("Leaderboard.SubtractTime", null, (Block b) => ((BlockObjectCounterUI)b).SubtractLeaderboardTime, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockObjectCounterUI>("ObjectCounterUI.SetMaxValue", null, (Block b) => ((BlockObjectCounterUI)b).SetMaxValue, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockObjectCounterUI>("ObjectCounterUI.SetText", null, (Block b) => ((BlockAbstractCounterUI)b).SetText, new Type[1] { typeof(string) }, new string[1] { "Text" });
		PredicateRegistry.Add<BlockObjectCounterUI>("ObjectCounterUI.ShowUI", null, (Block b) => ((BlockAbstractUI)b).ShowUI, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockObjectCounterUI>("ObjectCounterUI.ShowPhysical", null, (Block b) => ((BlockAbstractUI)b).ShowPhysical, new Type[1] { typeof(int) });
		List<List<Tile>> list = new List<List<Tile>>();
		list.Add(new List<Tile>
		{
			Block.FirstFrameTile(),
			Block.ThenTile(),
			new Tile(new GAF("Leaderboard.SetType", "ShortestTime"))
		});
		list.Add(new List<Tile>
		{
			Block.FirstFrameTile(),
			Block.ThenTile(),
			new Tile(new GAF("ObjectCounterUI.Equals", 0, 0))
		});
		list.Add(new List<Tile>
		{
			new Tile(new GAF("ObjectCounterUI.EqualsMax", 0)),
			Block.ThenTile(),
			new Tile(new GAF("Meta.GameWin", "You win!", 0f))
		});
		list.Add(Block.EmptyTileRow());
		List<List<Tile>> value = list;
		Block.defaultExtraTiles["UI Object Counter I"] = value;
	}

	public override void Play()
	{
		base.Play();
		maxValue = 5;
		maxFromTreasure = true;
		TreasureHandler.RegisterObjectCounter(this);
	}

	public int GetTileCountLimit(GAF gaf)
	{
		int result = 7;
		CommonIconOverride iconOverride = GetIconOverride(gaf);
		if (iconOverride != null && iconOverride.tileCountLimit > 0)
		{
			result = iconOverride.tileCountLimit;
		}
		return result;
	}

	private void UpdateTileArray()
	{
		if (!base.uiVisible)
		{
			if (counterTiles != null)
			{
				for (int i = 0; i < counterTiles.Length; i++)
				{
					counterTiles[i].Show(show: false);
				}
			}
			return;
		}
		if (base.uiVisible && counterTiles != null)
		{
			if (Blocksworld.UI.SpeechBubble.ActiveCount() > 0)
			{
				didHideCounterBehindSpeechBubble = false;
				List<Rect> list = Blocksworld.UI.SpeechBubble.ActiveScreenRects();
				for (int j = 0; j < counterTiles.Length; j++)
				{
					Rect rectForTile = GetRectForTile(counterTiles[j]);
					for (int k = 0; k < list.Count; k++)
					{
						Rect r = list[k];
						bool flag = rectForTile.Intersects(r);
						counterTiles[j].Show(!flag);
						didHideCounterBehindSpeechBubble |= flag;
					}
				}
			}
			else if (didHideCounterBehindSpeechBubble)
			{
				didHideCounterBehindSpeechBubble = false;
				textureDirty = true;
			}
		}
		string texture = GetTexture();
		bool flag2 = iconGaf == null && texture == "Plain";
		if (!flag2 && iconGaf != null)
		{
			flag2 = iconGaf.Predicate == Block.predicateTextureTo && (string)iconGaf.Args[0] == "Plain";
		}
		if (counterTiles == null || (maxValue > currentTileCountLimit && counterTiles.Length != 1) || (maxValue <= currentTileCountLimit && counterTiles.Length != maxValue) || textureDirty)
		{
			int num = maxValue;
			GAF gAF = ((iconGaf == null) ? new GAF("Block.TextureTo", texture, Vector3.zero) : iconGaf.Clone());
			currentTileCountLimit = GetTileCountLimit(gAF);
			scaledCurrentTileSize = (float)GetTileSize(gAF) * NormalizedScreen.pixelScale;
			if (maxValue > currentTileCountLimit)
			{
				num = 1;
			}
			if (counterTiles != null)
			{
				for (int l = 0; l < counterTiles.Length; l++)
				{
					TileCustom tileCustom = counterTiles[l];
					tileCustom.Destroy();
				}
			}
			counterTiles = new TileCustom[num];
			for (int m = 0; m < counterTiles.Length; m++)
			{
				if (flag2)
				{
					TileCustom tileCustom2 = new TileCustom(Blocksworld.tilePool.GetTileObjectForIcon(starDisabledIconName, enabled: false));
					counterTiles[m] = tileCustom2;
					continue;
				}
				GAF newGaf = gAF.Clone();
				TileCustom orCreateTile = GetOrCreateTile(newGaf, customBackgroundColors);
				orCreateTile.Enable(enable: false);
				counterTiles[m] = orCreateTile;
			}
		}
		if (!dirty && !textureDirty)
		{
			return;
		}
		float num2 = scaledCurrentTileSize * 0.15f;
		float num3 = (float)counterTiles.Length * scaledCurrentTileSize + (float)(counterTiles.Length - 1) * num2;
		float num4 = (float)NormalizedScreen.width * 0.5f - num3 * 0.5f;
		float num5 = 140f;
		float y = (float)NormalizedScreen.height - num5 * NormalizedScreen.pixelScale;
		for (int n = 0; n < counterTiles.Length; n++)
		{
			TileCustom tileCustom3 = counterTiles[n];
			tileCustom3.Show(base.uiVisible);
			if (base.uiVisible)
			{
				bool flag3 = n < currentValue + extraValue || maxValue > currentTileCountLimit;
				if (flag2)
				{
					string icon = ((!flag3) ? starDisabledIconName : starEnabledIconName);
					counterTiles[n].SetIcon(icon);
					counterTiles[n].Enable(enable: true);
				}
				else
				{
					tileCustom3.Enable(flag3);
				}
				tileCustom3.MoveTo(num4, y, 2f);
			}
			num4 += scaledCurrentTileSize + num2;
		}
	}

	private Rect GetRectForTile(TileCustom tile)
	{
		return new Rect(tile.position.x, tile.position.y, scaledCurrentTileSize, scaledCurrentTileSize);
	}

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

	public override void Update()
	{
		base.Update();
		if (isDefined && Blocksworld.CurrentState == State.Play)
		{
			UpdateTileArray();
			dirty = false;
			textureDirty = false;
		}
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		if (counterTiles != null)
		{
			for (int i = 0; i < counterTiles.Length; i++)
			{
				TileCustom tileCustom = counterTiles[i];
				tileCustom.Destroy();
			}
		}
		counterTiles = null;
	}

	public override void OnHudMesh()
	{
		base.OnHudMesh();
		if (base.uiVisible && Blocksworld.CurrentState == State.Play)
		{
			if (isDefined && counterTiles != null && counterTiles.Length == 1 && maxValue != 1 && counterTiles[0].IsShown())
			{
				string text = currentValue + extraValue + " / " + maxValue;
				Rect rectForTile = GetRectForTile(counterTiles[0]);
				rectForTile.y = (float)Screen.height - NormalizedScreen.scale * (rectForTile.y + rectForTile.height * 0.55f);
				rectForTile.x = NormalizedScreen.scale * (rectForTile.x + rectForTile.width * 1.5f);
				HudMeshOnGUI.Label(rect: new Rect(rectForTile.x + 2f, rectForTile.y + 2f, rectForTile.width, rectForTile.height), label: ref counterOutlineLabel, text: text, style: outlineStyle);
				HudMeshOnGUI.Label(ref counterTextLabel, rectForTile, text, textColor, style);
			}
			if (!string.IsNullOrEmpty(base.text))
			{
				float num = (float)Screen.width * 0.85f;
				float x = ((float)Screen.width - num) * 0.5f;
				float num2 = 44f;
				float y = num2 * NormalizedScreen.scale;
				Rect rect = new Rect(x, y, num, 40f * NormalizedScreen.scale);
				HudMeshOnGUI.Label(rect: new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height), label: ref textOutlineLabel, text: base.text, style: outlineStyle);
				HudMeshOnGUI.Label(ref textLabel, rect, base.text, textColor, style);
			}
		}
	}

	public void UpdateTreasureState(int available, int pickedUp)
	{
		if (maxFromTreasure)
		{
			dirty |= maxValue != available;
			maxValue = available;
		}
		int num = extraValue;
		extraValue = pickedUp;
		int num2 = currentValue;
		AdjustCurrentValue();
		dirty |= extraValue != num;
		dirty |= num2 != currentValue;
		isDefined = true;
	}

	protected override void SetMaxValue(int newMax)
	{
		base.SetMaxValue(newMax);
		maxFromTreasure = false;
	}

	public TileResultCode SetLeaderboardType(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}

	public TileResultCode IsPauseLeaderboardTime(ScriptRowExecutionInfo eInfo, object[] args)
	{
		bool flag = Util.GetIntArg(args, 0, 0) != 0;
		return boolToTileResult(flag == Blocksworld.leaderboardData.IsTimePaused());
	}

	public TileResultCode PauseLeaderboardTime(ScriptRowExecutionInfo eInfo, object[] args)
	{
		bool pause = Util.GetIntArg(args, 0, 0) != 0;
		Blocksworld.leaderboardData.PauseTime(pause);
		return TileResultCode.True;
	}

	public TileResultCode AddLeaderboardTime(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 0f);
		Blocksworld.leaderboardData.AddTime(floatArg);
		return TileResultCode.True;
	}

	public TileResultCode SubtractLeaderboardTime(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 0f);
		Blocksworld.leaderboardData.AddTime(0f - floatArg);
		return TileResultCode.True;
	}
}
