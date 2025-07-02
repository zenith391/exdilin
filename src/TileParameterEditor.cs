using System.Collections.Generic;
using UnityEngine;

public class TileParameterEditor : MonoBehaviour
{
	public Tile selectedTile;

	public EditableTileParameter parameter;

	private bool editing;

	private GameObject uiObject;

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
			new AngleXYVisualizer()
		},
		{
			TileParameterVisualizer.AngleXYInverted,
			new AngleXYVisualizer(-1f)
		},
		{
			TileParameterVisualizer.TranslationAxisConstrain,
			new AxesVisualizer(isTranslationMode: true, isSelectedRed: true)
		},
		{
			TileParameterVisualizer.TranslationAxisFree,
			new AxesVisualizer(isTranslationMode: true, isSelectedRed: false)
		},
		{
			TileParameterVisualizer.RotationAxisConstrain,
			new AxesVisualizer(isTranslationMode: false, isSelectedRed: true)
		},
		{
			TileParameterVisualizer.RotationAxisFree,
			new AxesVisualizer(isTranslationMode: false, isSelectedRed: false)
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

	public void StartEditing(Tile tile, EditableTileParameter parameter)
	{
		base.enabled = true;
		editing = true;
		selectedTile = tile;
		this.parameter = parameter;
		uiObject = parameter.SetupUI(tile);
		DestroyVisualizers();
		TileIconManager.Instance.ClearNewLoadLimit();
	}

	public void UpdateValue(Tile tile)
	{
		if (base.enabled && editing)
		{
			parameter.objectValue = tile.gaf.Args[parameter.parameterIndex];
		}
	}

	public bool IsEditing()
	{
		return editing;
	}

	public void SetEditing(bool e)
	{
		TileIconManager.Instance.ClearNewLoadLimit();
		editing = e;
	}

	public void StopEditing()
	{
		if (!editing && selectedTile == null && uiObject == null)
		{
			return;
		}
		if (parameter is NumericHandleTileParameter numericHandleTileParameter)
		{
			numericHandleTileParameter.ReleaseHandle();
		}
		if (uiObject != null)
		{
			Object.Destroy(uiObject);
			uiObject = null;
		}
		if (selectedTile != null && selectedTile.IsShowing() && parameter != null)
		{
			parameter.WriteValueToTile(selectedTile);
		}
		DestroyVisualizers();
		if (parameter != null)
		{
			parameter.CleanupUI();
		}
		if (selectedTile != null)
		{
			selectedTile.doubleWidth = false;
			if (selectedTile.UpdateDynamicLabelIfNecessary())
			{
				Blocksworld.UpdateTiles();
			}
		}
		Blocksworld.scriptPanel.Layout();
		editing = false;
		base.enabled = false;
		selectedTile = null;
		Tutorial.Step();
		TileIconManager.Instance.ClearNewLoadLimit();
		History.AddStateIfNecessary();
	}

	private void Update()
	{
		if (editing)
		{
			if (parameter != null && parameter.UIUpdate())
			{
				parameter.WriteValueToTile(selectedTile);
			}
			if (parameter == null || parameter.HasUIQuit() || !selectedTile.IsShowing())
			{
				StopEditing();
			}
			UpdateVisualizer();
			if (parameter != null)
			{
				parameter.OnHudMesh();
			}
		}
	}

	private void UpdateVisualizer()
	{
		if (Blocksworld.selectedBlock != null && parameter != null && parameter.objectValue != null && parameter.settings != null && visualizers.ContainsKey(parameter.settings.visualizer))
		{
			visualizers[parameter.settings.visualizer].Update();
		}
	}

	private void DestroyVisualizers()
	{
		foreach (KeyValuePair<TileParameterVisualizer, ParameterValueVisualizer> visualizer in visualizers)
		{
			visualizer.Value.Destroy();
		}
	}
}
