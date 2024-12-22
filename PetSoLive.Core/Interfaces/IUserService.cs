// Interfaces (Abstraction for services and repositories)
using PetSoLive.Core.Entities;
// Core Layer Updates
// Update IUserService with methods for authentication and registration
namespace PetSoLive.Core.Interfaces
{
    public interface IUserService
    {
        Task<User> AuthenticateAsync(string username, string password);
        Task RegisterAsync(User user);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User> GetUserByIdAsync(int userId);  // Change to accept an int
        Task UpdateUserAsync(User user);
    }
}
