using Hangfire;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Tippser.Core.Entities;
using Tippser.Core.Interfaces;

namespace Tippser.Infrastructure.Services
{
    public class ScraperService : IScraperService
    {
        public async Task<IEnumerable<string>?> GetScheduleData(string url)
        {
            HttpClient http = new HttpClient();
            var response = await http.GetAsync(url);
            var pageContents = await response.Content.ReadAsStringAsync();

            HtmlDocument pageDocument = new HtmlDocument();
            pageDocument.LoadHtml(pageContents);

            var nodes = pageDocument.DocumentNode.SelectNodes("//p");
            return nodes?.Select(n => n.InnerHtml);
        }
    }

    public class UserService(ILogger<UserService> logger, IDbService db, IPasswordHasher<Person> passwordHasher, RoleManager<IdentityRole> roleManager, UserManager<Person> userManager) : IUserService, IUserStore<Person>, IUserPasswordStore<Person>
    {
        private readonly ILogger<UserService> _logger = logger;
        private readonly IDbService _db = db;
        private readonly IPasswordHasher<Person> _passwordHasher = passwordHasher;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly UserManager<Person> _userManager = userManager;

        public async Task<IdentityResult> CreateAsync(Person person, CancellationToken cancellationToken)
        {
            try
            {
                person.PasswordHash = CreatePasswordHash(person, person.PasswordHash!);
                person.EmailConfirmed = false;
                await _db.Create(person);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating person");
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public async Task<IdentityResult> UpdateAsync(Person person, CancellationToken cancellationToken)
        {
            try
            {
                await _db.Update(person);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public async Task<IdentityResult> DeleteAsync(Person person, CancellationToken cancellationToken)
        {
            try
            {
                await _db.Delete(person);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public async Task<Person?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await _db.Read(userId);
        }

        public async Task<Person?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return (await _db.Read()).FirstOrDefault(u => u.NormalizedUserName == normalizedUserName);
        }

        public Task<string> GetUserIdAsync(Person person, CancellationToken cancellationToken)
        {
            return Task.FromResult(person.Id);
        }

        public Task<string?> GetUserNameAsync(Person person, CancellationToken cancellationToken)
        {
            return Task.FromResult(person.UserName);
        }

        public Task SetUserNameAsync(Person person, string? userName, CancellationToken cancellationToken)
        {
            person.UserName = userName;
            return Task.CompletedTask;
        }

        public Task<string?> GetNormalizedUserNameAsync(Person person, CancellationToken cancellationToken)
        {
            return Task.FromResult(person.NormalizedUserName);
        }

        public Task SetNormalizedUserNameAsync(Person person, string? normalizedName, CancellationToken cancellationToken)
        {
            person.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task<string?> GetPasswordHashAsync(Person person, CancellationToken cancellationToken)
        {
            return Task.FromResult(person.PasswordHash);
        }

        public Task SetPasswordHashAsync(Person person, string? passwordHash, CancellationToken cancellationToken)
        {
            person.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task<bool> HasPasswordAsync(Person person, CancellationToken cancellationToken)
        {
            return Task.FromResult(person.PasswordHash != null);
        }

        public string CreatePasswordHash(Person person, string password)
        {
            return _passwordHasher.HashPassword(person, password);
        }

        public bool VerifyPasswordHash(Person person, string password)
        {
            var result = _passwordHasher.VerifyHashedPassword(person, person.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }

        public async Task<IdentityResult> CreateRoleAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return await _roleManager.CreateAsync(new IdentityRole(roleName));
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> AddToRoleAsync(Person person, string role)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var response = await CreateRoleAsync(role);
                if (!response.Succeeded)
                {
                    return response;
                }
            }
            return await _userManager.AddToRoleAsync(person, role);
        }

        public void Dispose()
        {
            // Dispose any resources if necessary
        }
    }

    public class SignInService : ISignInService
    {
        private readonly SignInManager<Person> _signInManager;

        public SignInService(SignInManager<Person> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<SignInResult> PasswordSignIn(string email, string password, bool isPersistent = false, bool lockoutOnFailure = false)
        {
            return await _signInManager.PasswordSignInAsync(email, password, isPersistent, lockoutOnFailure);
        }

        public async Task SignOut(HttpContext context)
        {
            await _signInManager.SignOutAsync();
        }
    }

    public class EmailService : IEmailSender<Person>
    {
        private readonly IEmailSender emailSender = new NoOpEmailSender();

        public Task SendConfirmationLinkAsync(Person user, string email, string confirmationLink) =>
            emailSender.SendEmailAsync(email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

        public Task SendPasswordResetLinkAsync(Person user, string email, string resetLink) =>
            emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

        public Task SendPasswordResetCodeAsync(Person user, string email, string resetCode) =>
            emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password using the following code: {resetCode}");
    }

    public class BackroundJobsService : IBackroundJobsService
    {
        private readonly IBackgroundJobClient _backgroundJobs;
        private readonly IRecurringJobManager _jobManager;

        public BackroundJobsService(IBackgroundJobClient backgroundJobs, IRecurringJobManager jobManager)
        {
            _backgroundJobs = backgroundJobs;
            _jobManager = jobManager;
        }

        public void CreateSchedule()
        {
            _backgroundJobs.Enqueue<IMatchDataService>(x => x.ImportScheduleData());

            //_jobManager.AddOrUpdate<IMatchDataService>(nameof(IMatchDataService.ImportScheduleData), x => x.ImportScheduleData(), Cron.Daily);
        }
    }
}
