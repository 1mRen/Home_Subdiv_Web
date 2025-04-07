using System.Threading.Tasks;
using Home_Sbdv.Data;
using Home_Sbdv.Entities;
using Microsoft.EntityFrameworkCore;

namespace Home_Sbdv.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Users> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == usernameOrEmail.ToLower() ||
                                         u.Email.ToLower() == usernameOrEmail.ToLower());
        }
    }
}