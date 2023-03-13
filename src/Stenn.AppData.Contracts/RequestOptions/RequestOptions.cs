namespace Stenn.AppData.Contracts.RequestOptions
{
    public class RequestOptions : IAppDataRequestOptions
    {
        public Filter? Filter { get; set; }
        public SortOptions? SortOptions { get; set; }
        public PagingOptions? Paging { get; set; }
    }
}
