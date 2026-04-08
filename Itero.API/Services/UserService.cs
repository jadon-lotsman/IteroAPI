using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Itero.API.Data;
using Itero.API.Data.Entities;

namespace Itero.API.Services
{
    public class UserService
    {
        private AppDbContext _context;


        public UserService(AppDbContext context)
        {
            _context = context;
        }


        public User? GetById(int userId)
        {
            return _context.Users.FirstOrDefault(u => u.Id == userId);
        }

        public User? GetByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }

        public void Create(string username)
        {
            _context.Users.Add(new User(username));

            _context.SaveChanges();
        }
    }
}
