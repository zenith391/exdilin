using System;
using System.Collections.Generic;

// Token: 0x0200003F RID: 63
public class ObjectData
{
	// Token: 0x06000218 RID: 536 RVA: 0x0000C6BC File Offset: 0x0000AABC
	public string GetString(string key)
	{
		string result;
		if (this.strings.TryGetValue(key, out result))
		{
			return result;
		}
		return string.Empty;
	}

	// Token: 0x06000219 RID: 537 RVA: 0x0000C6E3 File Offset: 0x0000AAE3
	public void SetString(string key, string s)
	{
		this.strings[key] = s;
	}

	// Token: 0x0600021A RID: 538 RVA: 0x0000C6F2 File Offset: 0x0000AAF2
	public bool TryGetString(string key, out string value)
	{
		return this.strings.TryGetValue(key, out value);
	}

	// Token: 0x0600021B RID: 539 RVA: 0x0000C704 File Offset: 0x0000AB04
	public bool GetBoolean(string key)
	{
		bool flag;
		return this.bools.TryGetValue(key, out flag) && flag;
	}

	// Token: 0x0600021C RID: 540 RVA: 0x0000C727 File Offset: 0x0000AB27
	public void SetBoolean(string key, bool b)
	{
		this.bools[key] = b;
	}

	// Token: 0x0600021D RID: 541 RVA: 0x0000C736 File Offset: 0x0000AB36
	public bool TryGetFloat(string key, out float value)
	{
		return this.floats.TryGetValue(key, out value);
	}

	// Token: 0x0600021E RID: 542 RVA: 0x0000C748 File Offset: 0x0000AB48
	public float GetFloat(string key)
	{
		float result;
		if (this.floats.TryGetValue(key, out result))
		{
			return result;
		}
		return 0f;
	}

	// Token: 0x0600021F RID: 543 RVA: 0x0000C76F File Offset: 0x0000AB6F
	public void SetFloat(string key, float f)
	{
		this.floats[key] = f;
	}

	// Token: 0x06000220 RID: 544 RVA: 0x0000C77E File Offset: 0x0000AB7E
	public void RemoveFloat(string key)
	{
		this.floats.Remove(key);
	}

	// Token: 0x04000204 RID: 516
	private Dictionary<string, bool> bools = new Dictionary<string, bool>();

	// Token: 0x04000205 RID: 517
	private Dictionary<string, float> floats = new Dictionary<string, float>();

	// Token: 0x04000206 RID: 518
	private Dictionary<string, string> strings = new Dictionary<string, string>();
}
