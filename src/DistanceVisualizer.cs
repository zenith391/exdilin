using System;
using UnityEngine;

// Token: 0x02000233 RID: 563
public class DistanceVisualizer : ParameterValueVisualizer
{
	// Token: 0x06001AE0 RID: 6880 RVA: 0x000C4E88 File Offset: 0x000C3288
	public override void Update()
	{
		if (this.visualizerGo == null)
		{
			if (DistanceVisualizer.visualizerPrefab == null)
			{
				DistanceVisualizer.visualizerPrefab = (Resources.Load("GUI/Distance Visualizer") as GameObject);
			}
			this.visualizerGo = UnityEngine.Object.Instantiate<GameObject>(DistanceVisualizer.visualizerPrefab);
			this.visualizerGo.GetComponent<Renderer>().material.color = this.color;
		}
		float d = (float)Blocksworld.bw.tileParameterEditor.parameter.objectValue;
		Vector3 position = Blocksworld.selectedBlock.go.transform.position;
		this.visualizerGo.transform.position = position;
		this.visualizerGo.transform.localScale = Vector3.one * d * 2f;
	}

	// Token: 0x04001688 RID: 5768
	protected static GameObject visualizerPrefab;

	// Token: 0x04001689 RID: 5769
	public Color color = Color.white;
}
