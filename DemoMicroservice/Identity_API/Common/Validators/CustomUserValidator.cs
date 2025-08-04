using Microsoft.AspNetCore.Identity;

namespace Identity_API.Common.Validators
{
    public class CustomUserValidator<TUser> : IUserValidator<TUser> where TUser : class
    {
        public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
        {
            var errors = new List<IdentityError>();
            
            // Example of custom user validation
            // You can add more custom validation logic here
            
            return Task.FromResult(errors.Count == 0 
                ? IdentityResult.Success 
                : IdentityResult.Failed(errors.ToArray()));
        }
    }
}
