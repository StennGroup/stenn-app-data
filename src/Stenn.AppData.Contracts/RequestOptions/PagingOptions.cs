namespace Stenn.AppData.Contracts.RequestOptions
{
    public class PagingOptions
    {
        public int PageSize { get; }
        public int PageNumber { get; }

        public PagingOptions()
        {
            PageNumber = 1;
            PageSize = 10;
        }

        public PagingOptions(int pageSize, int pageNumber)
        {
            PageNumber = pageNumber < 1 ? 1 : pageNumber;
            PageSize = pageSize;
        }
    }
}
