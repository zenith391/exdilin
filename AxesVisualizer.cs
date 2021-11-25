using System;
using UnityEngine;

// Token: 0x0200023A RID: 570
public class AxesVisualizer : ParameterValueVisualizer
{
	// Token: 0x06001AEF RID: 6895 RVA: 0x000C5700 File Offset: 0x000C3B00
	public AxesVisualizer(bool isTranslationMode, bool isSelectedRed)
	{
		this.translationMode = isTranslationMode;
		if (isSelectedRed)
		{
			this.selectedAxisColor = AxesVisualizer.redAxisColor;
			this.deselectedAxisColor = AxesVisualizer.greenAxisColor;
		}
		else
		{
			this.selectedAxisColor = AxesVisualizer.greenAxisColor;
			this.deselectedAxisColor = AxesVisualizer.redAxisColor;
		}
	}

	// Token: 0x06001AF0 RID: 6896 RVA: 0x000C5754 File Offset: 0x000C3B54
	private Color ColorForAxis(AxesVisualizer.AxisValue axis, AxesVisualizer.AxisValue setting)
	{
		bool flag = setting == AxesVisualizer.AxisValue.ALL_AXES;
		flag |= (axis == setting);
		return (!flag) ? this.deselectedAxisColor : this.selectedAxisColor;
	}

	// Token: 0x06001AF1 RID: 6897 RVA: 0x000C5784 File Offset: 0x000C3B84
	public override void Update()
	{
		if (this.visualizerGo == null)
		{
			if (AxesVisualizer.rotationVisualizerPrefab == null)
			{
				AxesVisualizer.rotationVisualizerPrefab = (Resources.Load("GUI/Rotation Axis Visualizer") as GameObject);
			}
			if (AxesVisualizer.translationVisualizerPrefab == null)
			{
				AxesVisualizer.translationVisualizerPrefab = (Resources.Load("GUI/Translation Axis Visualizer") as GameObject);
			}
			if (this.translationMode)
			{
				this.visualizerPrefab = AxesVisualizer.translationVisualizerPrefab;
			}
			else
			{
				this.visualizerPrefab = AxesVisualizer.rotationVisualizerPrefab;
			}
			this.visualizerGo = new GameObject("AxesVisualizer");
			this.goX = UnityEngine.Object.Instantiate<GameObject>(this.visualizerPrefab);
			this.goY = UnityEngine.Object.Instantiate<GameObject>(this.visualizerPrefab);
			this.goZ = UnityEngine.Object.Instantiate<GameObject>(this.visualizerPrefab);
			this.goX.name = "X-axis";
			this.goY.name = "Y-axis";
			this.goZ.name = "Z-axis";
			this.goX.transform.localScale = new Vector3(0.4f, 100f, 0.4f);
			this.goY.transform.localScale = new Vector3(0.4f, 100f, 0.4f);
			this.goZ.transform.localScale = new Vector3(0.4f, 100f, 0.4f);
			this.goX.transform.rotation = Quaternion.AngleAxis(90f, Vector3.forward);
			this.goY.transform.rotation = Quaternion.identity;
			this.goZ.transform.rotation = Quaternion.AngleAxis(90f, Vector3.right);
			this.goX.transform.parent = this.visualizerGo.transform;
			this.goY.transform.parent = this.visualizerGo.transform;
			this.goZ.transform.parent = this.visualizerGo.transform;
		}
		AxesVisualizer.AxisValue setting = (AxesVisualizer.AxisValue)Blocksworld.bw.tileParameterEditor.parameter.objectValue;
		this.goX.GetComponent<Renderer>().material.color = this.ColorForAxis(AxesVisualizer.AxisValue.X_AXIS, setting);
		this.goY.GetComponent<Renderer>().material.color = this.ColorForAxis(AxesVisualizer.AxisValue.Y_AXIS, setting);
		this.goZ.GetComponent<Renderer>().material.color = this.ColorForAxis(AxesVisualizer.AxisValue.Z_AXIS, setting);
		this.visualizerGo.transform.position = Blocksworld.selectedBlock.go.transform.position;
	}

	// Token: 0x04001691 RID: 5777
	protected static GameObject rotationVisualizerPrefab;

	// Token: 0x04001692 RID: 5778
	protected static GameObject translationVisualizerPrefab;

	// Token: 0x04001693 RID: 5779
	private static Color redAxisColor = new Color(0.768627465f, 0.156862751f, 0.05882353f);

	// Token: 0x04001694 RID: 5780
	private static Color greenAxisColor = new Color(0.235294119f, 0.768627465f, 0.05882353f);

	// Token: 0x04001695 RID: 5781
	private bool translationMode;

	// Token: 0x04001696 RID: 5782
	private Color selectedAxisColor;

	// Token: 0x04001697 RID: 5783
	private Color deselectedAxisColor;

	// Token: 0x04001698 RID: 5784
	private GameObject visualizerPrefab;

	// Token: 0x04001699 RID: 5785
	private GameObject goX;

	// Token: 0x0400169A RID: 5786
	private GameObject goY;

	// Token: 0x0400169B RID: 5787
	private GameObject goZ;

	// Token: 0x0200023B RID: 571
	public enum AxisValue
	{
		// Token: 0x0400169D RID: 5789
		ALL_AXES,
		// Token: 0x0400169E RID: 5790
		X_AXIS,
		// Token: 0x0400169F RID: 5791
		Y_AXIS,
		// Token: 0x040016A0 RID: 5792
		Z_AXIS
	}
}
