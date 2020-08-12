using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class Seed
    {
        //public static void SeedUsers(DataContext context)
        //{
        //    if (!context.Users.Any())
        //    {
        //        var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
        //        var users = JsonConvert.DeserializeObject<List<User>>(userData);

        //        foreach (var user in users)
        //        {
        //            byte[] passwordhash, passwordSalt;
        //            CreatePasswordHash("password", out passwordhash, out passwordSalt);

        //            user.PasswordHash = passwordhash;
        //            user.PasswordSalt = passwordSalt;
        //            user.Username = user.Username.ToLower();
        //            context.Users.Add(user);
        //        }
        //        context.SaveChanges();
        //    }
        //}

        public static void SeedUsers(DataContext context)
        {
            if (!context.Users.Any())
            {
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<User>>(userData);
                foreach (var user in users)
                {
                    byte[] passwordhash, passwordsalt;
                    CreatePasswordHash("password", out passwordhash, out passwordsalt);

                    user.PasswordHash = passwordhash;
                    user.PasswordSalt = passwordsalt;
                    user.Username = user.Username.ToLower();
                    context.Users.Add(user);
                }

                context.SaveChanges();
            }
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            }
        }
    }
}
