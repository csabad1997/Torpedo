using System;
using System.Collections.Generic;
using System.Text;

namespace DataContract
{
    public interface IPagedResult<T>
    {
        int PageCount { get; set; }
        int CurrentPage { get; set; }
        List<T> Items { get; set; }
    }
}
