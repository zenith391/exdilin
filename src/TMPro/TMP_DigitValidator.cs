using System;

namespace TMPro
{
	// Token: 0x0200037E RID: 894
	[Serializable]
	public class TMP_DigitValidator : TMP_InputValidator
	{
		// Token: 0x060027B8 RID: 10168 RVA: 0x00123B85 File Offset: 0x00121F85
		public override char Validate(ref string text, ref int pos, char ch)
		{
			if (ch >= '0' && ch <= '9')
			{
				pos++;
				return ch;
			}
			return '\0';
		}
	}
}
