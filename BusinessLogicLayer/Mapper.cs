using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DataContract;
using DataAccess;

namespace BusinessLogicLayer
{
    public static class Mapper
    {
        public static UserData UserToUserData(User user)
        {
            if (user == null)
            {
                return null;
            }
            return new UserData()
            {
                Id = user.Id,
                UserName = user.UserName,
            };
        }
        public static List<UserData> UserListToUserDataList(List<User> users)
        {
            return users.Select(x => UserToUserData(x)).ToList();
        }
    }
}
