public class BWAPI_Stub : IBackend
{
	public BWAPIRequestBase CreateRequest(string method, string path)
	{
		return new BWAPIRequest_Stub(method, path);
	}
}
