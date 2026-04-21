using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mnemo.Common;
using Mnemo.Data;
using Mnemo.Data.Entities;

namespace Mnemo.Services
{
    public class AccountManagementService
    {
        private AppDbContext _context;


        public AccountManagementService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<User?> GetByIdAsync(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => string.Equals(u.Username, username));
        }


        public async Task<RequestResult<bool>> CreateAsync(string username)
        {
            if (await GetByUsernameAsync(username) != null)
                return RequestResult<bool>.Failure("USERNAME_TAKEN");

            //if (string.IsNullOrWhiteSpace(password))
            //    return RequestResult<bool>.Failure("INVALID_PASSWORD");


            var user = new User
            {
                Username = username,
                RegisteredAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return RequestResult<bool>.Success(true);
        }
    }
}
