using System;

// Token: 0x0200016E RID: 366
public class StringArgumentPattenMatch : ArgumentPatternMatch
{
	// Token: 0x06001583 RID: 5507 RVA: 0x00096810 File Offset: 0x00094C10
	public StringArgumentPattenMatch(string patternStr)
	{
		if (patternStr == "*")
		{
			this._matchAny = true;
		}
		else if (patternStr.Contains(","))
		{
			this._multivalued = true;
			this._multiValues = patternStr.Split(new char[]
			{
				','
			});
		}
		else
		{
			this._value = patternStr;
		}
	}

	// Token: 0x06001584 RID: 5508 RVA: 0x0009687C File Offset: 0x00094C7C
	public override bool Matches(object o)
	{
		if (o is string)
		{
			if (this._matchAny)
			{
				return true;
			}
			if (!this._multivalued)
			{
				return (o as string).Equals(this._value);
			}
			string text = o as string;
			foreach (string value in this._multiValues)
			{
				if (text.Equals(value))
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x040010C8 RID: 4296
	private string _value;

	// Token: 0x040010C9 RID: 4297
	private bool _matchAny;

	// Token: 0x040010CA RID: 4298
	private bool _multivalued;

	// Token: 0x040010CB RID: 4299
	private string[] _multiValues;
}
