using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockCounterUI : BlockAbstractLimitedCounterUI
{
	private class AnimatedBlockScore
	{
		public int animationType = -1;

		public int positiveScoreMultiplier = 1;

		public int negativeScoreMultiplier = 1;
	}

	private class AnimatedScore
	{
		private int score;

		private AnimatedBlockScore blockScore;

		private HudMeshLabel label;

		private HudMeshLabel outlineLabel;

		public bool canDelay;

		private float duration = 0.5f;

		private float startTime;

		private Vector2 position;

		private Rect scoreRect = new Rect(0f, 0f, 200f, 50f);

		private AnimationCurve animCurveX;

		private AnimationCurve animCurveY;

		private BlockCounterUI counter;

		private static float[] animTypeDurations = new float[2] { 0.5f, 0.3f };

		public AnimatedScore(BlockCounterUI counter, Vector2 startPos, Vector2 targetPos, int score, AnimatedBlockScore blockScore)
		{
			this.counter = counter;
			this.score = score;
			startTime = Time.time;
			this.blockScore = blockScore;
			duration = animTypeDurations[blockScore.animationType % animTypeDurations.Length];
			if (blockScore.animationType == 0)
			{
				animCurveX = new AnimationCurve(new Keyframe(0f, startPos.x, 0f, 0f), new Keyframe(0.2f, startPos.x, 0f, 0f), new Keyframe(0.25f, startPos.x, 0f, 1f), new Keyframe(1f, targetPos.x, 1f, 1f));
				animCurveY = new AnimationCurve(new Keyframe(0f, startPos.y, 0f, 0f), new Keyframe(0.2f, startPos.y - 20f, 0f, 0f), new Keyframe(0.25f, startPos.y - 25f, 0f, 1f), new Keyframe(1f, targetPos.y, 1f, 1f));
			}
			else
			{
				animCurveX = new AnimationCurve(new Keyframe(0f, startPos.x, 0f, 0f), new Keyframe(0.8f, startPos.x, 0f, 0f), new Keyframe(1f, startPos.x, 0f, 1f));
				animCurveY = new AnimationCurve(new Keyframe(0f, startPos.y, 0f, 0f), new Keyframe(0.8f, startPos.y - 20f, 0f, 0f), new Keyframe(1f, startPos.y - 25f, 0f, 1f));
			}
		}

		public int GetDelayedScore()
		{
			if (blockScore.animationType == 0 && canDelay)
			{
				return score * ((score <= 0) ? (blockScore.negativeScoreMultiplier * counter.negativeScoreMultiplier) : (blockScore.positiveScoreMultiplier * counter.positiveScoreMultiplier));
			}
			return 0;
		}

		public bool Update(int displayType, HudMeshStyle style, HudMeshStyle outlineStyle)
		{
			float num = (Time.time - startTime) / duration;
			position = new Vector2(animCurveX.Evaluate(num), animCurveY.Evaluate(num));
			scoreRect.center = position;
			string text = GetCounterString(betweenStrings[displayType], score, addPositiveSign: true);
			if (score > 0 && blockScore.positiveScoreMultiplier * counter.positiveScoreMultiplier > 1)
			{
				text = text + " X" + blockScore.positiveScoreMultiplier * counter.positiveScoreMultiplier;
			}
			else if (score < 0 && blockScore.negativeScoreMultiplier * counter.negativeScoreMultiplier > 1)
			{
				text = text + " X" + blockScore.negativeScoreMultiplier * counter.negativeScoreMultiplier;
			}
			HudMeshOnGUI.Label(ref label, scoreRect, text, style);
			HudMeshOnGUI.Label(ref outlineLabel, scoreRect, text, outlineStyle);
			return num >= 1f;
		}
	}

	private Rect rect;

	private Rect scoreRect;

	public static Dictionary<int, BlockCounterUI> allCounterBlocks = new Dictionary<int, BlockCounterUI>();

	private TileCustom counterTile;

	private int positiveScoreMultiplier = 1;

	private int negativeScoreMultiplier = 1;

	public const int MAX_PARAMETER_VALUE = 10000000;

	public const int DEFAULT_COUNTER_MAX_VALUE = 1000000000;

	private const int DEFAULT_DISPLAY_TYPE = 1;

	private int displayType = 1;

	private bool alignScoreLeft;

	protected HudMeshLabel textAboveLabel;

	protected HudMeshLabel textAboveOutlineLabel;

	private Dictionary<Block, AnimatedBlockScore> animatedBlocks = new Dictionary<Block, AnimatedBlockScore>();

	private List<AnimatedScore> animatedScores = new List<AnimatedScore>();

	public static string[] betweenStrings = new string[4]
	{
		string.Empty,
		",",
		".",
		" "
	};

	private Dictionary<string, Vector2> textDims = new Dictionary<string, Vector2>();

	public BlockCounterUI(List<List<Tile>> tiles, int index)
		: base(tiles, index)
	{
		defaultSingleGaf = new GAF("Block.Create", BlockType());
		maxValue = 1000000000;
		minValue = -1000000000;
		displayType = 1;
		flashDurations = new List<float[]>
		{
			new float[2] { 1f, -0.1f },
			new float[11]
			{
				0.1f, -0.1f, 0.1f, -0.1f, 0.1f, -0.1f, 0.1f, -0.1f, 0.1f, -0.1f,
				0.5f
			}
		};
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockCounterUI>("CounterUI.SetDisplayType", null, (Block b) => ((BlockCounterUI)b).SetDisplayType, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockCounterUI>("CounterUI.Flash", null, (Block b) => ((BlockAbstractUI)b).Flash, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockCounterUI>("CounterUI.SetMaxValue", null, (Block b) => ((BlockAbstractLimitedCounterUI)b).SetMaxValue, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockCounterUI>("CounterUI.SetMinValue", null, (Block b) => ((BlockAbstractLimitedCounterUI)b).SetMinValue, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockCounterUI>("CounterUI.SetText", null, (Block b) => ((BlockAbstractUI)b).SetText, new Type[1] { typeof(string) });
		PredicateRegistry.Add<BlockCounterUI>("CounterUI.ShowUI", null, (Block b) => ((BlockAbstractUI)b).ShowUI, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockCounterUI>("CounterUI.ShowPhysical", null, (Block b) => ((BlockAbstractUI)b).ShowPhysical, new Type[1] { typeof(int) });
	}

	public override void Play()
	{
		base.Play();
		maxValue = 1000000000;
		minValue = -1000000000;
		allCounterBlocks[index] = this;
		displayType = 1;
		animatedScores.Clear();
		animatedBlocks.Clear();
	}

	public override void Play2()
	{
		base.Play2();
		rect = GetLayoutRect();
		scoreRect = new Rect(rect);
		scoreRect.center = rect.center + Vector2.up * 30f;
	}

	public override void Stop(bool resetBlock)
	{
		base.Stop(resetBlock);
		allCounterBlocks.Clear();
		DestroyTile(ref counterTile);
		animatedScores.Clear();
		animatedBlocks.Clear();
	}

	public override void Update()
	{
		base.Update();
		if (Blocksworld.CurrentState == State.Play)
		{
			UpdateTile(ref counterTile);
			if (counterTile != null)
			{
				Vector2 vector = new Vector2(rect.x - 40f, (float)NormalizedScreen.height - rect.y - 80f);
				counterTile.MoveTo(vector.x, vector.y, 2f);
			}
		}
	}

	public static string GetCounterString(string b, int value, bool addPositiveSign = false)
	{
		int num = Mathf.Abs(value);
		string text = string.Empty;
		do
		{
			int num2 = num % 1000;
			num = Mathf.FloorToInt(num / 1000);
			text = ((num <= 0) ? num2.ToString() : num2.ToString("D3")) + ((text.Length != 0) ? b : string.Empty) + text;
		}
		while (num > 0);
		return ((value < 0) ? "-" : ((!addPositiveSign) ? string.Empty : "+")) + text;
	}

	public override void OnHudMesh()
	{
		base.OnHudMesh();
		if (!base.uiVisible || Blocksworld.CurrentState != State.Play)
		{
			return;
		}
		if (isDefined)
		{
			int num = 0;
			for (int num2 = animatedScores.Count - 1; num2 >= 0; num2--)
			{
				AnimatedScore animatedScore = animatedScores[num2];
				if (animatedScore.Update(displayType, style, outlineStyle))
				{
					animatedScores.RemoveAt(num2);
				}
				else
				{
					num += animatedScore.GetDelayedScore();
				}
			}
			int value = currentValue - num;
			string txt = ((displayType != 0 && displayType >= 0 && displayType < betweenStrings.Length) ? GetCounterString(betweenStrings[displayType], value) : value.ToString());
			Vector2 textDimension = GetTextDimension(txt);
			Rect rect = new Rect(scoreRect);
			if (alignScoreLeft)
			{
				Vector2 center = scoreRect.center;
				float x = center.x - 0.5f * (rect.width - textDimension.x) + 40f;
				center.x = x;
				rect.center = center;
			}
			HudMeshOnGUI.Label(ref textLabel, rect, txt, textColor, style);
			HudMeshOnGUI.Label(ref textOutlineLabel, rect, txt, outlineStyle);
		}
		if (text.Length > 0)
		{
			HudMeshOnGUI.Label(ref textAboveLabel, this.rect, text, textColor, style);
			HudMeshOnGUI.Label(ref textAboveOutlineLabel, this.rect, text, outlineStyle);
		}
	}

	private Vector2 GetTextDimension(string txt)
	{
		if (textDims.Count > 100)
		{
			textDims.Clear();
		}
		if (!textDims.TryGetValue(txt, out var value))
		{
			value = HudMeshUtils.CalcSize(style, txt);
			textDims[txt] = value;
		}
		return value;
	}

	public TileResultCode SetDisplayType(ScriptRowExecutionInfo eInfo, object[] args)
	{
		displayType = Util.GetIntArg(args, 0, 0);
		return TileResultCode.True;
	}

	protected override int AdjustCurrentLongValue(long currentLongValue)
	{
		if (currentLongValue < minValue)
		{
			currentLongValue = minValue;
		}
		else if (currentLongValue > maxValue)
		{
			currentLongValue = maxValue;
		}
		return (int)currentLongValue;
	}

	public void BlockIncrementValue(Block b, int inc)
	{
		AnimatedScore animatedScore = null;
		if (animatedBlocks.TryGetValue(b, out var value))
		{
			if (value.animationType != -1)
			{
				Vector3 position = b.go.transform.position;
				float num = 10f * Mathf.Log10(Mathf.Max(1, Mathf.Abs(inc)));
				Vector3 vector = Util.WorldToScreenPoint(position, z: true);
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
				animatedScore = new AnimatedScore(this, startPos, rect.center, inc, value);
				animatedScores.Add(animatedScore);
			}
			inc *= ((inc <= 0) ? (value.negativeScoreMultiplier * negativeScoreMultiplier) : (value.positiveScoreMultiplier * positiveScoreMultiplier));
		}
		IncrementValue(inc);
		if (animatedScore != null && (currentValue == maxValue || currentValue == minValue))
		{
			animatedScore.canDelay = false;
		}
	}

	protected override void SetUIVisible(bool v)
	{
		base.SetUIVisible(v);
		if (dirty && counterTile != null)
		{
			counterTile.Show(v);
		}
	}

	public void SetScoreAnimatedBlock(Block b, int animationType)
	{
		if (!animatedBlocks.TryGetValue(b, out var value))
		{
			value = new AnimatedBlockScore();
			animatedBlocks[b] = value;
		}
		value.animationType = animationType;
	}

	public void SetScoreMultiplierBlock(Block b, int multiplier, bool positiveOnly = true)
	{
		if (!animatedBlocks.TryGetValue(b, out var value))
		{
			value = new AnimatedBlockScore();
			animatedBlocks[b] = value;
		}
		value.negativeScoreMultiplier = (positiveOnly ? 1 : multiplier);
		value.positiveScoreMultiplier = multiplier;
	}

	public void SetGlobalScoreMultiplier(int multiplier, bool positiveOnly = true)
	{
		negativeScoreMultiplier = (positiveOnly ? 1 : multiplier);
		positiveScoreMultiplier = multiplier;
	}
}
