using System;
using UnityEngine;

// Token: 0x0200040A RID: 1034
[Serializable]
public class FontDef
{
	// Token: 0x040025B4 RID: 9652
	public bool useDefault = true;

	// Token: 0x040025B5 RID: 9653
	public Font font;

	// Token: 0x040025B6 RID: 9654
	public float fontSize = 20f;

	// Token: 0x040025B7 RID: 9655
	public FontStyle fontStyle;

	// Token: 0x0200040B RID: 1035
	public enum Style
	{
		// Token: 0x040025B9 RID: 9657
		Default,
		// Token: 0x040025BA RID: 9658
		DescNorm,
		// Token: 0x040025BB RID: 9659
		DescBold,
		// Token: 0x040025BC RID: 9660
		Title,
		// Token: 0x040025BD RID: 9661
		MunuBar,
		// Token: 0x040025BE RID: 9662
		TestFontStyle1,
		// Token: 0x040025BF RID: 9663
		TestFontStyle2,
		// Token: 0x040025C0 RID: 9664
		TestFontStyle3,
		// Token: 0x040025C1 RID: 9665
		TestFontStyle4,
		// Token: 0x040025C2 RID: 9666
		TestFontStyle5,
		// Token: 0x040025C3 RID: 9667
		TestFontStyle6,
		// Token: 0x040025C4 RID: 9668
		TestFontStyle7,
		// Token: 0x040025C5 RID: 9669
		TestFontStyle8,
		// Token: 0x040025C6 RID: 9670
		Count
	}
}
