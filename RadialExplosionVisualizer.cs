using System;
using UnityEngine;

// Token: 0x02000234 RID: 564
public class RadialExplosionVisualizer : ParameterValueVisualizer
{
	// Token: 0x06001AE2 RID: 6882 RVA: 0x000C4F64 File Offset: 0x000C3364
	public override void Update()
	{
		if (this.visualizerGo == null)
		{
			if (RadialExplosionVisualizer.visualizerPrefab == null)
			{
				RadialExplosionVisualizer.visualizerPrefab = (Resources.Load("GUI/Distance Visualizer") as GameObject);
			}
			this.visualizerGo = UnityEngine.Object.Instantiate<GameObject>(RadialExplosionVisualizer.visualizerPrefab);
			this.visualizerGo.GetComponent<Renderer>().material.color = Color.red;
			this.childGo = UnityEngine.Object.Instantiate<GameObject>(RadialExplosionVisualizer.visualizerPrefab);
			this.childGo.transform.parent = this.visualizerGo.transform;
		}
		float d = (float)Blocksworld.bw.tileParameterEditor.parameter.objectValue;
		Vector3 position = Blocksworld.selectedBlock.go.transform.position;
		this.visualizerGo.transform.position = position;
		this.visualizerGo.transform.localScale = Vector3.one * d * 2f;
		this.childGo.transform.localScale = Vector3.one * 2f;
	}

	// Token: 0x0400168A RID: 5770
	protected static GameObject visualizerPrefab;

	// Token: 0x0400168B RID: 5771
	private GameObject childGo;
}
