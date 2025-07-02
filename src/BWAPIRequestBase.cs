using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class BWAPIRequestBase
{
	public delegate void SuccessHandler(JObject responseJson);

	public delegate void FailureHandler(BWAPIRequestError error);

	public SuccessHandler onSuccess;

	public FailureHandler onFailure;

	public virtual void AddParam(string key, string valueStr)
	{
	}

	public virtual void AddImageData(string key, byte[] data, string filename, string mimeType)
	{
	}

	public virtual void AddJsonParameters(string jsonStr)
	{
	}

	public virtual void Send()
	{
	}

	public virtual void SendOwnerCoroutine(MonoBehaviour owner)
	{
	}

	public void AddParams(Dictionary<string, string> attrs)
	{
		foreach (KeyValuePair<string, string> attr in attrs)
		{
			AddParam(attr.Key, attr.Value);
		}
	}
}
