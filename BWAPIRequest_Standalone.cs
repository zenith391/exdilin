using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

// Token: 0x02000037 RID: 55
public class BWAPIRequest_Standalone : BWAPIRequestBase
{
    // Token: 0x060001EA RID: 490 RVA: 0x0000A9A9 File Offset: 0x00008DA9
    public BWAPIRequest_Standalone(string method, string path)
    {
        this.Method = this.validHttpMethod(method);
        this.Path = path;
        this.headers = new Dictionary<string, string>();
    }

    // Token: 0x060001EB RID: 491 RVA: 0x0000A9D0 File Offset: 0x00008DD0
    private static string currentUserAuthToken()
    {
        string result = string.Empty;
        if (BWUser.currentUser != null)
        {
            result = BWUser.currentUser.authToken;
        }
        return result;
    }

    // Token: 0x060001EC RID: 492 RVA: 0x0000A9F9 File Offset: 0x00008DF9
    public static string ApiBaseUrl()
    {
        return BWEnvConfig.API_BASE_URL;
    }

    // Token: 0x060001ED RID: 493 RVA: 0x0000AA00 File Offset: 0x00008E00
    public override void AddParam(string key, string valueStr)
    {
        if (this.Method == "GET" || this.Method == "DELETE")
        {
            string text = (!this.Path.Contains("?")) ? "?" : "&";
            this.Path = string.Concat(new string[]
            {
                this.Path,
                text,
                key,
                "=",
                valueStr
            });
            return;
        }
        if (this.form == null) {
            this.form = new WWWForm();
        }
        BWLog.Info(key + " = " + valueStr);
        this.form.AddField(key, valueStr);
    }

    // Token: 0x060001EE RID: 494 RVA: 0x0000AAB2 File Offset: 0x00008EB2
    public override void AddJsonParameters(string jsonStr)
    {
        this.jsonParamStr = jsonStr;
    }

    // Token: 0x060001EF RID: 495 RVA: 0x0000AABB File Offset: 0x00008EBB
    public override void AddImageData(string key, byte[] data, string filename, string mimeType)
    {
        if (this.form == null)
        {
            this.form = new WWWForm();
        }
        this.form.AddBinaryData(key, data, filename, mimeType);
    }

    // Token: 0x060001F0 RID: 496 RVA: 0x0000AAE3 File Offset: 0x00008EE3
    public override void Send()
    {
        Blocksworld.bw.StartCoroutine(this.SendCoroutine());
    }

    // Token: 0x060001F1 RID: 497 RVA: 0x0000AAF6 File Offset: 0x00008EF6
    public override void SendOwnerCoroutine(MonoBehaviour owner)
    {
        owner.StartCoroutine(this.SendCoroutine());
    }

    // Token: 0x060001F2 RID: 498 RVA: 0x0000AB08 File Offset: 0x00008F08
    private IEnumerator SendCoroutine()
    {
        string url = BWAPIRequest_Standalone.ApiBaseUrl() + this.Path;
        UnityWebRequest request = null;
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
                            request = UnityWebRequest.Delete(url);
                        }
                    }
                    else if (!string.IsNullOrEmpty(this.jsonParamStr))
                    {
                        request = UnityWebRequest.Put(url, this.jsonParamStr);
                        request.SetRequestHeader("Content-Type", "application/json");
                    }
                    else if (this.form != null)
                    {
                        request = UnityWebRequest.Put(url, this.form.data);
                    }
                    else
                    {
                        request = UnityWebRequest.Put(url, string.Empty);
                    }
                }
                else if (!string.IsNullOrEmpty(this.jsonParamStr))
                {
                    request = UnityWebRequest.Post(url, this.jsonParamStr);
                    byte[] bytes = Encoding.UTF8.GetBytes(this.jsonParamStr);
                    request.uploadHandler = new UploadHandlerRaw(bytes);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");
                }
                else if (this.form != null)
                {
                    request = UnityWebRequest.Post(url, this.form);
                }
                else
                {
                    request = UnityWebRequest.Post(url, string.Empty);
                }
            }
            else
            {
                request = UnityWebRequest.Get(url);
            }
        }
        if (request == null)
        {
            BWLog.Error("Invalid request");
            yield break;
        }
        BWLog.Info("Requesting " + url);
        this.AddHeaderAppVersion();
        this.AddHeaderAuthToken();
        this.AddHeaderClientType();
        if (this.form != null)
        {
            foreach (KeyValuePair<string, string> keyValuePair in this.form.headers)
            {
                this.headers.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }
        foreach (KeyValuePair<string, string> keyValuePair2 in this.headers)
        {
            request.SetRequestHeader(keyValuePair2.Key, keyValuePair2.Value);
        }
        AsyncOperation send = request.Send();
        if (request.downloadHandler != null)
        {
            while (!request.downloadHandler.isDone)
            {
                if (request.isError && (request.error == "Cannot connect to destination host" || request.error == "Unknown Error"))
                {
					BWLog.Warning("Request error (download handler): " + request.error + " (status code is " + request.responseCode + ")");
                    BWEnvConfig.RevertAPIServer();
                    yield return new WaitForSeconds(1f);
                    yield return SendCoroutine(); // repost request
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
        JObject responseJson = null;
        if (request.downloadHandler != null && !string.IsNullOrEmpty(request.downloadHandler.text))
        {
			bool err = false;
			try {
				responseJson = JSONDecoder.Decode(request.downloadHandler.text);
			} catch (ParseError e) {
				err = true;
				BWLog.Warning("error decoding json: " + e.ToString());
			}
			if (err) {
				BWEnvConfig.RevertAPIServer();
				yield return SendCoroutine();
				yield break;
			}
		} else if (string.IsNullOrEmpty(request.downloadHandler.text)) {
			BWLog.Warning("empty download handler");
			BWEnvConfig.RevertAPIServer();
			yield return SendCoroutine();
			yield break;
		}
		if (request.responseCode >= 200L && request.responseCode < 300L)
        {
            if (this.onSuccess != null)
            {
                if (responseJson != null)
                {
                    this.onSuccess(responseJson);
                }
                else
                {
                    this.onSuccess(null);
                }
            }
        }
        else if (this.onFailure != null)
        {
            string text = (responseJson == null || !responseJson.ContainsKey("error")) ? ((request.error != null) ? request.error : "<no message>") : responseJson["error"].ToString();
            BWLog.Info(string.Concat(new object[]
            {
                "BWAPIRequest failed with: ",
                request.responseCode,
                " -- message: ",
                text,
                "\nFrom: ",
                request.method,
                " ",
                request.url
            }));
            BWAPIRequestError bwapirequestError = new BWAPIRequestError();
            bwapirequestError.title = "Network Error";
            bwapirequestError.message = text;
            bwapirequestError.httpStatusCode = (int)request.responseCode;
            if (responseJson != null && responseJson.ContainsKey("error_details"))
            {
                bwapirequestError.responseBodyJson = responseJson["error_details"];
            } else if (responseJson != null && responseJson.ContainsKey("error_message")) {
				bwapirequestError.message = responseJson["error_message"].ToString();
			}
			this.onFailure(bwapirequestError);
        }
        yield break;
    }

    // Token: 0x060001F3 RID: 499 RVA: 0x0000AB24 File Offset: 0x00008F24
    private int ExtractHTTPStatusCode(Dictionary<string, string> responseHeaders)
    {
        int result = 0;
        if (responseHeaders != null && responseHeaders.ContainsKey("STATUS"))
        {
            string[] array = responseHeaders["STATUS"].Split(new char[]
            {
                ' '
            });
            if (array.Length >= 2)
            {
                int.TryParse(array[1], out result);
            }
        }
        return result;
    }

    // Token: 0x060001F4 RID: 500 RVA: 0x0000AB7C File Offset: 0x00008F7C
    private void AddHeaderMethodOverride()
    {
        if (this.Method == "PUT" || this.Method == "DELETE")
        {
            this.headers["_method"] = this.Method;
        }
    }

    // Token: 0x060001F5 RID: 501 RVA: 0x0000ABC9 File Offset: 0x00008FC9
    private void AddHeaderAppVersion()
    {
        this.headers["BW-App-Version"] = BWEnvConfig.BLOCKSWORLD_VERSION;
    }

    // Token: 0x060001F6 RID: 502 RVA: 0x0000ABE0 File Offset: 0x00008FE0
    private void AddHeaderAuthToken()
    {
        string value = BWAPIRequest_Standalone.currentUserAuthToken();
        if (!string.IsNullOrEmpty(value))
        {
            this.headers["BW-Auth-Token"] = value;
        }
    }

    // Token: 0x060001F7 RID: 503 RVA: 0x0000AC10 File Offset: 0x00009010
    private void AddHeaderClientType()
    {
        string value = "Windows";
        this.headers["BW-Client-Type"] = value;
    }

    // Token: 0x060001F8 RID: 504 RVA: 0x0000AC34 File Offset: 0x00009034
    private string validHttpMethod(string method)
    {
        string text = method.ToUpper();
        if (!(text == "GET") && !(text == "POST") && !(text == "PUT") && !(text == "DELETE"))
        {
            throw new InvalidOperationException("Unexpected HTTP method '" + method + "'. Expected one of 'GET', 'POST', 'PUT' or 'DELETE'.");
        }
        return text;
    }

    // Token: 0x060001F9 RID: 505 RVA: 0x0000ACA0 File Offset: 0x000090A0
    public static JObject BlockingGetRequest(string path, ref string errorMsg) // edited try fix
    {
        string text = BWAPIRequest_Standalone.ApiBaseUrl() + path;
        WWW www = new WWW(text);
        DateTime now = DateTime.Now;
        while (!www.isDone)
        {
            Thread.Sleep(100);
            if (DateTime.Now.Subtract(now).TotalSeconds > 10.0)
            {
                break;
            }
        }
        if (!www.isDone)
        {
            www.Dispose();
            errorMsg = "Failed request " + text + ". Error: The request timed out.";
            return null;
        }
        if (!string.IsNullOrEmpty(www.error))
        {
            errorMsg = "Failed request " + text + ". Error: " + www.error;
            return null;
        }
        JObject rst = JSONDecoder.Decode(www.text);
        errorMsg = string.Empty;
        return rst;
    }

    // Token: 0x040001D6 RID: 470
    private string Method;

    // Token: 0x040001D7 RID: 471
    private string Path;

    // Token: 0x040001D8 RID: 472
    private WWWForm form;

    // Token: 0x040001D9 RID: 473
    private Dictionary<string, string> headers;

    // Token: 0x040001DA RID: 474
    private string jsonParamStr;
}
