using System;
using UnityEngine;

// Token: 0x0200040E RID: 1038
[ExecuteInEditMode]
public class UIPanelContentScaler : MonoBehaviour
{
	// Token: 0x06002D42 RID: 11586 RVA: 0x00143238 File Offset: 0x00141638
	private void Update()
	{
		this.parentTransform = (RectTransform)base.transform.parent;
		if (this.parentTransform != null && this.defaultParentSize.sqrMagnitude > Mathf.Epsilon)
		{
			RectTransform rectTransform = (RectTransform)base.transform;
			if (this.useWidth)
			{
				this.scale.y = (this.scale.x = this.parentTransform.rect.width / this.defaultParentSize.x);
			}
			else
			{
				this.scale.y = (this.scale.x = this.parentTransform.rect.height / this.defaultParentSize.y);
			}
			rectTransform.localScale = this.scale;
		}
	}

	// Token: 0x040025D3 RID: 9683
	public bool useWidth;

	// Token: 0x040025D4 RID: 9684
	public Vector2 defaultParentSize;

	// Token: 0x040025D5 RID: 9685
	private RectTransform parentTransform;

	// Token: 0x040025D6 RID: 9686
	private Vector3 scale = Vector3.one;
}
