using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public abstract class BlockAbstractCounterUI : BlockAbstractUI
{
	protected bool isDefined;

	protected int previousValue;

	protected int currentValue;

	protected int extraValue;

	protected HudMeshLabel textLabel;

	protected HudMeshLabel textOutlineLabel;

	protected Color textColor = Color.white;

	protected GAF iconGaf;

	protected string starEnabledIconName = "Misc/Counter_Star_Enabled";

	protected string starDisabledIconName = "Misc/Counter_Star_Disabled";

	protected GAF defaultSingleGaf;

	protected string noTileTexture = string.Empty;

	protected int textColorMeshIndex;

	protected string textColorPaint;

	protected int backgroundColorMeshIndex;

	protected Color[] customBackgroundColors;

	protected string backgroundColorPaint;

	protected bool textureDirty = true;

	protected HudMeshStyle style;

	protected HudMeshStyle outlineStyle;

	protected int currentTileSize = 80;

	protected Dictionary<string, BlockIconOverride> blockIconOverrides = new Dictionary<string, BlockIconOverride>();

	protected Dictionary<string, TextureIconOverride> textureIconOverrides = new Dictionary<string, TextureIconOverride>();

	protected ObjectCounterMetaData counterMeta;

	private static GameObject tileInfoPrefab;

	public BlockAbstractCounterUI(List<List<Tile>> tiles, int index)
		: base(tiles, index)
	{
		defaultSingleGaf = new GAF("Block.TextureTo", "Texture Clock Face");
	}

	protected bool UseDefaultTile(string texture)
	{
		bool flag = iconGaf == null && texture == "Plain";
		if (!flag && iconGaf != null)
		{
			flag = iconGaf.Predicate == Block.predicateTextureTo && (string)iconGaf.Args[0] == "Plain";
		}
		return flag;
	}

	protected void DestroyTile(ref TileCustom counterTile)
	{
		if (counterTile != null)
		{
			counterTile.Destroy();
			counterTile = null;
		}
	}

	protected Rect GetLayoutRect()
	{
		float num = 260f;
		float num2 = 65f;
		float num3 = 0.5f;
		float num4 = 0f;
		int count = BlockGaugeUI.allGaugeBlocks.Count;
		int count2 = BlockCounterUI.allCounterBlocks.Count;
		if (this is BlockGaugeUI)
		{
			if (count == 1 && count2 == 0)
			{
				num3 = 0.5f;
			}
			else if (count == 1 && count2 >= 1)
			{
				num3 = 0f;
			}
			else if (count == 2)
			{
				num3 = index;
			}
		}
		else if (this is BlockCounterUI)
		{
			switch (count)
			{
			case 0:
				num3 = ((count2 != 1) ? ((float)index) : 0.5f);
				break;
			case 1:
				if (count2 == 1 || index == 0)
				{
					num3 = 1f;
					break;
				}
				num3 = 0.5f;
				num4 = 1f;
				break;
			case 2:
				num4 = 1f;
				num3 = index;
				break;
			}
		}
		else
		{
			BWLog.Info("No support yet for laying out UI blocks of type '" + BlockType() + "'");
		}
		float num5 = ((num3 != 0.5f) ? 20f : 0f);
		float num6 = 0.5f * ((float)NormalizedScreen.width - num * 2f - num5);
		float x = num6 + num3 * (num + num5) + 40f;
		float y = (float)NormalizedScreen.height - NormalizedScreen.scale * 100f - num2 * num4;
		return new Rect(x, y, num * NormalizedScreen.scale, num2 * NormalizedScreen.scale);
	}

	protected void UpdateTile(ref TileCustom counterTile)
	{
		if (!isDefined || Blocksworld.CurrentState != State.Play)
		{
			return;
		}
		if (counterTile == null || textureDirty)
		{
			GAF gAF = iconGaf;
			string texture = GetTexture();
			if (UseDefaultTile(texture))
			{
				gAF = defaultSingleGaf;
			}
			GAF gAF2 = ((gAF != null) ? gAF.Clone() : ((!(texture == noTileTexture)) ? new GAF("Block.TextureTo", texture, Vector3.zero) : null));
			if (gAF2 != null)
			{
				currentTileSize = GetTileSize(gAF2);
				counterTile = GetOrCreateTile(gAF2, customBackgroundColors);
				counterTile.Show(show: true);
			}
		}
		dirty = false;
		textureDirty = false;
	}

	protected TileCustom GetOrCreateTile(GAF newGaf, Color[] customBackgroundColors)
	{
		return new TileCustom(newGaf, ShowBackground(newGaf), customBackgroundColors);
	}

	public override void Play()
	{
		base.Play();
		currentValue = 0;
		extraValue = 0;
		previousValue = 0;
		isDefined = false;
		textureDirty = true;
		iconGaf = null;
		style = HudMeshOnGUI.dataSource.GetStyle("Counter");
		outlineStyle = HudMeshOnGUI.dataSource.GetStyle("Outline");
		if (tileInfoPrefab == null)
		{
			tileInfoPrefab = Resources.Load("UI Tile Info") as GameObject;
		}
		counterMeta = tileInfoPrefab.GetComponent<ObjectCounterMetaData>();
		blockIconOverrides.Clear();
		textureIconOverrides.Clear();
		BlockIconOverride[] array = counterMeta.blockIconOverrides;
		foreach (BlockIconOverride blockIconOverride in array)
		{
			blockIconOverrides[blockIconOverride.blockName] = blockIconOverride;
		}
		TextureIconOverride[] array2 = counterMeta.textureIconOverrides;
		foreach (TextureIconOverride textureIconOverride in array2)
		{
			textureIconOverrides[textureIconOverride.textureName] = textureIconOverride;
		}
		textColorMeshIndex = counterMeta.textColorMeshIndex;
		backgroundColorMeshIndex = counterMeta.backgroundColorMeshIndex;
	}

	public override void Stop(bool resetBlock)
	{
		base.Stop(resetBlock);
		iconGaf = null;
	}

	protected bool ShowBackground(GAF gaf)
	{
		if (gaf.Predicate == Block.predicateCreate)
		{
			if (blockIconOverrides.TryGetValue((string)gaf.Args[0], out var value))
			{
				return value.addBackground;
			}
			return false;
		}
		if (gaf.Predicate == Block.predicateTextureTo)
		{
			if (textureIconOverrides.TryGetValue((string)gaf.Args[0], out var value2))
			{
				return !value2.removeBackground;
			}
			return true;
		}
		return gaf.Predicate != Block.predicateUI;
	}

	public GAF GetCustomIconGAF()
	{
		return iconGaf;
	}

	public void SetCustomIconGAF(GAF gaf)
	{
		if (iconGaf == null || !iconGaf.Equals(gaf))
		{
			textureDirty = true;
		}
		iconGaf = gaf;
	}

	protected CommonIconOverride GetIconOverride(GAF gaf)
	{
		BlockIconOverride blockIconOverride = GetBlockIconOverride(gaf);
		if (blockIconOverride != null)
		{
			return blockIconOverride;
		}
		TextureIconOverride textureIconOverride = GetTextureIconOverride(gaf);
		if (textureIconOverride != null)
		{
			return textureIconOverride;
		}
		return null;
	}

	public int GetTileSize(GAF gaf)
	{
		int result = 80;
		CommonIconOverride iconOverride = GetIconOverride(gaf);
		if (iconOverride != null && iconOverride.tileSize > 0)
		{
			result = iconOverride.tileSize;
		}
		return result;
	}

	protected BlockIconOverride GetBlockIconOverride(GAF gaf)
	{
		if (gaf.Predicate == Block.predicateCreate && blockIconOverrides.TryGetValue((string)gaf.Args[0], out var value))
		{
			return value;
		}
		return null;
	}

	protected TextureIconOverride GetTextureIconOverride(GAF gaf)
	{
		if (gaf.Predicate == Block.predicateTextureTo && textureIconOverrides.TryGetValue((string)gaf.Args[0], out var value))
		{
			return value;
		}
		return null;
	}

	public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
	{
		if (meshIndex == 0 && Blocksworld.CurrentState == State.Play)
		{
			string texture2 = GetTexture();
			TileResultCode result = base.TextureTo(texture, normal, permanent, meshIndex, force);
			textureDirty = texture2 != GetTexture();
			dirty = dirty || textureDirty;
			return result;
		}
		return base.TextureTo(texture, normal, permanent, meshIndex, force);
	}

	public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
	{
		if (meshIndex == backgroundColorMeshIndex && paint != backgroundColorPaint)
		{
			textureDirty = true;
			backgroundColorPaint = paint;
			Color[] value;
			if (paint == GetDefaultPaint(backgroundColorMeshIndex))
			{
				customBackgroundColors = null;
			}
			else if (Blocksworld.colorDefinitions.TryGetValue(paint, out value))
			{
				customBackgroundColors = value;
			}
		}
		if (meshIndex == textColorMeshIndex && paint != textColorPaint)
		{
			textColorPaint = paint;
			Color[] value2;
			if (paint == GetDefaultPaint(backgroundColorMeshIndex))
			{
				textColor = Color.white;
			}
			else if (Blocksworld.colorDefinitions.TryGetValue(paint, out value2))
			{
				textColor = value2[0];
			}
		}
		return base.PaintTo(paint, permanent, meshIndex);
	}

	public bool ValueEquals(int value)
	{
		if (isDefined)
		{
			return value == currentValue + extraValue;
		}
		return false;
	}

	public TileResultCode ValueEquals(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		if (ValueEquals(intArg))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode ValueCondition(int value, int condition)
	{
		if (isDefined)
		{
			int num = currentValue + extraValue;
			bool flag = false;
			switch (condition)
			{
			case 2:
				flag = num != value;
				break;
			case 1:
				flag = num > value;
				break;
			case 0:
				flag = num < value;
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

	public TileResultCode ValueCondition(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (isDefined)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			int intArg2 = Util.GetIntArg(args, 1, 0);
			return ValueCondition(intArg, intArg2);
		}
		return TileResultCode.False;
	}

	public virtual void SetValue(int value)
	{
		int num = currentValue + extraValue;
		currentValue = value;
		extraValue = 0;
		isDefined = true;
		dirty = dirty || currentValue != num;
	}

	public virtual TileResultCode SetValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int value = ((args.Length != 0) ? ((int)args[0]) : 0);
		SetValue(value);
		return TileResultCode.True;
	}

	public virtual void IncrementValue(int inc)
	{
		if (isDefined)
		{
			currentValue += inc;
			dirty = true;
		}
	}

	public virtual TileResultCode IncrementValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int inc = ((args.Length == 0) ? 1 : ((int)args[0]));
		IncrementValue(inc);
		return TileResultCode.True;
	}

	public virtual void Randomize(int min, int max)
	{
		int num = currentValue + extraValue;
		extraValue = 0;
		currentValue = Random.Range(min, max + 1);
		dirty = dirty || currentValue != num;
		isDefined = true;
	}

	public virtual TileResultCode Randomize(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		int intArg2 = Util.GetIntArg(args, 1, 10);
		Randomize(intArg, intArg2);
		return TileResultCode.True;
	}
}
