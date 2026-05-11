using System.Collections.Generic;
using System.Linq;
using Task_1.Models;

namespace Task_1.Services
{
    public static class AuthService
    {
        public static User Authenticate(string username, string password, List<User> users)
        {
            return users.FirstOrDefault(u => u.Username == username && u.PasswordHash == password);
        }

        public static void Register(List<User> users, User newUser)
        {
            if (!users.Any(u => u.Username == newUser.Username))
                users.Add(newUser);
        }
    }
}