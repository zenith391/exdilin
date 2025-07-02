using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockGaugeUI : BlockAbstractLimitedCounterUI
{
	private Rect rect;

	private Rect withinRect;

	protected HudMeshLabel withinLabel;

	protected HudMeshLabel withinOutlineLabel;

	private TileCustom gaugeTile;

	private static GameObject gaugePrefab;

	private GameObject gauge;

	private Material gaugeMaterial;

	private const int GAUGE_WIDTH = 200;

	private const int GAUGE_HEIGHT = 32;

	protected bool showFractionUI = true;

	public static Dictionary<int, BlockGaugeUI> allGaugeBlocks = new Dictionary<int, BlockGaugeUI>();

	public BlockGaugeUI(List<List<Tile>> tiles, int index)
		: base(tiles, index)
	{
		defaultSingleGaf = new GAF("Block.Create", BlockType());
		if (gaugePrefab == null)
		{
			gaugePrefab = Resources.Load("GUI/Gauge " + ((!Blocksworld.hd) ? "SD" : "HD")) as GameObject;
		}
		gauge = UnityEngine.Object.Instantiate(gaugePrefab);
		gauge.SetActive(value: false);
		gaugeMaterial = gauge.GetComponent<MeshRenderer>().material;
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockGaugeUI>("GaugeUI.SetMaxValue", null, (Block b) => ((BlockAbstractLimitedCounterUI)b).SetMaxValue, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockGaugeUI>("GaugeUI.Flash", null, (Block b) => ((BlockAbstractUI)b).Flash, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockGaugeUI>("GaugeUI.SetText", null, (Block b) => ((BlockAbstractCounterUI)b).SetText, new Type[1] { typeof(string) });
		PredicateRegistry.Add<BlockGaugeUI>("GaugeUI.ShowFractionUI", null, (Block b) => ((BlockGaugeUI)b).ShowFractionUI, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockGaugeUI>("GaugeUI.ShowUI", null, (Block b) => ((BlockAbstractUI)b).ShowUI, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockGaugeUI>("GaugeUI.ShowPhysical", null, (Block b) => ((BlockAbstractUI)b).ShowPhysical, new Type[1] { typeof(int) });
	}

	public override void Play()
	{
		base.Play();
		maxValue = 100;
		minValue = 0;
		showFractionUI = true;
		allGaugeBlocks[index] = this;
		gauge.SetActive(value: true);
		gauge.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
		gauge.transform.localScale = new Vector3(200f, 1f, 32f);
		UpdateGaugeMaterial(GetPaint());
	}

	public override void Play2()
	{
		base.Play2();
		rect = GetLayoutRect();
		Vector3 position = new Vector3(rect.center.x, (float)NormalizedScreen.height - rect.center.y - 20f, 2f);
		gauge.transform.position = position;
		withinRect = new Rect(position.x - 100f, (float)NormalizedScreen.height - (position.y + 16f), 200f, 32f);
	}

	public override void Stop(bool resetBlock)
	{
		base.Stop(resetBlock);
		allGaugeBlocks.Clear();
		DestroyTile(ref gaugeTile);
		gauge.SetActive(value: false);
	}

	public TileResultCode ShowFractionUI(ScriptRowExecutionInfo eInfo, object[] args)
	{
		showFractionUI = Util.GetIntBooleanArg(args, 0, defaultValue: true);
		return TileResultCode.True;
	}

	public override void Destroy()
	{
		base.Destroy();
		if (gauge != null)
		{
			UnityEngine.Object.Destroy(gauge);
			gauge = null;
		}
	}

	public override void Update()
	{
		base.Update();
		if (Blocksworld.CurrentState == State.Play)
		{
			if (dirty)
			{
				gaugeMaterial.SetFloat("_Fill", 100f * ((float)currentValue / (float)maxValue));
			}
			UpdateTile(ref gaugeTile);
			if (gaugeTile != null)
			{
				Vector2 vector = new Vector2(rect.x - 40f, (float)NormalizedScreen.height - rect.y - 80f);
				gaugeTile.MoveTo(vector.x, vector.y, 2f);
			}
		}
	}

	public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex)
	{
		TileResultCode result = base.PaintTo(paint, permanent, meshIndex);
		if (meshIndex == 0)
		{
			UpdateGaugeMaterial(paint);
		}
		return result;
	}

	private void UpdateGaugeMaterial(string paint)
	{
		if (gaugeMaterial != null && Blocksworld.colorDefinitions.TryGetValue(paint, out var value))
		{
			gaugeMaterial.color = value[0];
		}
	}

	public override void OnHudMesh()
	{
		base.OnHudMesh();
		if (base.uiVisible && isDefined && Blocksworld.CurrentState == State.Play)
		{
			if (!string.IsNullOrEmpty(base.text))
			{
				string text = base.text;
				HudMeshOnGUI.Label(ref textLabel, rect, text, style);
				HudMeshOnGUI.Label(ref textOutlineLabel, rect, text, outlineStyle);
			}
			if (showFractionUI)
			{
				string text2 = currentValue + " / " + maxValue;
				HudMeshOnGUI.Label(ref withinLabel, withinRect, text2, style);
				HudMeshOnGUI.Label(ref withinOutlineLabel, withinRect, text2, outlineStyle);
			}
		}
	}

	protected override void SetUIVisible(bool v)
	{
		base.SetUIVisible(v);
		if (dirty && gaugeTile != null)
		{
			gauge.SetActive(v);
			gaugeTile.Show(v);
		}
	}
}
