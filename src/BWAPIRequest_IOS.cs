using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x02000036 RID: 54
public class BWAPIRequest_IOS : BWAPIRequestBase
{
	// Token: 0x060001E0 RID: 480 RVA: 0x0000A6EC File Offset: 0x00008AEC
	public BWAPIRequest_IOS(string method, string path)
	{
		this.Method = this.validHttpMethod(method);
		this.Path = path;
		this.Params = new Dictionary<string, string>();
	}

	// Token: 0x060001E1 RID: 481 RVA: 0x0000A714 File Offset: 0x00008B14
	private static int RegisterRequest(BWAPIRequest_IOS request)
	{
		int num = ++BWAPIRequest_IOS.LastRequestId;
		if (BWAPIRequest_IOS.Requests.ContainsKey(num))
		{
			throw new InvalidOperationException("Requests already contains id " + num);
		}
		BWAPIRequest_IOS.Requests[num] = request;
		return num;
	}

	// Token: 0x060001E2 RID: 482 RVA: 0x0000A764 File Offset: 0x00008B64
	private static BWAPIRequest_IOS CompleteRequest(int requestId)
	{
		if (!BWAPIRequest_IOS.Requests.ContainsKey(requestId))
		{
			throw new InvalidOperationException("Failed to find request with id " + requestId);
		}
		BWAPIRequest_IOS result = BWAPIRequest_IOS.Requests[requestId];
		BWAPIRequest_IOS.Requests.Remove(requestId);
		return result;
	}

	// Token: 0x060001E3 RID: 483 RVA: 0x0000A7B0 File Offset: 0x00008BB0
	public static void RequestSucceeded(int requestId, string responseJsonStr)
	{
		BWAPIRequest_IOS bwapirequest_IOS = BWAPIRequest_IOS.CompleteRequest(requestId);
		if (bwapirequest_IOS.onSuccess != null)
		{
			JObject responseJson = JSONDecoder.Decode(responseJsonStr);
			bwapirequest_IOS.onSuccess(responseJson);
		}
	}

	// Token: 0x060001E4 RID: 484 RVA: 0x0000A7E4 File Offset: 0x00008BE4
	public static void RequestFailed(int requestId, string errorJsonStr)
	{
		BWAPIRequest_IOS bwapirequest_IOS = BWAPIRequest_IOS.CompleteRequest(requestId);
		if (bwapirequest_IOS.onFailure != null)
		{
			BWAPIRequestError error = BWAPIRequestError.BuildWithJsonStr(errorJsonStr);
			bwapirequest_IOS.onFailure(error);
		}
	}

	// Token: 0x060001E5 RID: 485 RVA: 0x0000A816 File Offset: 0x00008C16
	public override void AddParam(string key, string valueStr)
	{
		if (this.Params.ContainsKey(key))
		{
			throw new InvalidOperationException("Attempt to add duplicate param key " + key + " with value " + valueStr);
		}
		this.Params[key] = valueStr;
	}

	// Token: 0x060001E6 RID: 486 RVA: 0x0000A850 File Offset: 0x00008C50
	public override void Send()
	{
		int requestId = BWAPIRequest_IOS.RegisterRequest(this);
		string method = this.Method;
		if (method != null)
		{
			if (!(method == "GET"))
			{
				if (!(method == "POST"))
				{
					if (!(method == "PUT"))
					{
						if (method == "DELETE")
						{
							IOSInterface.ApiDelete(requestId, this.Path);
						}
					}
					else
					{
						IOSInterface.ApiPut(requestId, this.Path, this.ParamsAsJsonStr());
					}
				}
				else
				{
					IOSInterface.ApiPost(requestId, this.Path, this.ParamsAsJsonStr());
				}
			}
			else
			{
				IOSInterface.ApiGet(requestId, this.Path, this.ParamsAsJsonStr());
			}
		}
	}

	// Token: 0x060001E7 RID: 487 RVA: 0x0000A90C File Offset: 0x00008D0C
	private string ParamsAsJsonStr()
	{
		if (this.Params.Count == 0)
		{
			return null;
		}
		return JSONEncoder.Encode(this.Params);
	}

	// Token: 0x060001E8 RID: 488 RVA: 0x0000A92C File Offset: 0x00008D2C
	private string validHttpMethod(string method)
	{
		string text = method.ToUpper();
		if (text != "GET" && text != "POST" && text != "PUT" && text != "DELETE")
		{
			throw new InvalidOperationException("Unexpected HTTP method '" + method + "'. Expected one of 'GET', 'POST', 'PUT' or 'DELETE'.");
		}
		return text;
	}

	// Token: 0x040001D1 RID: 465
	private static readonly Dictionary<int, BWAPIRequest_IOS> Requests = new Dictionary<int, BWAPIRequest_IOS>();

	// Token: 0x040001D2 RID: 466
	private static int LastRequestId = 0;

	// Token: 0x040001D3 RID: 467
	private readonly string Method;

	// Token: 0x040001D4 RID: 468
	private readonly string Path;

	// Token: 0x040001D5 RID: 469
	private readonly Dictionary<string, string> Params;
}
