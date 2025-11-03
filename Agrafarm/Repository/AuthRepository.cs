using Agrafarm.Data;
using Agrafarm.Interfaces;
using Agrafarm.Model;
using Microsoft.EntityFrameworkCore;

namespace Agrafarm.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _context;
        public AuthRepository(ApplicationDbContext context) => _context = context;

        public async Task<User?> GetByUsernameAsync(string username) =>
            await _context.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Username == username);

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }


        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshToken));
        }

        public async Task DeleteOldTokensAsync(int userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Created)
                .ToListAsync();

            // Keep only latest 5 tokens
            var tokensToRemove = tokens.Skip(5).Where(t => !t.IsActive || t.IsExpired).ToList();

            if (tokensToRemove.Any())
            {
                _context.RefreshTokens.RemoveRange(tokensToRemove);
                await _context.SaveChangesAsync();
            }
        }




        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}
