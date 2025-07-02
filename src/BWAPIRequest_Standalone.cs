using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

public class BWAPIRequest_Standalone : BWAPIRequestBase
{
	private string Method;

	private string Path;

	private WWWForm form;

	private Dictionary<string, string> headers;

	private string jsonParamStr;

	public BWAPIRequest_Standalone(string method, string path)
	{
		Method = validHttpMethod(method);
		Path = path;
		headers = new Dictionary<string, string>();
	}

	private static string currentUserAuthToken()
	{
		string result = string.Empty;
		if (BWUser.currentUser != null)
		{
			result = BWUser.currentUser.authToken;
		}
		return result;
	}

	public static string ApiBaseUrl()
	{
		return BWEnvConfig.API_BASE_URL;
	}

	public override void AddParam(string key, string valueStr)
	{
		if (Method == "GET" || Method == "DELETE")
		{
			string text = ((!Path.Contains("?")) ? "?" : "&");
			Path = Path + text + key + "=" + valueStr;
		}
		else
		{
			if (form == null)
			{
				form = new WWWForm();
			}
			BWLog.Info(key + " = " + valueStr);
			form.AddField(key, valueStr);
		}
	}

	public override void AddJsonParameters(string jsonStr)
	{
		jsonParamStr = jsonStr;
	}

	public override void AddImageData(string key, byte[] data, string filename, string mimeType)
	{
		if (form == null)
		{
			form = new WWWForm();
		}
		form.AddBinaryData(key, data, filename, mimeType);
	}

	public override void Send()
	{
		Blocksworld.bw.StartCoroutine(SendCoroutine());
	}

	public override void SendOwnerCoroutine(MonoBehaviour owner)
	{
		owner.StartCoroutine(SendCoroutine());
	}

	private IEnumerator SendCoroutine()
	{
		string text = ApiBaseUrl() + Path;
		UnityWebRequest request = null;
		switch (Method)
		{
		case "DELETE":
			request = UnityWebRequest.Delete(text);
			break;
		case "PUT":
			if (!string.IsNullOrEmpty(jsonParamStr))
			{
				request = UnityWebRequest.Put(text, jsonParamStr);
				request.SetRequestHeader("Content-Type", "application/json");
			}
			else
			{
				request = ((form == null) ? UnityWebRequest.Put(text, string.Empty) : UnityWebRequest.Put(text, form.data));
			}
			break;
		case "POST":
			if (!string.IsNullOrEmpty(jsonParamStr))
			{
				request = UnityWebRequest.Post(text, jsonParamStr);
				byte[] bytes = Encoding.UTF8.GetBytes(jsonParamStr);
				request.uploadHandler = new UploadHandlerRaw(bytes);
				request.downloadHandler = new DownloadHandlerBuffer();
				request.SetRequestHeader("Content-Type", "application/json");
			}
			else
			{
				request = ((form == null) ? UnityWebRequest.Post(text, string.Empty) : UnityWebRequest.Post(text, form));
			}
			break;
		case "GET":
			request = UnityWebRequest.Get(text);
			break;
		}
		if (request == null)
		{
			BWLog.Error("Invalid request");
			yield break;
		}
		BWLog.Info("Requesting " + text);
		AddHeaderAppVersion();
		AddHeaderAuthToken();
		AddHeaderClientType();
		if (form != null)
		{
			foreach (KeyValuePair<string, string> header in form.headers)
			{
				headers.Add(header.Key, header.Value);
			}
		}
		foreach (KeyValuePair<string, string> header2 in headers)
		{
			request.SetRequestHeader(header2.Key, header2.Value);
		}
		request.Send();
		if (request.downloadHandler != null)
		{
			while (!request.downloadHandler.isDone)
			{
				if (request.isError && (request.error == "Cannot connect to destination host" || request.error == "Unknown Error"))
				{
					BWLog.Warning("Request error (download handler): " + request.error + " (status code is " + request.responseCode + ")");
					BWEnvConfig.RevertAPIServer();
					yield return new WaitForSeconds(1f);
					yield return SendCoroutine();
					yield break;
				}
				yield return null;
			}
		}
		else
		{
			while (!request.isDone)
			{
				if (request.isError && (request.error == "Cannot connect to destination host" || request.error == "Unknown Error"))
				{
					BWLog.Warning("Request error (no download handler): " + request.error);
					BWEnvConfig.RevertAPIServer();
					yield return new WaitForSeconds(1f);
					yield return SendCoroutine();
					yield break;
				}
				yield return null;
			}
		}
		if (request.isError)
		{
			BWLog.Error("UnityWebRequest error: " + request.error);
		}
		JObject jObject = null;
		if (request.downloadHandler != null && !string.IsNullOrEmpty(request.downloadHandler.text))
		{
			bool flag = false;
			try
			{
				jObject = JSONDecoder.Decode(request.downloadHandler.text);
			}
			catch (ParseError parseError)
			{
				flag = true;
				BWLog.Warning("error decoding json: " + parseError.ToString());
			}
			if (flag)
			{
				BWEnvConfig.RevertAPIServer();
				yield return SendCoroutine();
				yield break;
			}
		}
		else if (string.IsNullOrEmpty(request.downloadHandler.text))
		{
			BWLog.Warning("empty download handler");
			BWEnvConfig.RevertAPIServer();
			yield return SendCoroutine();
			yield break;
		}
		if (request.responseCode >= 200 && request.responseCode < 300)
		{
			if (onSuccess != null)
			{
				if (jObject != null)
				{
					onSuccess(jObject);
				}
				else
				{
					onSuccess(null);
				}
			}
		}
		else if (onFailure != null)
		{
			string text2 = ((jObject != null && jObject.ContainsKey("error")) ? jObject["error"].ToString() : ((request.error != null) ? request.error : "<no message>"));
			BWLog.Info("BWAPIRequest failed with: " + request.responseCode + " -- message: " + text2 + "\nFrom: " + request.method + " " + request.url);
			BWAPIRequestError bWAPIRequestError = new BWAPIRequestError();
			bWAPIRequestError.title = "Network Error";
			bWAPIRequestError.message = text2;
			bWAPIRequestError.httpStatusCode = (int)request.responseCode;
			if (jObject != null && jObject.ContainsKey("error_details"))
			{
				bWAPIRequestError.responseBodyJson = jObject["error_details"];
			}
			else if (jObject != null && jObject.ContainsKey("error_message"))
			{
				bWAPIRequestError.message = jObject["error_message"].ToString();
			}
			onFailure(bWAPIRequestError);
		}
	}

	private int ExtractHTTPStatusCode(Dictionary<string, string> responseHeaders)
	{
		int result = 0;
		if (responseHeaders != null && responseHeaders.ContainsKey("STATUS"))
		{
			string[] array = responseHeaders["STATUS"].Split(' ');
			if (array.Length >= 2)
			{
				int.TryParse(array[1], out result);
			}
		}
		return result;
	}

	private void AddHeaderMethodOverride()
	{
		if (Method == "PUT" || Method == "DELETE")
		{
			headers["_method"] = Method;
		}
	}

	private void AddHeaderAppVersion()
	{
		headers["BW-App-Version"] = BWEnvConfig.BLOCKSWORLD_VERSION;
	}

	private void AddHeaderAuthToken()
	{
		string value = currentUserAuthToken();
		if (!string.IsNullOrEmpty(value))
		{
			headers["BW-Auth-Token"] = value;
		}
	}

	private void AddHeaderClientType()
	{
		string value = "Windows";
		headers["BW-Client-Type"] = value;
	}

	private string validHttpMethod(string method)
	{
		string text = method.ToUpper();
		switch (text)
		{
		default:
			throw new InvalidOperationException("Unexpected HTTP method '" + method + "'. Expected one of 'GET', 'POST', 'PUT' or 'DELETE'.");
		case "GET":
		case "POST":
		case "PUT":
		case "DELETE":
			return text;
		}
	}

	public static JObject BlockingGetRequest(string path, ref string errorMsg)
	{
		string text = ApiBaseUrl() + path;
		WWW wWW = new WWW(text);
		DateTime now = DateTime.Now;
		while (!wWW.isDone)
		{
			Thread.Sleep(100);
			if (DateTime.Now.Subtract(now).TotalSeconds > 10.0)
			{
				break;
			}
		}
		if (!wWW.isDone)
		{
			wWW.Dispose();
			errorMsg = "Failed request " + text + ". Error: The request timed out.";
			return null;
		}
		if (!string.IsNullOrEmpty(wWW.error))
		{
			errorMsg = "Failed request " + text + ". Error: " + wWW.error;
			return null;
		}
		JObject result = JSONDecoder.Decode(wWW.text);
		errorMsg = string.Empty;
		return result;
	}
}
