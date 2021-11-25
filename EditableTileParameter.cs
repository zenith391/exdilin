using System;
using Blocks;
using UnityEngine;

// Token: 0x0200014E RID: 334
public class EditableTileParameter
{
	// Token: 0x060014A7 RID: 5287 RVA: 0x00091126 File Offset: 0x0008F526
	public EditableTileParameter(int parameterIndex, bool useDoubleWidth = true, int subParameterCount = 1)
	{
		this.parameterIndex = parameterIndex;
		this.useDoubleWidth = useDoubleWidth;
		this.subParameterCount = subParameterCount;
	}

	// Token: 0x17000057 RID: 87
	// (get) Token: 0x060014A8 RID: 5288 RVA: 0x00091143 File Offset: 0x0008F543
	// (set) Token: 0x060014A9 RID: 5289 RVA: 0x0009114B File Offset: 0x0008F54B
	public int parameterIndex { get; private set; }

	// Token: 0x17000058 RID: 88
	// (get) Token: 0x060014AA RID: 5290 RVA: 0x00091154 File Offset: 0x0008F554
	// (set) Token: 0x060014AB RID: 5291 RVA: 0x0009115C File Offset: 0x0008F55C
	public object objectValue
	{
		get
		{
			return this._objectValue;
		}
		set
		{
			if (value != null)
			{
				this._objectValue = value;
			}
			else
			{
				BWLog.Warning("Trying to set objectValue to null in " + base.GetType().Name);
			}
		}
	}

	// Token: 0x17000059 RID: 89
	// (get) Token: 0x060014AC RID: 5292 RVA: 0x0009118A File Offset: 0x0008F58A
	// (set) Token: 0x060014AD RID: 5293 RVA: 0x00091192 File Offset: 0x0008F592
	public bool useDoubleWidth { get; set; }

	// Token: 0x1700005A RID: 90
	// (get) Token: 0x060014AE RID: 5294 RVA: 0x0009119B File Offset: 0x0008F59B
	// (set) Token: 0x060014AF RID: 5295 RVA: 0x000911A3 File Offset: 0x0008F5A3
	public int subParameterCount { get; set; }

	// Token: 0x1700005B RID: 91
	// (get) Token: 0x060014B0 RID: 5296 RVA: 0x000911AC File Offset: 0x0008F5AC
	// (set) Token: 0x060014B1 RID: 5297 RVA: 0x000911B4 File Offset: 0x0008F5B4
	public TileParameterSetting settings { get; set; }

	// Token: 0x060014B2 RID: 5298 RVA: 0x000911C0 File Offset: 0x0008F5C0
	public void WriteValueToTile(Tile tile)
	{
		if (this.objectValue != null)
		{
			tile.gaf.Args[this.parameterIndex] = this.objectValue;
		}
		else
		{
			BWLog.Warning("Trying to write null to GAF argument in " + base.GetType().Name);
		}
	}

	// Token: 0x060014B3 RID: 5299 RVA: 0x0009120F File Offset: 0x0008F60F
	public virtual string ValueAsString()
	{
		return this.objectValue.ToString();
	}

	// Token: 0x060014B4 RID: 5300 RVA: 0x0009121C File Offset: 0x0008F61C
	public virtual GameObject SetupUI(Tile tile)
	{
		this.ApplyTileParameterUI(tile);
		return null;
	}

	// Token: 0x060014B5 RID: 5301 RVA: 0x00091226 File Offset: 0x0008F626
	public virtual void ApplyTileParameterUI(Tile tile)
	{
		this.tile = tile;
	}

	// Token: 0x060014B6 RID: 5302 RVA: 0x0009122F File Offset: 0x0008F62F
	public virtual void CleanupUI()
	{
	}

	// Token: 0x060014B7 RID: 5303 RVA: 0x00091231 File Offset: 0x0008F631
	public virtual bool UIUpdate()
	{
		return false;
	}

	// Token: 0x060014B8 RID: 5304 RVA: 0x00091234 File Offset: 0x0008F634
	public virtual bool HasUIQuit()
	{
		return false;
	}

	// Token: 0x060014B9 RID: 5305 RVA: 0x00091238 File Offset: 0x0008F638
	protected Rect GetRightSideRect()
	{
		float num = 80f * NormalizedScreen.pixelScale;
		float num2 = (float)Blocksworld.marginTile * NormalizedScreen.pixelScale;
		float num3 = num + num2;
		Vector3 position = this.tile.tileObject.GetPosition();
		float num4 = 0.5f * num2;
		float scale = NormalizedScreen.scale;
		Rect result = new Rect((num3 + position.x + 1f) * scale, ((float)NormalizedScreen.height - position.y - num - num4 + 6f) * scale, num3 * scale, num3 * scale);
		return result;
	}

	// Token: 0x060014BA RID: 5306 RVA: 0x000912C2 File Offset: 0x0008F6C2
	protected virtual HudMeshStyle GetHudMeshStyle()
	{
		if (this.style == null)
		{
			this.style = HudMeshOnGUI.dataSource.intParamStyle;
		}
		return this.style;
	}

	// Token: 0x060014BB RID: 5307 RVA: 0x000912E8 File Offset: 0x0008F6E8
	protected void DisplayDescriptor()
	{
		if (!string.IsNullOrEmpty(this.settings.descriptorText))
		{
			if (EditableTileParameter.descriptorStyle == null)
			{
				EditableTileParameter.descriptorStyle = HudMeshOnGUI.dataSource.paramDescStyle;
			}
			HudMeshOnGUI.Label(ref this.descriptorLabel, this.GetRightSideRect(), this.GetDescriptorText(), EditableTileParameter.descriptorStyle, 0f);
		}
	}

	// Token: 0x060014BC RID: 5308 RVA: 0x00091344 File Offset: 0x0008F744
	protected virtual string GetDescriptorText()
	{
		return this.settings.descriptorText;
	}

	// Token: 0x060014BD RID: 5309 RVA: 0x00091354 File Offset: 0x0008F754
	public virtual void OnHudMesh()
	{
		if (this.tile == null)
		{
			BWLog.Info("Tile was null in " + base.GetType().Name);
			return;
		}
		if (this.useDoubleWidth && this.tile.IsShowing())
		{
			this.DisplayDescriptor();
			HudMeshOnGUI.Label(ref this.label, this.GetRightSideRect(), this.ValueAsString(), this.GetHudMeshStyle(), 0f);
		}
	}

	// Token: 0x060014BE RID: 5310 RVA: 0x000913CA File Offset: 0x0008F7CA
	public virtual void HelpSetParameterValueInTutorial(Block block, Tile thisTile, Tile goalTile)
	{
		if (Blocksworld.bw.tileParameterEditor.selectedTile != thisTile)
		{
			Tutorial.HelpToggleTile(block, thisTile);
			return;
		}
	}

	// Token: 0x0400104B RID: 4171
	private object _objectValue;

	// Token: 0x0400104F RID: 4175
	public Tile tile;

	// Token: 0x04001050 RID: 4176
	protected HudMeshStyle style;

	// Token: 0x04001051 RID: 4177
	protected static HudMeshStyle descriptorStyle;

	// Token: 0x04001052 RID: 4178
	protected HudMeshLabel label;

	// Token: 0x04001053 RID: 4179
	protected HudMeshLabel descriptorLabel;
}
