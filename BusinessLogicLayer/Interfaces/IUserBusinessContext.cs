using System;
using System.Collections.Generic;
using System.Text;
using DataContract;

namespace BusinessLogicLayer
{
    public interface IUserBusinessContext : IDisposable
    {
        GetUsersDataResult GetUsersListed(GetUsersDataRequest requestData);
        UserData GetUserById(int id);
        UserData Login(string userName, string password);
    }
}
