using System;
using System.Collections.Generic;
using System.Text;
using DataContract;

namespace DataAccess
{
    public interface IUserDataRepository : IDisposable
    {
        List<User> GetUsers(GetUsersDataRequest requestData);
        int CountUsers();
        User GetUserById(int id);
        User Login(string username, string password);
        User Register(UserData requestData);
    }
}
