using Microsoft.AspNetCore.Identity;

namespace Identity_API.Common.Validators
{
    public class CustomPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : class
    {
        public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            var errors = new List<IdentityError>();

            // Example of custom password validation
            if (password.ToLower().Contains("password"))
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordContainsWord",
                    Description = "Password cannot contain the word 'password'"
                });
            }

            // Add more custom validations as needed

            return Task.FromResult(errors.Count == 0 
                ? IdentityResult.Success 
                : IdentityResult.Failed(errors.ToArray()));
        }
    }
}
