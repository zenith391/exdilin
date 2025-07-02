using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Blocks;

public class BlockRadarUI : BlockAbstractUI
{
	private const float defaultMaxDistance = 50f;

	protected float maxDistance = 50f;

	protected HashSet<string> prevFollowedTags = new HashSet<string>();

	protected HashSet<string> followedTags = new HashSet<string>();

	protected string centerTag;

	protected HashSet<string> guiFollowedTags = new HashSet<string>();

	private static Dictionary<string, Color> tagColors = new Dictionary<string, Color>();

	private static Dictionary<string, string> tagNameCompares = new Dictionary<string, string>();

	private static Dictionary<string, List<Tile>> tagTiles = new Dictionary<string, List<Tile>>();

	public static Predicate predicateRadarTrackTag;

	public static Predicate predicateRadarCenterOnTag;

	private bool radarActive;

	private bool alwaysSee;

	private Vector2 backgroundPosition;

	private float minY;

	private float maxY;

	private Rect radarRect;

	private Rect textRect;

	private HudMeshStyle style;

	private HudMeshStyle outlineStyle;

	protected HudMeshLabel textLabel;

	protected HudMeshLabel textOutlineLabel;

	protected Color textColor = Color.white;

	private Transform sweepMesh;

	private float sweepAngle;

	private static Dictionary<string, string> defaultTagColorNames = new Dictionary<string, string>
	{
		{ "0", "Orange" },
		{ "1", "Yellow" },
		{ "2", "Yellow Green" },
		{ "3", "Green" },
		{ "4", "Teal" },
		{ "5", "Blue" },
		{ "6", "Magenta" },
		{ "7", "Purple" },
		{ "8", "Pink" },
		{ "A", "White" },
		{ "B", "Deep Red" }
	};

	private const int MAX_BLIPS = 25;

	private Dictionary<string, List<Block>> sortedBlockList = new Dictionary<string, List<Block>>();

	private const float HEIGHT_DISTANCE_CHECK = 5f;

	private const float HEIGHT_COLOR_MULTIPLIER = 0.15f;

	public BlockRadarUI(List<List<Tile>> tiles, int index)
		: base(tiles, index)
	{
		sweepMesh = goT.Find("UINRadarSweep_RdUpR");
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockRadarUI>("RadarUI.SetMaxDistance", null, (Block b) => ((BlockRadarUI)b).SetMaxDistance, new Type[1] { typeof(float) });
		predicateRadarTrackTag = PredicateRegistry.Add<BlockRadarUI>("RadarUI.TrackTag", (Block b) => ((BlockRadarUI)b).TagWithinRange, (Block b) => ((BlockRadarUI)b).TrackTag, new Type[3]
		{
			typeof(string),
			typeof(int),
			typeof(string)
		});
		PredicateRegistry.Add<BlockRadarUI>("RadarUI.SetText", null, (Block b) => ((BlockRadarUI)b).SetText, new Type[1] { typeof(string) });
		predicateRadarCenterOnTag = PredicateRegistry.Add<BlockRadarUI>("RadarUI.CenterOnTag", null, (Block b) => ((BlockRadarUI)b).CenterOnTag, new Type[1] { typeof(string) });
		PredicateRegistry.Add<BlockRadarUI>("RadarUI.ShowUI", null, (Block b) => ((BlockAbstractUI)b).ShowUI, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockRadarUI>("RadarUI.SeeForever", null, (Block b) => ((BlockRadarUI)b).SeeForever, new Type[1] { typeof(int) });
		List<GAF> list = new List<GAF>();
		list.Add(new GAF("RadarUI.SetMaxDistance", 50f));
		list.Add(new GAF("RadarUI.SeeForever"));
		Block.AddSimpleDefaultTiles(list, "UI Radar I");
		if (true)
		{
			Blocksworld.UI.SetRadarUIActive(active: false);
		}
		SetNameCompares();
	}

	private void ActivateRadar(bool enabled)
	{
		if (radarActive != enabled)
		{
			radarActive = enabled;
			Blocksworld.UI.SetRadarUIActive(enabled);
			Blocksworld.UI.SetRadarUICenterTagActive(centerTag);
		}
	}

	protected override void SetUIVisible(bool v)
	{
		base.SetUIVisible(v);
		if (dirty)
		{
			ActivateRadar(base.uiVisible);
		}
	}

	private static void SetNameCompares()
	{
		string[] tagNames = Tile.tagNames;
		string[] shortTagNames = Tile.shortTagNames;
		for (int i = 0; i < Mathf.Min(tagNames.Length, shortTagNames.Length); i++)
		{
			if (!tagNameCompares.ContainsKey(shortTagNames[i]))
			{
				tagNameCompares.Add(shortTagNames[i], tagNames[i]);
			}
		}
	}

	public override void Play()
	{
		base.Play();
		style = HudMeshOnGUI.dataSource.GetStyle("Counter");
		outlineStyle = HudMeshOnGUI.dataSource.GetStyle("Outline");
		radarRect = Blocksworld.UI.GetRadarUIRect();
		textRect = GetRadarTextRect(radarRect);
		radarRect.x /= NormalizedScreen.scale;
		radarRect.y /= NormalizedScreen.scale;
		radarRect.width /= NormalizedScreen.scale;
		radarRect.height /= NormalizedScreen.scale;
		followedTags.Clear();
		guiFollowedTags.Clear();
		centerTag = string.Empty;
		maxDistance = 50f;
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		maxDistance = 50f;
		ActivateRadar(enabled: false);
		ClearTiles();
	}

	private void ClearTiles()
	{
		foreach (KeyValuePair<string, List<Tile>> tagTile in tagTiles)
		{
			foreach (Tile item in tagTile.Value)
			{
				item.DestroyPhysical();
			}
		}
		tagTiles.Clear();
		tagColors.Clear();
	}

	public TileResultCode SeeForever(ScriptRowExecutionInfo eInfo, object[] args)
	{
		alwaysSee = args.Length == 0 || (int)args[0] != 1;
		return TileResultCode.True;
	}

	public override TileResultCode SetText(ScriptRowExecutionInfo eInfo, object[] args)
	{
		text = ((args.Length == 0) ? string.Empty : ParseText((string)args[0]));
		return TileResultCode.True;
	}

	public TileResultCode SetMaxDistance(ScriptRowExecutionInfo eInfo, object[] args)
	{
		maxDistance = ((args.Length == 0) ? 50f : ((float)args[0]));
		return TileResultCode.True;
	}

	public TileResultCode TrackTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string text = ((args.Length == 0) ? string.Empty : ((string)args[0]));
		string colorName = ((args.Length <= 2) ? "White" : ((string)args[2]));
		SetColor(text, colorName);
		if (text.Length > 0)
		{
			followedTags.Add(text);
			guiFollowedTags.Add(text);
		}
		return TileResultCode.True;
	}

	public TileResultCode CenterOnTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string a = ((args.Length == 0) ? string.Empty : ((string)args[0]));
		if (!string.Equals(a, centerTag))
		{
			centerTag = a;
			Blocksworld.UI.SetRadarUICenterTagActive(centerTag);
		}
		return TileResultCode.True;
	}

	public TileResultCode TagWithinRange(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string tagName = ((args.Length == 0) ? string.Empty : ((string)args[0]));
		Vector3 position = goT.position;
		if (TagManager.TryGetClosestBlockWithTag(tagName, position, out var block) && (block.goT.position - position).sqrMagnitude < maxDistance * maxDistance)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public override void ResetFrame()
	{
		alwaysSee = false;
	}

	private void TrackAllTags()
	{
		Color[] value = new Color[2]
		{
			Color.white,
			Color.white
		};
		for (int i = 0; i < Tile.shortTagNames.Length; i++)
		{
			if (centerTag != Tile.shortTagNames[i])
			{
				guiFollowedTags.Add(Tile.shortTagNames[i]);
				Blocksworld.colorDefinitions.TryGetValue(defaultTagColorNames[Tile.shortTagNames[i]], out value);
				tagColors[Tile.shortTagNames[i]] = value[0];
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Blocksworld.CurrentState != State.Play)
		{
			return;
		}
		ActivateRadar(base.uiVisible);
		if (!base.uiVisible)
		{
			return;
		}
		bool flag = false;
		foreach (string followedTag in followedTags)
		{
			if (!prevFollowedTags.Contains(followedTag))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			foreach (string prevFollowedTag in prevFollowedTags)
			{
				if (!followedTags.Contains(prevFollowedTag))
				{
					flag = true;
					break;
				}
			}
		}
		if (flag || guiFollowedTags.Count == 0)
		{
			guiFollowedTags.Clear();
			if (followedTags.Count == 0)
			{
				TrackAllTags();
			}
			else
			{
				foreach (string followedTag2 in followedTags)
				{
					guiFollowedTags.Add(followedTag2);
				}
			}
		}
		prevFollowedTags.Clear();
		foreach (string followedTag3 in followedTags)
		{
			prevFollowedTags.Add(followedTag3);
		}
		followedTags.Clear();
		sweepAngle += 2f;
		if (sweepAngle > 360f)
		{
			sweepAngle -= 360f;
		}
	}

	private static Rect GetRadarTextRect(Rect radarDisplayRect)
	{
		float width = radarDisplayRect.width;
		float height = 65f * NormalizedScreen.scale;
		float x = radarDisplayRect.x;
		float y = 32f * NormalizedScreen.scale;
		return new Rect(x, y, width, height);
	}

	private Tile GetTile(string tag, Dictionary<string, int> ourIndices)
	{
		int num = 0;
		if (ourIndices.ContainsKey(tag))
		{
			num = ourIndices[tag];
		}
		tagNameCompares.TryGetValue(tag, out var value);
		string iconName = "Radar/Radar_Tag_" + value;
		Tile tile;
		if (!tagTiles.ContainsKey(tag))
		{
			tile = new Tile(Blocksworld.tilePool.GetTileObjectForIcon(iconName, enabled: true));
			tagTiles.Add(tag, new List<Tile>());
			tagTiles[tag].Add(tile);
		}
		else if (tagTiles[tag].Count < num)
		{
			tile = new Tile(Blocksworld.tilePool.GetTileObjectForIcon(iconName, enabled: true));
			tagTiles[tag].Add(tile);
		}
		else
		{
			tile = tagTiles[tag][num - 1];
		}
		return tile;
	}

	private void SetColor(string tagName, string colorName)
	{
		Color[] value = new Color[1] { Color.white };
		Blocksworld.colorDefinitions.TryGetValue(colorName, out value);
		tagColors[tagName] = value[0];
	}

	private Color GetColor(string tag)
	{
		Color value = Color.white;
		tagColors.TryGetValue(tag, out value);
		return value;
	}

	private void GetSortedBlockList(Vector3 ourCenter)
	{
		sortedBlockList.Clear();
		List<Block> list = new List<Block>();
		foreach (string guiFollowedTag in guiFollowedTags)
		{
			List<Block> blocksWithTag = TagManager.GetBlocksWithTag(guiFollowedTag);
			for (int i = 0; i < blocksWithTag.Count; i++)
			{
				Block block = blocksWithTag[i];
				if ((!block.isTreasure || (!TreasureHandler.IsHiddenTreasureModel(block) && TreasureHandler.GetTreasureModelScale(block) >= 0.15f)) && !list.Contains(block))
				{
					list.Add(block);
				}
			}
		}
		IEnumerable<Block> source = list;
		source = source.OrderBy((Block taggedBlock) => (taggedBlock.goT.position - ourCenter).sqrMagnitude);
		list = source.ToList();
		if (list.Count > 25)
		{
			list.RemoveRange(25, list.Count - 25);
		}
		foreach (Block item2 in list)
		{
			float y = item2.goT.position.y;
			minY = Mathf.Min(y, minY);
			maxY = Mathf.Max(y, maxY);
		}
		foreach (string guiFollowedTag2 in guiFollowedTags)
		{
			List<Block> blocksWithTag2 = TagManager.GetBlocksWithTag(guiFollowedTag2);
			List<Block> list2 = new List<Block>();
			for (int num = 0; num < blocksWithTag2.Count; num++)
			{
				Block item = blocksWithTag2[num];
				if (list.Contains(item))
				{
					list2.Add(item);
				}
			}
			sortedBlockList.Add(guiFollowedTag2, list2);
		}
	}

	public override void OnHudMesh()
	{
		base.OnHudMesh();
		if (base.uiVisible && Blocksworld.CurrentState == State.Play && !string.IsNullOrEmpty(text))
		{
			HudMeshOnGUI.Label(rect: new Rect(textRect.x + 2f, textRect.y + 2f, textRect.width, textRect.height), label: ref textOutlineLabel, text: text, style: outlineStyle);
			HudMeshOnGUI.Label(ref textLabel, textRect, text, textColor, style);
		}
	}

	private Vector3 GetRadarCenter()
	{
		if (Blocksworld.blocksworldCamera.firstPersonBlock != null)
		{
			return Blocksworld.blocksworldCamera.firstPersonBlock.go.transform.position;
		}
		if (string.IsNullOrEmpty(centerTag))
		{
			return Blocksworld.blocksworldCamera.GetTargetPosition();
		}
		if (TagManager.TryGetClosestBlockWithTag(centerTag, goT.position, out var block))
		{
			return block.goT.position;
		}
		return goT.position;
	}

	public override void Update()
	{
		base.Update();
		HashSet<Tile> hashSet = new HashSet<Tile>();
		foreach (KeyValuePair<string, List<Tile>> tagTile in tagTiles)
		{
			for (int i = 0; i < tagTile.Value.Count; i++)
			{
				if (tagTile.Value[i].IsShowing())
				{
					hashSet.Add(tagTile.Value[i]);
				}
			}
		}
		if (base.uiVisible && Blocksworld.CurrentState == State.Play)
		{
			Vector3 cameraRight = Blocksworld.cameraRight;
			Vector3 vec = -Blocksworld.cameraForward;
			Vector3 radarCenter = GetRadarCenter();
			Vector2 vector = new Vector2(radarCenter.x, radarCenter.z);
			Vector2 center = radarRect.center;
			float num = 30f;
			float num2 = radarRect.width * 0.45f;
			Vector3 normalized = Util.ProjectOntoPlane(cameraRight, Vector3.up).normalized;
			Vector3 normalized2 = Util.ProjectOntoPlane(vec, Vector3.up).normalized;
			Vector2 lhs = new Vector2(normalized.x, normalized.z);
			Vector2 lhs2 = new Vector2(normalized2.x, normalized2.z);
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			GetSortedBlockList(radarCenter);
			foreach (KeyValuePair<string, List<Block>> sortedBlock in sortedBlockList)
			{
				string key = sortedBlock.Key;
				foreach (Block item in sortedBlock.Value)
				{
					if (!item.go.activeInHierarchy)
					{
						continue;
					}
					Vector3 position = item.goT.position;
					Vector2 vector2 = new Vector2(position.x, position.z);
					Vector2 rhs = vector2 - vector;
					float num3 = maxDistance * maxDistance;
					float num4 = num3 * 0.8f * 0.8f;
					bool flag = rhs.sqrMagnitude < num3;
					bool flag2 = rhs.sqrMagnitude > num4;
					if (flag || alwaysSee)
					{
						int value = 1;
						if (!dictionary.ContainsKey(key))
						{
							dictionary[key] = value;
						}
						else
						{
							dictionary[key]++;
						}
						Tile tile = GetTile(key, dictionary);
						hashSet.Remove(tile);
						Vector2 vector3 = new Vector2(Vector2.Dot(lhs, rhs), 0f - Vector2.Dot(lhs2, rhs));
						Vector2 vector4 = vector3 / maxDistance;
						Vector2 vector5 = center + new Vector2(vector4.x * num2, vector4.y * num2);
						Rect rect = new Rect(vector5.x, vector5.y, num, num);
						float num5 = TreasureHandler.GetTreasureModelScale(item);
						if (!flag)
						{
							Vector2 vector6 = vector5 - center;
							Vector2 vector7 = vector6 / vector6.magnitude;
							rect.position = center + vector7 * num2;
							num5 *= 0.5f;
						}
						else if (flag2)
						{
							float t = (num3 - rhs.sqrMagnitude) / (num3 - num4);
							num5 = Mathf.Lerp(0.5f, 1f, t);
						}
						float num6 = position.y - radarCenter.y;
						float z = 0f;
						Color color = GetColor(key);
						tile.Show(show: true);
						if (tile.tileObject != null)
						{
							tile.tileObject.SetScale(Vector3.one * num5);
							tile.SetTileForegroundColor(color);
							rect.position -= Vector2.one * num5 * num + Vector2.one * 2f;
							tile.SmoothMoveTo(new Vector3(rect.position.x, rect.position.y, z));
						}
					}
				}
			}
			sweepMesh.localRotation = Quaternion.AngleAxis(sweepAngle, Vector3.up);
		}
		else
		{
			ActivateRadar(enabled: false);
		}
		minY = 0f;
		maxY = 0f;
		foreach (Tile item2 in hashSet)
		{
			item2.Show(show: false);
		}
	}
}
