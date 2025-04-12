using System;
using UnityEngine;

// Token: 0x0200016D RID: 365
public class VectorArgumentPatternMatch : ArgumentPatternMatch
{
	// Token: 0x06001581 RID: 5505 RVA: 0x00096738 File Offset: 0x00094B38
	public VectorArgumentPatternMatch(string patternStr)
	{
		if (patternStr == "*")
		{
			this._matchAny = true;
		}
		else
		{
			string[] array = patternStr.Split(new char[]
			{
				','
			});
			if (array.Length == 3)
			{
				float x;
				bool flag = float.TryParse(array[0], out x);
				float y;
				flag &= float.TryParse(array[1], out y);
				float z;
				flag &= float.TryParse(array[2], out z);
				if (flag)
				{
					this._value = new Vector3(x, y, z);
				}
			}
		}
	}

	// Token: 0x06001582 RID: 5506 RVA: 0x000967C4 File Offset: 0x00094BC4
	public override bool Matches(object o)
	{
		if (!(o is Vector3))
		{
			return false;
		}
		if (this._matchAny)
		{
			return true;
		}
		Vector3 a = (Vector3)o;
		return (a - this._value).sqrMagnitude < Mathf.Epsilon;
	}

	// Token: 0x040010C6 RID: 4294
	private Vector3 _value;

	// Token: 0x040010C7 RID: 4295
	private bool _matchAny;
}
