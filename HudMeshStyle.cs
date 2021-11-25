using System;
using UnityEngine;

// Token: 0x02000196 RID: 406
[Serializable]
public class HudMeshStyle
{
	// Token: 0x060016C6 RID: 5830 RVA: 0x000A2EB0 File Offset: 0x000A12B0
	public HudMeshStyle Clone()
	{
		return new HudMeshStyle
		{
			id = this.id,
			backgroundTexture = this.backgroundTexture,
			textColor = this.textColor,
			font = this.font,
			fontStyle = this.fontStyle,
			fontSize = this.fontSize,
			alignment = this.alignment,
			wordWrap = this.wordWrap,
			stretchWidth = this.stretchWidth,
			stretchHeight = this.stretchHeight,
			border = this.border,
			padding = this.padding
		};
	}

	// Token: 0x040011C6 RID: 4550
	public string id;

	// Token: 0x040011C7 RID: 4551
	public Texture2D backgroundTexture;

	// Token: 0x040011C8 RID: 4552
	public Color textColor;

	// Token: 0x040011C9 RID: 4553
	public Font font;

	// Token: 0x040011CA RID: 4554
	public FontStyle fontStyle;

	// Token: 0x040011CB RID: 4555
	public int fontSize;

	// Token: 0x040011CC RID: 4556
	public TextAnchor alignment;

	// Token: 0x040011CD RID: 4557
	public bool wordWrap;

	// Token: 0x040011CE RID: 4558
	public bool stretchWidth;

	// Token: 0x040011CF RID: 4559
	public bool stretchHeight;

	// Token: 0x040011D0 RID: 4560
	public RectOffset border;

	// Token: 0x040011D1 RID: 4561
	public RectOffset padding;
}
