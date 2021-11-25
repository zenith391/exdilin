using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000231 RID: 561
public class TileParameterEditor : MonoBehaviour
{
	// Token: 0x06001AD4 RID: 6868 RVA: 0x000C4B13 File Offset: 0x000C2F13
	public void StartEditing(Tile tile, EditableTileParameter parameter)
	{
		base.enabled = true;
		this.editing = true;
		this.selectedTile = tile;
		this.parameter = parameter;
		this.uiObject = parameter.SetupUI(tile);
		this.DestroyVisualizers();
		TileIconManager.Instance.ClearNewLoadLimit();
	}

	// Token: 0x06001AD5 RID: 6869 RVA: 0x000C4B4E File Offset: 0x000C2F4E
	public void UpdateValue(Tile tile)
	{
		if (base.enabled && this.editing)
		{
			this.parameter.objectValue = tile.gaf.Args[this.parameter.parameterIndex];
		}
	}

	// Token: 0x06001AD6 RID: 6870 RVA: 0x000C4B88 File Offset: 0x000C2F88
	public bool IsEditing()
	{
		return this.editing;
	}

	// Token: 0x06001AD7 RID: 6871 RVA: 0x000C4B90 File Offset: 0x000C2F90
	public void SetEditing(bool e)
	{
		TileIconManager.Instance.ClearNewLoadLimit();
		this.editing = e;
	}

	// Token: 0x06001AD8 RID: 6872 RVA: 0x000C4BA4 File Offset: 0x000C2FA4
	public void StopEditing()
	{
		if (!this.editing && this.selectedTile == null && this.uiObject == null)
		{
			return;
		}
		NumericHandleTileParameter numericHandleTileParameter = this.parameter as NumericHandleTileParameter;
		if (numericHandleTileParameter != null)
		{
			numericHandleTileParameter.ReleaseHandle();
		}
		if (this.uiObject != null)
		{
			UnityEngine.Object.Destroy(this.uiObject);
			this.uiObject = null;
		}
		if (this.selectedTile != null && this.selectedTile.IsShowing() && this.parameter != null)
		{
			this.parameter.WriteValueToTile(this.selectedTile);
		}
		this.DestroyVisualizers();
		if (this.parameter != null)
		{
			this.parameter.CleanupUI();
		}
		if (this.selectedTile != null)
		{
			this.selectedTile.doubleWidth = false;
			if (this.selectedTile.UpdateDynamicLabelIfNecessary())
			{
				Blocksworld.UpdateTiles();
			}
		}
		Blocksworld.scriptPanel.Layout();
		this.editing = false;
		base.enabled = false;
		this.selectedTile = null;
		Tutorial.Step();
		TileIconManager.Instance.ClearNewLoadLimit();
		History.AddStateIfNecessary();
	}

	// Token: 0x06001AD9 RID: 6873 RVA: 0x000C4CC8 File Offset: 0x000C30C8
	private void Update()
	{
		if (!this.editing)
		{
			return;
		}
		if (this.parameter != null && this.parameter.UIUpdate())
		{
			this.parameter.WriteValueToTile(this.selectedTile);
		}
		if (this.parameter == null || this.parameter.HasUIQuit() || !this.selectedTile.IsShowing())
		{
			this.StopEditing();
		}
		this.UpdateVisualizer();
		if (this.parameter != null)
		{
			this.parameter.OnHudMesh();
		}
	}

	// Token: 0x06001ADA RID: 6874 RVA: 0x000C4D5C File Offset: 0x000C315C
	private void UpdateVisualizer()
	{
		if (Blocksworld.selectedBlock != null && this.parameter != null && this.parameter.objectValue != null && this.parameter.settings != null && this.visualizers.ContainsKey(this.parameter.settings.visualizer))
		{
			this.visualizers[this.parameter.settings.visualizer].Update();
		}
	}

	// Token: 0x06001ADB RID: 6875 RVA: 0x000C4DE0 File Offset: 0x000C31E0
	private void DestroyVisualizers()
	{
		foreach (KeyValuePair<TileParameterVisualizer, ParameterValueVisualizer> keyValuePair in this.visualizers)
		{
			keyValuePair.Value.Destroy();
		}
	}

	// Token: 0x04001682 RID: 5762
	public Tile selectedTile;

	// Token: 0x04001683 RID: 5763
	public EditableTileParameter parameter;

	// Token: 0x04001684 RID: 5764
	private bool editing;

	// Token: 0x04001685 RID: 5765
	private GameObject uiObject;

	// Token: 0x04001686 RID: 5766
	private Dictionary<TileParameterVisualizer, ParameterValueVisualizer> visualizers = new Dictionary<TileParameterVisualizer, ParameterValueVisualizer>
	{
		{
			TileParameterVisualizer.DirectionAngleXZ,
			new DirectionAngleXZVisualizer()
		},
		{
			TileParameterVisualizer.Distance,
			new DistanceVisualizer
			{
				color = Color.white
			}
		},
		{
			TileParameterVisualizer.DistanceRed,
			new DistanceVisualizer
			{
				color = Color.red
			}
		},
		{
			TileParameterVisualizer.VerticalOffset,
			new VerticalOffsetVisualizer()
		},
		{
			TileParameterVisualizer.RadialExplosion,
			new RadialExplosionVisualizer()
		},
		{
			TileParameterVisualizer.RelativeDirectionAngleXZ,
			new RelativeDirectionAngleXZVisualizer()
		},
		{
			TileParameterVisualizer.AngleXY,
			new AngleXYVisualizer(1f)
		},
		{
			TileParameterVisualizer.AngleXYInverted,
			new AngleXYVisualizer(-1f)
		},
		{
			TileParameterVisualizer.TranslationAxisConstrain,
			new AxesVisualizer(true, true)
		},
		{
			TileParameterVisualizer.TranslationAxisFree,
			new AxesVisualizer(true, false)
		},
		{
			TileParameterVisualizer.RotationAxisConstrain,
			new AxesVisualizer(false, true)
		},
		{
			TileParameterVisualizer.RotationAxisFree,
			new AxesVisualizer(false, false)
		},
		{
			TileParameterVisualizer.SkyBoxRotation,
			new SkyBoxRotationVisualizer()
		},
		{
			TileParameterVisualizer.SunIntensity,
			new SunIntensityVisualizer()
		},
		{
			TileParameterVisualizer.FogParameters,
			new FogParametersVisualizer()
		}
	};
}
