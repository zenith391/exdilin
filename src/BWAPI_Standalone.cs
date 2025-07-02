public class BWAPI_Standalone : IBackend
{
	public BWAPIRequestBase CreateRequest(string method, string path)
	{
		return new BWAPIRequest_Standalone(method, path);
	}
}
