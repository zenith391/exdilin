using SimpleJSON;

public class BWAPIRequestError
{
	public string title;

	public string message;

	public int httpStatusCode;

	private string responseBodyStr;

	private JObject _responseBodyJson;

	public JObject responseBodyJson
	{
		get
		{
			if (_responseBodyJson != null)
			{
				return _responseBodyJson;
			}
			if (string.IsNullOrEmpty(responseBodyStr))
			{
				return null;
			}
			_responseBodyJson = JSONDecoder.Decode(responseBodyStr);
			return _responseBodyJson;
		}
		set
		{
			_responseBodyJson = value;
		}
	}

	public static BWAPIRequestError BuildWithJsonStr(string errorJsonStr)
	{
		JObject json = JSONDecoder.Decode(errorJsonStr);
		return BuildWithJsonObj(json);
	}

	public static BWAPIRequestError BuildWithJsonObj(JObject json)
	{
		return new BWAPIRequestError
		{
			title = json["title"].StringValue,
			message = json["message"].StringValue,
			httpStatusCode = json["http_status_code"].IntValue,
			responseBodyStr = json["response_body_str"].StringValue
		};
	}
}
