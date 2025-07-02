using UnityEngine;

public class AxesVisualizer : ParameterValueVisualizer
{
	public enum AxisValue
	{
		ALL_AXES,
		X_AXIS,
		Y_AXIS,
		Z_AXIS
	}

	protected static GameObject rotationVisualizerPrefab;

	protected static GameObject translationVisualizerPrefab;

	private static Color redAxisColor = new Color(0.76862746f, 8f / 51f, 1f / 17f);

	private static Color greenAxisColor = new Color(0.23529412f, 0.76862746f, 1f / 17f);

	private bool translationMode;

	private Color selectedAxisColor;

	private Color deselectedAxisColor;

	private GameObject visualizerPrefab;

	private GameObject goX;

	private GameObject goY;

	private GameObject goZ;

	public AxesVisualizer(bool isTranslationMode, bool isSelectedRed)
	{
		translationMode = isTranslationMode;
		if (isSelectedRed)
		{
			selectedAxisColor = redAxisColor;
			deselectedAxisColor = greenAxisColor;
		}
		else
		{
			selectedAxisColor = greenAxisColor;
			deselectedAxisColor = redAxisColor;
		}
	}

	private Color ColorForAxis(AxisValue axis, AxisValue setting)
	{
		bool flag = setting == AxisValue.ALL_AXES;
		if (flag || axis == setting)
		{
			return selectedAxisColor;
		}
		return deselectedAxisColor;
	}

	public override void Update()
	{
		if (visualizerGo == null)
		{
			if (rotationVisualizerPrefab == null)
			{
				rotationVisualizerPrefab = Resources.Load("GUI/Rotation Axis Visualizer") as GameObject;
			}
			if (translationVisualizerPrefab == null)
			{
				translationVisualizerPrefab = Resources.Load("GUI/Translation Axis Visualizer") as GameObject;
			}
			if (translationMode)
			{
				visualizerPrefab = translationVisualizerPrefab;
			}
			else
			{
				visualizerPrefab = rotationVisualizerPrefab;
			}
			visualizerGo = new GameObject("AxesVisualizer");
			goX = Object.Instantiate(visualizerPrefab);
			goY = Object.Instantiate(visualizerPrefab);
			goZ = Object.Instantiate(visualizerPrefab);
			goX.name = "X-axis";
			goY.name = "Y-axis";
			goZ.name = "Z-axis";
			goX.transform.localScale = new Vector3(0.4f, 100f, 0.4f);
			goY.transform.localScale = new Vector3(0.4f, 100f, 0.4f);
			goZ.transform.localScale = new Vector3(0.4f, 100f, 0.4f);
			goX.transform.rotation = Quaternion.AngleAxis(90f, Vector3.forward);
			goY.transform.rotation = Quaternion.identity;
			goZ.transform.rotation = Quaternion.AngleAxis(90f, Vector3.right);
			goX.transform.parent = visualizerGo.transform;
			goY.transform.parent = visualizerGo.transform;
			goZ.transform.parent = visualizerGo.transform;
		}
		AxisValue setting = (AxisValue)Blocksworld.bw.tileParameterEditor.parameter.objectValue;
		goX.GetComponent<Renderer>().material.color = ColorForAxis(AxisValue.X_AXIS, setting);
		goY.GetComponent<Renderer>().material.color = ColorForAxis(AxisValue.Y_AXIS, setting);
		goZ.GetComponent<Renderer>().material.color = ColorForAxis(AxisValue.Z_AXIS, setting);
		visualizerGo.transform.position = Blocksworld.selectedBlock.go.transform.position;
	}
}
