using System.Collections.Generic;

namespace Stenn.AppData.Contracts.RequestOptions
{
    public class SortOptions : List<SortItem>
    {
    }

    public class SortItem
    {
        public string FieldName { get; set; }
        public SortDirection SortDirection { get; set; }
    }

    public enum SortDirection
    {
        Ascending = 0,
        Descending = 1
    }
}
