using System;
using UnityEngine;

// Token: 0x02000237 RID: 567
public class DirectionAngleXZVisualizer : ParameterValueVisualizer
{
	// Token: 0x06001AE9 RID: 6889 RVA: 0x000C5268 File Offset: 0x000C3668
	public override void Update()
	{
		if (this.visualizerGo == null)
		{
			if (DirectionAngleXZVisualizer.visualizerPrefab == null)
			{
				DirectionAngleXZVisualizer.visualizerPrefab = (Resources.Load("GUI/Direction Visualizer") as GameObject);
			}
			this.visualizerGo = UnityEngine.Object.Instantiate<GameObject>(DirectionAngleXZVisualizer.visualizerPrefab);
		}
		float angle = (float)Blocksworld.bw.tileParameterEditor.parameter.objectValue;
		Vector3 position = Blocksworld.selectedBlock.go.transform.position;
		Quaternion rotation = this.GetRotation(angle);
		this.visualizerGo.transform.position = position + Vector3.up * Util.MaxComponent(Blocksworld.selectedBlock.size);
		this.visualizerGo.transform.rotation = rotation;
		this.visualizerGo.transform.localScale = new Vector3(3f, 3f, 3f);
	}

	// Token: 0x06001AEA RID: 6890 RVA: 0x000C5355 File Offset: 0x000C3755
	protected virtual Quaternion GetRotation(float angle)
	{
		return Quaternion.Euler(0f, angle + 180f, 0f);
	}

	// Token: 0x0400168F RID: 5775
	protected static GameObject visualizerPrefab;
}
