using System;

// Token: 0x02000252 RID: 594
public class BWAPI_Standalone : IBackend
{
	// Token: 0x06001B1F RID: 6943 RVA: 0x000C6649 File Offset: 0x000C4A49
	public BWAPIRequestBase CreateRequest(string method, string path)
	{
		return new BWAPIRequest_Standalone(method, path);
	}
}
