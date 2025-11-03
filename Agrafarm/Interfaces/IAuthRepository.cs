using Agrafarm.Model;

namespace Agrafarm.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User> GetByRefreshTokenAsync(string refreshToken);
        Task AddUserAsync(User user);
        Task DeleteOldTokensAsync(int userId);

        Task SaveAsync();
    }
}
