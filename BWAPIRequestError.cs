using System;
using SimpleJSON;

// Token: 0x02000035 RID: 53
public class BWAPIRequestError
{
	// Token: 0x1700003E RID: 62
	// (get) Token: 0x060001DC RID: 476 RVA: 0x0000A61C File Offset: 0x00008A1C
	// (set) Token: 0x060001DD RID: 477 RVA: 0x0000A659 File Offset: 0x00008A59
	public JObject responseBodyJson
	{
		get
		{
			if (this._responseBodyJson != null)
			{
				return this._responseBodyJson;
			}
			if (string.IsNullOrEmpty(this.responseBodyStr))
			{
				return null;
			}
			this._responseBodyJson = JSONDecoder.Decode(this.responseBodyStr);
			return this._responseBodyJson;
		}
		set
		{
			this._responseBodyJson = value;
		}
	}

	// Token: 0x060001DE RID: 478 RVA: 0x0000A664 File Offset: 0x00008A64
	public static BWAPIRequestError BuildWithJsonStr(string errorJsonStr)
	{
		JObject json = JSONDecoder.Decode(errorJsonStr);
		return BWAPIRequestError.BuildWithJsonObj(json);
	}

	// Token: 0x060001DF RID: 479 RVA: 0x0000A680 File Offset: 0x00008A80
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

	// Token: 0x040001CC RID: 460
	public string title;

	// Token: 0x040001CD RID: 461
	public string message;

	// Token: 0x040001CE RID: 462
	public int httpStatusCode;

	// Token: 0x040001CF RID: 463
	private string responseBodyStr;

	// Token: 0x040001D0 RID: 464
	private JObject _responseBodyJson;
}
