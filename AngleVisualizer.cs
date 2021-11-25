using System;
using UnityEngine;

// Token: 0x02000235 RID: 565
public abstract class AngleVisualizer : ParameterValueVisualizer
{
	// Token: 0x06001AE3 RID: 6883 RVA: 0x000C507F File Offset: 0x000C347F
	public AngleVisualizer(float sign = 1f)
	{
		this.sign = sign;
	}

	// Token: 0x06001AE4 RID: 6884 RVA: 0x000C509C File Offset: 0x000C349C
	public override void Update()
	{
		Transform transform = Blocksworld.selectedBlock.go.transform;
		if (this.visualizerGo == null)
		{
			if (AngleVisualizer.visualizerPrefab == null)
			{
				AngleVisualizer.visualizerPrefab = (Resources.Load("GUI/Angle Visualizer") as GameObject);
			}
			this.visualizerGo = UnityEngine.Object.Instantiate<GameObject>(AngleVisualizer.visualizerPrefab);
			this.visualizerGo.transform.localScale = Vector3.one * 2f;
			this.movingArrow = this.visualizerGo.transform.Find("Arrow 1").gameObject;
			this.movingArrow.transform.Find("Head").gameObject.GetComponent<Renderer>().material.color = Color.red;
			this.movingArrow.transform.Find("Body").gameObject.GetComponent<Renderer>().material.color = Color.red;
			GameObject gameObject = this.visualizerGo.transform.Find("Arrow 2").gameObject;
			gameObject.transform.rotation = this.GetArrowRotation(0f, transform);
		}
		float angle = (float)Blocksworld.bw.tileParameterEditor.parameter.objectValue;
		Vector3 position = transform.position;
		this.visualizerGo.transform.position = position + Vector3.up * Util.MaxComponent(Blocksworld.selectedBlock.size);
		this.movingArrow.transform.rotation = this.GetArrowRotation(angle, transform);
	}

	// Token: 0x06001AE5 RID: 6885
	protected abstract Quaternion GetArrowRotation(float angle, Transform blockT);

	// Token: 0x0400168C RID: 5772
	protected static GameObject visualizerPrefab;

	// Token: 0x0400168D RID: 5773
	private GameObject movingArrow;

	// Token: 0x0400168E RID: 5774
	protected float sign = 1f;
}
