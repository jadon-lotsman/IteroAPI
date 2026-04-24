using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mnemo.Common;
using Mnemo.Data;
using Mnemo.Data.Entities;
using Mnemo.Services.Queries;

namespace Mnemo.Services
{
    public class AccountManagementService
    {
        private AppDbContext _context;

        private AccountQueries _accountQueries;


        public AccountManagementService(AppDbContext context, AccountQueries accountQueries)
        {
            _context = context;
            _accountQueries = accountQueries;
        }



        public async Task<RequestResult<bool>> CreateAsync(string username)
        {
            if (await _accountQueries.ExistsByUsernameAsync(username))
                return RequestResult<bool>.Failure(ErrorCode.UsernameTaken);

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
