using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using DataContract;
using System.Linq;

namespace DataAccess
{
    public partial class SQLContext : DbContext, IUserDataRepository
    {
        public DbSet<User> Users { get; set; }
        public SQLContext()
        {
            Database.Migrate();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>().HasData(
                new User()
                {
                    Id = 1,
                    UserName = "Test",
                    Password = "a",
                }
            );
            base.OnModelCreating(modelBuilder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Torpedo;Trusted_Connection=True");
        }

        #region UserRepository
        public User GetUserById(int id)
        {
            return Users.FirstOrDefault(x => x.Id == id);
        }

        public List<User> GetUsers(GetUsersDataRequest requestData)
        {
            return Users.Skip(requestData.PageSize * requestData.PageNum).Take(requestData.PageSize).ToList();
        }

        public User Login(string username, string password)
        {
            return Users.FirstOrDefault(x => x.UserName == username && x.Password == password);
        }

        public int CountUsers()
        {
            return Users.Count();
        }

        public User Register(UserData requestData)
        {
            if (Users.FirstOrDefault(x => x.UserName == requestData.UserName) != null)
            {
                return null;
            }
            var newUser = Users.Add(new User()
            {
                Password = requestData.Password,
                UserName = requestData.UserName,
            }).Entity;
            SaveChanges();
            return newUser;

        }
        #endregion
    }
}
