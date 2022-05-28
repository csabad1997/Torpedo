using System;
using System.Collections.Generic;
using System.Text;

namespace DataContract
{
    public class GetUsersDataResult : IPagedResult<UserData>
    {
        public int PageCount { get; set; }
        public int CurrentPage { get; set; }
        public List<UserData> Items { get; set; }
    }
}
