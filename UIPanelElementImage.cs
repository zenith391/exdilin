using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000418 RID: 1048
[RequireComponent(typeof(RawImage))]
public class UIPanelElementImage : UIPanelElement
{
	// Token: 0x06002D7C RID: 11644 RVA: 0x00144750 File Offset: 0x00142B50
	public override void Init(UIPanelContents parentPanel)
	{
		base.Init(parentPanel);
		this.image = base.GetComponent<RawImage>();
		this.imageTransform = (RectTransform)this.image.transform;
		this.baseSize = this.imageTransform.sizeDelta;
	}

	// Token: 0x06002D7D RID: 11645 RVA: 0x0014478C File Offset: 0x00142B8C
	public override void Clear()
	{
		base.Clear();
		this.image.texture = null;
		this.image.enabled = false;
	}

	// Token: 0x06002D7E RID: 11646 RVA: 0x001447AC File Offset: 0x00142BAC
	public override void Fill(Dictionary<string, string> data)
	{
		string text;
		if (data.TryGetValue(this.dataKey, out text) && !string.IsNullOrEmpty(text))
		{
			this.image.texture = Resources.Load<Texture2D>(text);
			if (this.image.texture != null)
			{
				this.image.enabled = true;
				int width = this.image.texture.width;
				int height = this.image.texture.height;
				bool flag = width > height;
				float num = (float)width / (float)height;
				if (this.resizeMode == UIPanelElementImage.ResizeMode.Fill)
				{
					if (flag)
					{
						this.imageTransform.sizeDelta = new Vector2(num * this.baseSize.y, this.baseSize.y);
					}
					else
					{
						this.imageTransform.sizeDelta = new Vector2(this.baseSize.x, this.baseSize.y / num);
					}
				}
				else if (this.resizeMode == UIPanelElementImage.ResizeMode.Fit)
				{
					if (flag)
					{
						this.imageTransform.sizeDelta = new Vector2(this.baseSize.x, this.baseSize.y / num);
					}
					else
					{
						this.imageTransform.sizeDelta = new Vector2(num * this.baseSize.y, this.baseSize.y);
					}
				}
			}
		}
		else
		{
			BWLog.Info("No data for key " + this.dataKey);
			this.image.texture = null;
			this.image.enabled = false;
		}
	}

	// Token: 0x04002605 RID: 9733
	public string dataKey = "imagePath";

	// Token: 0x04002606 RID: 9734
	public UIPanelElementImage.ResizeMode resizeMode;

	// Token: 0x04002607 RID: 9735
	private RawImage image;

	// Token: 0x04002608 RID: 9736
	private RectTransform imageTransform;

	// Token: 0x04002609 RID: 9737
	private Vector2 baseSize;

	// Token: 0x02000419 RID: 1049
	public enum ResizeMode
	{
		// Token: 0x0400260B RID: 9739
		None,
		// Token: 0x0400260C RID: 9740
		Fill,
		// Token: 0x0400260D RID: 9741
		Fit
	}
}
