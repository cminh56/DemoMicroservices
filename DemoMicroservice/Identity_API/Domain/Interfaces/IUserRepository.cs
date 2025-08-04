using Identity_API.Common.DTO.Auth;
using Identity_API.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Identity_API.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);
        Task<ApplicationUser?> FindByEmailAsync(string email);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<UserProfileResponse> GetUserProfileAsync(string userId);
        Task<ApplicationUser?> FindByIdAsync(string userId);
    }
}
