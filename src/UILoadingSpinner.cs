using System;
using UnityEngine;

// Token: 0x02000309 RID: 777
public class UILoadingSpinner : MonoBehaviour
{
	// Token: 0x060022FA RID: 8954 RVA: 0x0010404C File Offset: 0x0010244C
	private void Start()
	{
		this.ourSpinner = base.gameObject.GetComponent<RectTransform>();
	}

	// Token: 0x060022FB RID: 8955 RVA: 0x00104060 File Offset: 0x00102460
	private void Update()
	{
		if (base.gameObject.activeSelf)
		{
			this.waitTime += Time.deltaTime;
			if (this.waitTime > 0.1f)
			{
				this.ourSpinner.Rotate(0f, 0f, -30f);
				this.waitTime = 0f;
			}
		}
	}

	// Token: 0x04001E1E RID: 7710
	private RectTransform ourSpinner;

	// Token: 0x04001E1F RID: 7711
	private float waitTime;
}
