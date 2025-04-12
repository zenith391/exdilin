using System;

// Token: 0x02000251 RID: 593
public interface IBackend
{
	// Token: 0x06001B1D RID: 6941
	BWAPIRequestBase CreateRequest(string method, string path);
}
