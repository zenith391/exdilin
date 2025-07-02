using Blocks;
using UnityEngine;

public class EditableTileParameter
{
	private object _objectValue;

	public Tile tile;

	protected HudMeshStyle style;

	protected static HudMeshStyle descriptorStyle;

	protected HudMeshLabel label;

	protected HudMeshLabel descriptorLabel;

	public int parameterIndex { get; private set; }

	public object objectValue
	{
		get
		{
			return _objectValue;
		}
		set
		{
			if (value != null)
			{
				_objectValue = value;
			}
			else
			{
				BWLog.Warning("Trying to set objectValue to null in " + GetType().Name);
			}
		}
	}

	public bool useDoubleWidth { get; set; }

	public int subParameterCount { get; set; }

	public TileParameterSetting settings { get; set; }

	public EditableTileParameter(int parameterIndex, bool useDoubleWidth = true, int subParameterCount = 1)
	{
		this.parameterIndex = parameterIndex;
		this.useDoubleWidth = useDoubleWidth;
		this.subParameterCount = subParameterCount;
	}

	public void WriteValueToTile(Tile tile)
	{
		if (objectValue != null)
		{
			tile.gaf.Args[parameterIndex] = objectValue;
		}
		else
		{
			BWLog.Warning("Trying to write null to GAF argument in " + GetType().Name);
		}
	}

	public virtual string ValueAsString()
	{
		return objectValue.ToString();
	}

	public virtual GameObject SetupUI(Tile tile)
	{
		ApplyTileParameterUI(tile);
		return null;
	}

	public virtual void ApplyTileParameterUI(Tile tile)
	{
		this.tile = tile;
	}

	public virtual void CleanupUI()
	{
	}

	public virtual bool UIUpdate()
	{
		return false;
	}

	public virtual bool HasUIQuit()
	{
		return false;
	}

	protected Rect GetRightSideRect()
	{
		float num = 80f * NormalizedScreen.pixelScale;
		float num2 = (float)Blocksworld.marginTile * NormalizedScreen.pixelScale;
		float num3 = num + num2;
		Vector3 position = tile.tileObject.GetPosition();
		float num4 = 0.5f * num2;
		float scale = NormalizedScreen.scale;
		return new Rect((num3 + position.x + 1f) * scale, ((float)NormalizedScreen.height - position.y - num - num4 + 6f) * scale, num3 * scale, num3 * scale);
	}

	protected virtual HudMeshStyle GetHudMeshStyle()
	{
		if (style == null)
		{
			style = HudMeshOnGUI.dataSource.intParamStyle;
		}
		return style;
	}

	protected void DisplayDescriptor()
	{
		if (!string.IsNullOrEmpty(settings.descriptorText))
		{
			if (descriptorStyle == null)
			{
				descriptorStyle = HudMeshOnGUI.dataSource.paramDescStyle;
			}
			HudMeshOnGUI.Label(ref descriptorLabel, GetRightSideRect(), GetDescriptorText(), descriptorStyle);
		}
	}

	protected virtual string GetDescriptorText()
	{
		return settings.descriptorText;
	}

	public virtual void OnHudMesh()
	{
		if (tile == null)
		{
			BWLog.Info("Tile was null in " + GetType().Name);
		}
		else if (useDoubleWidth && tile.IsShowing())
		{
			DisplayDescriptor();
			HudMeshOnGUI.Label(ref label, GetRightSideRect(), ValueAsString(), GetHudMeshStyle());
		}
	}

	public virtual void HelpSetParameterValueInTutorial(Block block, Tile thisTile, Tile goalTile)
	{
		if (Blocksworld.bw.tileParameterEditor.selectedTile != thisTile)
		{
			Tutorial.HelpToggleTile(block, thisTile);
		}
	}
}
