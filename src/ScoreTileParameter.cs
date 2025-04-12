using System;
using UnityEngine;

// Token: 0x0200015A RID: 346
public class ScoreTileParameter : IntTileParameter
{
	// Token: 0x0600150A RID: 5386 RVA: 0x000932D8 File Offset: 0x000916D8
	public ScoreTileParameter(int parameterIndex = 0) : base(-10000000, 10000000, 1, 50f, parameterIndex, null, false, string.Empty, string.Empty)
	{
	}

	// Token: 0x0600150B RID: 5387 RVA: 0x00093308 File Offset: 0x00091708
	protected override HudMeshStyle GetHudMeshStyle()
	{
		int num = Mathf.Abs(base.currentValue);
		if (num > 9999)
		{
			this.style = HudMeshOnGUI.dataSource.intParamStyle;
		}
		else
		{
			this.style = HudMeshOnGUI.dataSource.timeParamStyle;
		}
		return this.style;
	}

	// Token: 0x0600150C RID: 5388 RVA: 0x00093358 File Offset: 0x00091758
	private int[] GetSteps()
	{
		if (this.steps == null)
		{
			int num = 10;
			this.steps = new int[num];
			int num2 = 1;
			for (int i = 0; i < this.steps.Length; i++)
			{
				this.steps[i] = num2;
				num2 *= 10;
			}
		}
		return this.steps;
	}

	// Token: 0x0600150D RID: 5389 RVA: 0x000933B0 File Offset: 0x000917B0
	protected override bool UpdateValue()
	{
		bool result = false;
		float f = this.screenPos.x - this.startPositionScreen.x;
		float num = Mathf.Sign(f);
		float num2 = Mathf.Abs(f);
		int[] array = this.GetSteps();
		float num3 = 0f;
		float num4 = 80f;
		int num5 = (num2 <= 0f) ? 1 : array[array.Length - 1];
		for (int i = 0; i < array.Length; i++)
		{
			float num6 = (float)i * num4;
			if (num2 <= num6)
			{
				break;
			}
			num3 = Mathf.Clamp(num2 - num6, 0f, num4);
			num5 = array[i];
		}
		int currentValue = base.currentValue;
		int num7 = Mathf.RoundToInt(Mathf.Abs(num3 / num4) * 10f);
		int num8 = this.startValue + Mathf.RoundToInt(num * (float)(((num5 != 1) ? num5 : 0) + Mathf.FloorToInt((float)(num7 * num5))));
		num8 = Mathf.Clamp(num8, this.minValue, this.maxValue);
		if (num8 != currentValue)
		{
			if (this.startValue != 0 && Mathf.Sign((float)num8) != Mathf.Sign((float)this.startValue))
			{
				num8 = 0;
			}
			base.currentValue = num8;
			result = true;
		}
		return result;
	}

	// Token: 0x0600150E RID: 5390 RVA: 0x00093500 File Offset: 0x00091900
	public override void OnHudMesh()
	{
		if (this.tile == null)
		{
			return;
		}
		if (base.useDoubleWidth && this.tile.IsShowing())
		{
			base.DisplayDescriptor();
			HudMeshStyle style = this.style;
			HudMeshStyle hudMeshStyle = this.GetHudMeshStyle();
			if (style != hudMeshStyle)
			{
				this.label = null;
			}
			HudMeshOnGUI.Label(ref this.label, base.GetRightSideRect(), this.ValueAsString(), hudMeshStyle, 0f);
		}
	}

	// Token: 0x0400108A RID: 4234
	private int[] steps;
}
