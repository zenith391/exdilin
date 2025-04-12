using System;
using Blocks;
using UnityEngine;

// Token: 0x0200015D RID: 349
public class TimeTileParameter : NumericHandleTileParameter
{
	// Token: 0x06001521 RID: 5409 RVA: 0x00093AC0 File Offset: 0x00091EC0
	public TimeTileParameter(int parameterIndex = 0, float sensitivity = 25f) : base(sensitivity, parameterIndex, 3, false, string.Empty, string.Empty)
	{
		this.timeComponents = new int[3];
		this.maxValues = new int[]
		{
			99,
			59,
			99
		};
		this.steps = new int[]
		{
			2,
			1,
			1
		};
	}

	// Token: 0x06001522 RID: 5410 RVA: 0x00093B1C File Offset: 0x00091F1C
	public override void HelpSetParameterValueInTutorial(Block block, Tile thisTile, Tile goalTile)
	{
		if (Blocksworld.bw.tileParameterEditor.selectedTile != thisTile || this.handle == null)
		{
			Tutorial.HelpToggleTile(block, thisTile);
			return;
		}
		int[] array = this.CalculateTimeComponents(thisTile.gaf, null);
		int[] array2 = this.CalculateTimeComponents(goalTile.gaf, null);
		bool flag = true;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != array2[i])
			{
				flag = false;
			}
		}
		if (flag)
		{
			return;
		}
		int num = this.steps[this.index];
		int num2 = array2[this.index] - array[this.index];
		if (num2 != 0)
		{
			float num3 = (float)num2 / (this.GetSensitivity() * (float)num);
			base.HelpDragHandle(thisTile, num3);
			this.tutorialErrorOffset = num3;
			return;
		}
		Tutorial.HelpToggleTile(block, thisTile);
	}

	// Token: 0x06001523 RID: 5411 RVA: 0x00093BEC File Offset: 0x00091FEC
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

	// Token: 0x06001524 RID: 5412 RVA: 0x00093C48 File Offset: 0x00092048
	public int[] CalculateTimeComponents(GAF gaf, int[] result = null)
	{
		float secondsRaw = (float)gaf.Args[base.parameterIndex];
		return TimeTileParameter.CalculateTimeComponents(secondsRaw, result);
	}

	// Token: 0x06001525 RID: 5413 RVA: 0x00093C70 File Offset: 0x00092070
	protected float GetSensitivity()
	{
		float num = this.sensitivity;
		if (this.index == 0)
		{
			num *= 1.5f;
		}
		return num;
	}

	// Token: 0x06001526 RID: 5414 RVA: 0x00093C98 File Offset: 0x00092098
	protected override bool UpdateValue()
	{
		bool result = false;
		int num = this.steps[this.index];
		float sensitivity = this.GetSensitivity();
		int num2 = Mathf.RoundToInt(Mathf.Clamp((float)this.startValue + (this.screenPos.x - this.startPositionScreen.x) * sensitivity * (float)num, 0f, (float)this.maxValues[this.index]));
		if (Mathf.Abs(num2 - this.timeComponents[this.index]) >= num)
		{
			this.timeComponents[this.index] = Mathf.RoundToInt((float)this.timeComponents[this.index] + (float)num * Mathf.Sign((float)(num2 - this.timeComponents[this.index])));
			result = true;
			base.objectValue = (float)this.timeComponents[0] * 0.01f + (float)this.timeComponents[1] + (float)this.timeComponents[2] * 60f;
		}
		return result;
	}

	// Token: 0x06001527 RID: 5415 RVA: 0x00093D88 File Offset: 0x00092188
	protected override void InitializeStartValue()
	{
		this.startValue = this.timeComponents[this.index];
	}

	// Token: 0x06001528 RID: 5416 RVA: 0x00093DA0 File Offset: 0x000921A0
	protected override void SetValueAndStep(Tile tile)
	{
		this.CalculateTimeComponents(tile.gaf, this.timeComponents);
		this.index = tile.subParameterIndex;
		float num = (float)tile.gaf.Args[base.parameterIndex];
		base.objectValue = num;
	}

	// Token: 0x06001529 RID: 5417 RVA: 0x00093DF0 File Offset: 0x000921F0
	protected override bool ValueAtMaxOrMore()
	{
		return this.timeComponents[this.index] >= this.maxValues[this.index];
	}

	// Token: 0x0600152A RID: 5418 RVA: 0x00093E11 File Offset: 0x00092211
	protected override bool ValueAtMinOrLess()
	{
		return this.timeComponents[this.index] <= 0;
	}

	// Token: 0x0600152B RID: 5419 RVA: 0x00093E28 File Offset: 0x00092228
	public override string ValueAsString()
	{
		string text = string.Empty;
		for (int i = 2; i >= 0; i--)
		{
			if (this.index == i)
			{
				text += "<color=#ffffffff>";
			}
			else
			{
				text += "<color=#ccccccff>";
			}
			text += this.timeComponents[i].ToString("D2");
			text += "</color>";
			if (i > 0)
			{
				text += ":";
			}
		}
		return text;
	}

	// Token: 0x0600152C RID: 5420 RVA: 0x00093EB4 File Offset: 0x000922B4
	public override void OnHudMesh()
	{
		if (this.tile == null)
		{
			BWLog.Info("Tile was null in " + base.GetType().Name);
			return;
		}
		if (base.useDoubleWidth && this.tile.IsShowing())
		{
			base.DisplayDescriptor();
			HudMeshStyle hudMeshStyle = this.GetHudMeshStyle();
			string text = this.ValueAsString();
			Vector2? vector = this.stringDim;
			if (vector == null)
			{
				this.stringDim = new Vector2?(HudMeshUtils.CalcSize(hudMeshStyle, text));
			}
			Rect rightSideRect = base.GetRightSideRect();
			HudMeshOnGUI.Label(ref this.label, rightSideRect, text, hudMeshStyle, 0f);
			if (this.rightSide == null)
			{
				return;
			}
			if (this.rightSide.tileObject == null)
			{
				return;
			}
			Vector3 position = this.rightSide.tileObject.GetPosition();
			float scale = NormalizedScreen.scale;
			Vector2 value = this.stringDim.Value;
			float num = (float)(-(float)this.index + 1) * (value.x * 0.35f) / scale;
			num *= NormalizedScreen.pixelScale;
			float num2 = value.y * 0.9f / scale;
			num2 *= NormalizedScreen.pixelScale;
			this.timerArrowUp.MoveTo(position.x + num, position.y - num2, 1f);
			this.timerArrowDown.MoveTo(position.x + num, position.y + num2, 1f);
		}
	}

	// Token: 0x0600152D RID: 5421 RVA: 0x0009402C File Offset: 0x0009242C
	public override void CleanupUI()
	{
		base.CleanupUI();
		this.timerArrowUp.Show(false);
		this.timerArrowDown.Show(false);
	}

	// Token: 0x0600152E RID: 5422 RVA: 0x0009404C File Offset: 0x0009244C
	public override GameObject SetupUI(Tile tile)
	{
		GameObject result = base.SetupUI(tile);
		if (this.timerArrowUp != null)
		{
			this.timerArrowUp.Show(false);
		}
		if (this.timerArrowDown != null)
		{
			this.timerArrowDown.Show(false);
		}
		this.timerArrowUp = new Tile(Blocksworld.tilePool.GetTileObjectForIcon("Misc/Timer_Arrow_Up", true));
		this.timerArrowDown = new Tile(Blocksworld.tilePool.GetTileObjectForIcon("Misc/Timer_Arrow_Down", true));
		this.timerArrowUp.Show(true);
		this.timerArrowDown.Show(true);
		return result;
	}

	// Token: 0x0600152F RID: 5423 RVA: 0x000940DE File Offset: 0x000924DE
	protected override HudMeshStyle GetHudMeshStyle()
	{
		if (this.style == null)
		{
			this.style = HudMeshOnGUI.dataSource.timeParamStyle;
		}
		return this.style;
	}

	// Token: 0x06001530 RID: 5424 RVA: 0x00094104 File Offset: 0x00092504
	protected override string GetDescriptorText()
	{
		int num = this.index;
		if (num == 0)
		{
			return "Sec/100";
		}
		if (num == 1)
		{
			return "Seconds";
		}
		if (num != 2)
		{
			return "Duration";
		}
		return "Minutes";
	}

	// Token: 0x04001097 RID: 4247
	public int index;

	// Token: 0x04001098 RID: 4248
	public int[] timeComponents;

	// Token: 0x04001099 RID: 4249
	private int[] steps;

	// Token: 0x0400109A RID: 4250
	private int[] maxValues;

	// Token: 0x0400109B RID: 4251
	protected int startValue;

	// Token: 0x0400109C RID: 4252
	protected Tile timerArrowUp;

	// Token: 0x0400109D RID: 4253
	protected Tile timerArrowDown;

	// Token: 0x0400109E RID: 4254
	private Vector2? stringDim;
}
