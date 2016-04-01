namespace DatabaseServer
{
    public interface IRequestResponseHandler
    {
        string HandleRequest(string request);
    }
}
