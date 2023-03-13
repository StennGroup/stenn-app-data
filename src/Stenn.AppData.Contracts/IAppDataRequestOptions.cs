using Stenn.AppData.Contracts.RequestOptions;

namespace Stenn.AppData.Contracts
{
    public interface IAppDataRequestOptions
    {
        //Filter options
        Filter? Filter { get; set; }

        //Paging options
        PagingOptions? Paging { get; set; }

        //Sorting options
        SortOptions? SortOptions { get; set; }
    }
}
