using System;
using System.Collections.Generic;
using SimpleJSON;

public class BWAPIRequest_IOS : BWAPIRequestBase
{
	private static readonly Dictionary<int, BWAPIRequest_IOS> Requests = new Dictionary<int, BWAPIRequest_IOS>();

	private static int LastRequestId = 0;

	private readonly string Method;

	private readonly string Path;

	private readonly Dictionary<string, string> Params;

	public BWAPIRequest_IOS(string method, string path)
	{
		Method = validHttpMethod(method);
		Path = path;
		Params = new Dictionary<string, string>();
	}

	private static int RegisterRequest(BWAPIRequest_IOS request)
	{
		int num = ++LastRequestId;
		if (Requests.ContainsKey(num))
		{
			throw new InvalidOperationException("Requests already contains id " + num);
		}
		Requests[num] = request;
		return num;
	}

	private static BWAPIRequest_IOS CompleteRequest(int requestId)
	{
		if (!Requests.ContainsKey(requestId))
		{
			throw new InvalidOperationException("Failed to find request with id " + requestId);
		}
		BWAPIRequest_IOS result = Requests[requestId];
		Requests.Remove(requestId);
		return result;
	}

	public static void RequestSucceeded(int requestId, string responseJsonStr)
	{
		BWAPIRequest_IOS bWAPIRequest_IOS = CompleteRequest(requestId);
		if (bWAPIRequest_IOS.onSuccess != null)
		{
			JObject responseJson = JSONDecoder.Decode(responseJsonStr);
			bWAPIRequest_IOS.onSuccess(responseJson);
		}
	}

	public static void RequestFailed(int requestId, string errorJsonStr)
	{
		BWAPIRequest_IOS bWAPIRequest_IOS = CompleteRequest(requestId);
		if (bWAPIRequest_IOS.onFailure != null)
		{
			BWAPIRequestError error = BWAPIRequestError.BuildWithJsonStr(errorJsonStr);
			bWAPIRequest_IOS.onFailure(error);
		}
	}

	public override void AddParam(string key, string valueStr)
	{
		if (Params.ContainsKey(key))
		{
			throw new InvalidOperationException("Attempt to add duplicate param key " + key + " with value " + valueStr);
		}
		Params[key] = valueStr;
	}

	public override void Send()
	{
		int requestId = RegisterRequest(this);
		switch (Method)
		{
		case "DELETE":
			IOSInterface.ApiDelete(requestId, Path);
			break;
		case "PUT":
			IOSInterface.ApiPut(requestId, Path, ParamsAsJsonStr());
			break;
		case "POST":
			IOSInterface.ApiPost(requestId, Path, ParamsAsJsonStr());
			break;
		case "GET":
			IOSInterface.ApiGet(requestId, Path, ParamsAsJsonStr());
			break;
		}
	}

	private string ParamsAsJsonStr()
	{
		if (Params.Count == 0)
		{
			return null;
		}
		return JSONEncoder.Encode(Params);
	}

	private string validHttpMethod(string method)
	{
		string text = method.ToUpper();
		if (text != "GET" && text != "POST" && text != "PUT" && text != "DELETE")
		{
			throw new InvalidOperationException("Unexpected HTTP method '" + method + "'. Expected one of 'GET', 'POST', 'PUT' or 'DELETE'.");
		}
		return text;
	}
}
