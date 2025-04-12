using System;

// Token: 0x0200016F RID: 367
public class BoolArgumentPatternMatch : ArgumentPatternMatch
{
	// Token: 0x06001585 RID: 5509 RVA: 0x000968F8 File Offset: 0x00094CF8
	public BoolArgumentPatternMatch(string patternStr)
	{
		if (patternStr == "*")
		{
			this._matchAny = true;
		}
		else if (patternStr == "True")
		{
			this._value = true;
		}
		else
		{
			this._value = false;
		}
	}

	// Token: 0x06001586 RID: 5510 RVA: 0x0009694C File Offset: 0x00094D4C
	public override bool Matches(object o)
	{
		return o is bool && (this._matchAny || ((bool)o).Equals(this._value));
	}

	// Token: 0x040010CC RID: 4300
	private bool _value;

	// Token: 0x040010CD RID: 4301
	private bool _matchAny;
}
