namespace Stenn.AppData.Contracts
{
    public interface IAppDataRequest
    {
        IAppDataRequestOptions? RequestOptions { get; set; }
    }
}
