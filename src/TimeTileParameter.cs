using Blocks;
using UnityEngine;

public class TimeTileParameter : NumericHandleTileParameter
{
	public int index;

	public int[] timeComponents;

	private int[] steps;

	private int[] maxValues;

	protected int startValue;

	protected Tile timerArrowUp;

	protected Tile timerArrowDown;

	private Vector2? stringDim;

	public TimeTileParameter(int parameterIndex = 0, float sensitivity = 25f)
		: base(sensitivity, parameterIndex, 3, onlyShowPositive: false, string.Empty, string.Empty)
	{
		timeComponents = new int[3];
		maxValues = new int[3] { 99, 59, 99 };
		steps = new int[3] { 2, 1, 1 };
	}

	public override void HelpSetParameterValueInTutorial(Block block, Tile thisTile, Tile goalTile)
	{
		if (Blocksworld.bw.tileParameterEditor.selectedTile != thisTile || handle == null)
		{
			Tutorial.HelpToggleTile(block, thisTile);
			return;
		}
		int[] array = CalculateTimeComponents(thisTile.gaf);
		int[] array2 = CalculateTimeComponents(goalTile.gaf);
		bool flag = true;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != array2[i])
			{
				flag = false;
			}
		}
		if (!flag)
		{
			int num = steps[index];
			int num2 = array2[index] - array[index];
			if (num2 != 0)
			{
				float offset = (float)num2 / (GetSensitivity() * (float)num);
				HelpDragHandle(thisTile, offset);
				tutorialErrorOffset = offset;
			}
			else
			{
				Tutorial.HelpToggleTile(block, thisTile);
			}
		}
	}

	public static int[] CalculateTimeComponents(float secondsRaw, int[] result = null)
	{
		if (result == null)
		{
			result = new int[3];
		}
		float num = Mathf.Floor(secondsRaw);
		int num2 = Mathf.RoundToInt(100f * (secondsRaw - num));
		int num3 = Mathf.FloorToInt(num / 60f);
		int num4 = Mathf.RoundToInt(num - (float)num3 * 60f);
		result[0] = num2;
		result[1] = num4;
		result[2] = num3;
		return result;
	}

	public int[] CalculateTimeComponents(GAF gaf, int[] result = null)
	{
		float secondsRaw = (float)gaf.Args[base.parameterIndex];
		return CalculateTimeComponents(secondsRaw, result);
	}

	protected float GetSensitivity()
	{
		float num = sensitivity;
		if (index == 0)
		{
			num *= 1.5f;
		}
		return num;
	}

	protected override bool UpdateValue()
	{
		bool result = false;
		int num = steps[index];
		float num2 = GetSensitivity();
		int num3 = Mathf.RoundToInt(Mathf.Clamp((float)startValue + (screenPos.x - startPositionScreen.x) * num2 * (float)num, 0f, maxValues[index]));
		if (Mathf.Abs(num3 - timeComponents[index]) >= num)
		{
			timeComponents[index] = Mathf.RoundToInt((float)timeComponents[index] + (float)num * Mathf.Sign(num3 - timeComponents[index]));
			result = true;
			base.objectValue = (float)timeComponents[0] * 0.01f + (float)timeComponents[1] + (float)timeComponents[2] * 60f;
		}
		return result;
	}

	protected override void InitializeStartValue()
	{
		startValue = timeComponents[index];
	}

	protected override void SetValueAndStep(Tile tile)
	{
		CalculateTimeComponents(tile.gaf, timeComponents);
		index = tile.subParameterIndex;
		float num = (float)tile.gaf.Args[base.parameterIndex];
		base.objectValue = num;
	}

	protected override bool ValueAtMaxOrMore()
	{
		return timeComponents[index] >= maxValues[index];
	}

	protected override bool ValueAtMinOrLess()
	{
		return timeComponents[index] <= 0;
	}

	public override string ValueAsString()
	{
		string text = string.Empty;
		for (int num = 2; num >= 0; num--)
		{
			text = ((index != num) ? (text + "<color=#ccccccff>") : (text + "<color=#ffffffff>"));
			text += timeComponents[num].ToString("D2");
			text += "</color>";
			if (num > 0)
			{
				text += ":";
			}
		}
		return text;
	}

	public override void OnHudMesh()
	{
		if (tile == null)
		{
			BWLog.Info("Tile was null in " + GetType().Name);
		}
		else if (base.useDoubleWidth && tile.IsShowing())
		{
			DisplayDescriptor();
			HudMeshStyle hudMeshStyle = GetHudMeshStyle();
			string text = ValueAsString();
			Vector2? vector = stringDim;
			if (!vector.HasValue)
			{
				stringDim = HudMeshUtils.CalcSize(hudMeshStyle, text);
			}
			Rect rightSideRect = GetRightSideRect();
			HudMeshOnGUI.Label(ref label, rightSideRect, text, hudMeshStyle);
			if (rightSide != null && !(rightSide.tileObject == null))
			{
				Vector3 position = rightSide.tileObject.GetPosition();
				float scale = NormalizedScreen.scale;
				Vector2 value = stringDim.Value;
				float num = (0f - (float)index + 1f) * (value.x * 0.35f) / scale;
				num *= NormalizedScreen.pixelScale;
				float num2 = value.y * 0.9f / scale;
				num2 *= NormalizedScreen.pixelScale;
				timerArrowUp.MoveTo(position.x + num, position.y - num2, 1f);
				timerArrowDown.MoveTo(position.x + num, position.y + num2, 1f);
			}
		}
	}

	public override void CleanupUI()
	{
		base.CleanupUI();
		timerArrowUp.Show(show: false);
		timerArrowDown.Show(show: false);
	}

	public override GameObject SetupUI(Tile tile)
	{
		GameObject result = base.SetupUI(tile);
		if (timerArrowUp != null)
		{
			timerArrowUp.Show(show: false);
		}
		if (timerArrowDown != null)
		{
			timerArrowDown.Show(show: false);
		}
		timerArrowUp = new Tile(Blocksworld.tilePool.GetTileObjectForIcon("Misc/Timer_Arrow_Up", enabled: true));
		timerArrowDown = new Tile(Blocksworld.tilePool.GetTileObjectForIcon("Misc/Timer_Arrow_Down", enabled: true));
		timerArrowUp.Show(show: true);
		timerArrowDown.Show(show: true);
		return result;
	}

	protected override HudMeshStyle GetHudMeshStyle()
	{
		if (style == null)
		{
			style = HudMeshOnGUI.dataSource.timeParamStyle;
		}
		return style;
	}

	protected override string GetDescriptorText()
	{
		return index switch
		{
			0 => "Sec/100", 
			1 => "Seconds", 
			2 => "Minutes", 
			_ => "Duration", 
		};
	}
}
