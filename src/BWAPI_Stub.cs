using System;

// Token: 0x02000253 RID: 595
public class BWAPI_Stub : IBackend
{
	// Token: 0x06001B21 RID: 6945 RVA: 0x000C665A File Offset: 0x000C4A5A
	public BWAPIRequestBase CreateRequest(string method, string path)
	{
		return new BWAPIRequest_Stub(method, path);
	}
}
