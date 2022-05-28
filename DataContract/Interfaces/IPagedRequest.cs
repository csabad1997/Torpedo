using System;
using System.Collections.Generic;
using System.Text;

namespace DataContract
{
    public interface IPagedRequest
    {
        int PageNum { get; set; }
        int PageSize { get; set; }
    }
}
