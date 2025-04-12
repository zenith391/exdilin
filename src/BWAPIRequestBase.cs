using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

// Token: 0x02000032 RID: 50
public class BWAPIRequestBase
{
	// Token: 0x060001CD RID: 461 RVA: 0x0000A5A3 File Offset: 0x000089A3
	public virtual void AddParam(string key, string valueStr)
	{
	}

	// Token: 0x060001CE RID: 462 RVA: 0x0000A5A5 File Offset: 0x000089A5
	public virtual void AddImageData(string key, byte[] data, string filename, string mimeType)
	{
	}

	// Token: 0x060001CF RID: 463 RVA: 0x0000A5A7 File Offset: 0x000089A7
	public virtual void AddJsonParameters(string jsonStr)
	{
	}

	// Token: 0x060001D0 RID: 464 RVA: 0x0000A5A9 File Offset: 0x000089A9
	public virtual void Send()
	{
	}

    // Token: 0x060001D1 RID: 465 RVA: 0x0000A5AB File Offset: 0x000089AB
    public virtual void SendOwnerCoroutine(MonoBehaviour owner)
	{
	}

	// Token: 0x060001D2 RID: 466 RVA: 0x0000A5B0 File Offset: 0x000089B0
	public void AddParams(Dictionary<string, string> attrs)
	{
		foreach (KeyValuePair<string, string> keyValuePair in attrs)
		{
			this.AddParam(keyValuePair.Key, keyValuePair.Value);
		}
	}

	// Token: 0x040001CA RID: 458
	public BWAPIRequestBase.SuccessHandler onSuccess;

	// Token: 0x040001CB RID: 459
	public BWAPIRequestBase.FailureHandler onFailure;

	// Token: 0x02000033 RID: 51
	// (Invoke) Token: 0x060001D4 RID: 468
	public delegate void SuccessHandler(JObject responseJson);

	// Token: 0x02000034 RID: 52
	// (Invoke) Token: 0x060001D8 RID: 472
	public delegate void FailureHandler(BWAPIRequestError error);
}
