using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020002F4 RID: 756
public class HighlightableText : Text
{
	// Token: 0x06002237 RID: 8759 RVA: 0x000FF4DD File Offset: 0x000FD8DD
	private new void Start()
	{
		this._defaultColor = base.color;
	}

	// Token: 0x06002238 RID: 8760 RVA: 0x000FF4EB File Offset: 0x000FD8EB
	public void Highlight(bool highlight)
	{
		base.color = ((!highlight) ? this._defaultColor : this.highlightColor);
	}

	// Token: 0x04001D32 RID: 7474
	public Color highlightColor;

	// Token: 0x04001D33 RID: 7475
	private Color _defaultColor;
}
