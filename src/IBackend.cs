public interface IBackend
{
	BWAPIRequestBase CreateRequest(string method, string path);
}
