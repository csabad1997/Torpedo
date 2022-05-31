using DataContract;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DataAccess;

namespace BusinessLogicLayer
{
    public partial class BusinessLogicContext : IUserBusinessContext
    {
        public UserData GetUserById(int id)
        {
            return Mapper.UserToUserData(((IUserDataRepository)context).GetUserById(id));
        }

        public GetUsersDataResult GetUsersListed(GetUsersDataRequest requestData)
        {
            var data = ((IUserDataRepository)context).GetUsers(requestData);
            int count = ((IUserDataRepository)context).CountUsers();
            return new GetUsersDataResult()
            {
                CurrentPage = 0,
                Items = data.Select(x => Mapper.UserToUserData(x)).ToList(),
                PageCount = count / requestData.PageSize + (count % requestData.PageSize > 0 ? 1 : 0)
            };
        }

        public UserData Login(string userName, string password)
        {
            return Mapper.UserToUserData(((IUserDataRepository)context).Login(userName, password));
        }
        public UserData Register(UserData requestData)
        {
            return Mapper.UserToUserData(((IUserDataRepository)context).Register(requestData));
        }
    }
}
