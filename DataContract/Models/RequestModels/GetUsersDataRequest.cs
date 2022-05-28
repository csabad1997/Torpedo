using System;
using System.Collections.Generic;
using System.Text;

namespace DataContract
{
    public class GetUsersDataRequest : IPagedRequest
    {
        public int PageNum { get; set; }
        public int PageSize { get; set; }
    }
}
