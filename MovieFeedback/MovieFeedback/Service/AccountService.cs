using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MovieFeedback.DTOs.Account;
using MovieFeedback.Models;
using MovieFeedback.ViewModels;
using System.Security.Claims;

namespace MovieFeedback.Services
{
    public interface IAccountService
    {
        public Task<bool> LoginAsync(string username, string password);
        public Task LogoutAsync();
        public Task<(bool Success, string Error)> RegisterAsync(string username, string email, string password);
        Task<ProfileViewModel> GetProfileAsync(int userId);
        Task<ChangePasswordResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword, string confirmPassword);
        Task<bool> DeleteAccountAsync(int userId, string password);
        Task<User> GetUserByCredentialsAsync(string username, string password);

    }
    public class AccountService : IAccountService
    {
        private readonly MovieFeedbackDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountService(MovieFeedbackDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return false;

            if (user.IsBanned)
                return false;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(7)
                });

            return true;
        }

        public async Task LogoutAsync()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync();
        }

        public async Task<(bool Success, string Error)> RegisterAsync(string username, string email, string password)
        {
            if (_context.Users.Any(u => u.Username == username || u.Email == email))
                return (false, "Username or email already exists.");

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return (true, null);
        }

        public async Task<ChangePasswordResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword, string confirmPassword)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return new ChangePasswordResult { Success = false, ErrorMessage = "User not found." };

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                return new ChangePasswordResult { Success = false, ErrorMessage = "Current password is incorrect." };

            if (newPassword != confirmPassword)
                return new ChangePasswordResult { Success = false, ErrorMessage = "New passwords do not match." };

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            return new ChangePasswordResult { Success = true };
        }

        public async Task<ProfileViewModel> GetProfileAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                throw new Exception("User not found");

            return new ProfileViewModel
            {
                Username = user.Username,
                Email = user.Email
            };
        }

        public async Task<bool> DeleteAccountAsync(int userId, string password)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return false;

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> GetUserByCredentialsAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null;
            }

            return user;
        }


    }
}
