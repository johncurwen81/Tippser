using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Tippser.Core.Entities;
using Tippser.Core.Models;
using static Tippser.Core.Enums.Endpoint;

namespace Tippser.Core.Interfaces
{
    public interface IBaseRepository
    {
        Task<TEntity> Create<TEntity>(TEntity item) where TEntity : BaseEntity;

        Task<Person> Create(Person item);

        Task<IEnumerable<TEntity?>> Read<TEntity>(bool? active = null) where TEntity : BaseEntity;

        Task<IEnumerable<Person?>> Read(bool? active = null);

        Task<TEntity?> Read<TEntity>(string id, bool? active = null) where TEntity : BaseEntity;

        Task<Person?> Read(string id, bool? active = null);

        Task<TEntity> Update<TEntity>(TEntity item) where TEntity : BaseEntity;

        Task<Person> Update(Person item);

        Task Delete<TEntity>(TEntity item) where TEntity : BaseEntity;

        Task Delete(Person item);

        Task<IEnumerable<TEntity>?> Find<TEntity>(TEntity item) where TEntity : BaseEntity;
    }

    public interface IDbService
    {
        Task<TEntity> Create<TEntity>(TEntity item) where TEntity : BaseEntity;

        Task<Person> Create(Person item);

        Task<IEnumerable<TEntity?>> Read<TEntity>(bool? active = null) where TEntity : BaseEntity;

        Task<IEnumerable<Person?>> Read(bool? active = null);

        Task<TEntity?> Read<TEntity>(string id, bool? active = null) where TEntity : BaseEntity;

        Task<Person?> Read(string id, bool? active = null);

        Task<TEntity> Update<TEntity>(TEntity item) where TEntity : BaseEntity;

        Task<Person> Update(Person item);

        Task Delete<TEntity>(TEntity item) where TEntity : BaseEntity;

        Task Delete(Person item);

        Task<IEnumerable<TEntity>?> Find<TEntity>(TEntity item) where TEntity : BaseEntity;
    }

    public interface IScraperService
    {
        Task<IEnumerable<string>?> GetScheduleData(string url);
    }

    public interface IMatchDataService
    {
        Task ImportScheduleData();

        IEnumerable<StandingsDto> CollateCompetitionData(IEnumerable<Competition> competitions);
    }

    public interface IUserService
    {
        Task<IdentityResult> CreateAsync(Person person, CancellationToken cancellationToken);
        Task<IdentityResult> UpdateAsync(Person person, CancellationToken cancellationToken);
        Task<IdentityResult> DeleteAsync(Person person, CancellationToken cancellationToken);
        Task<Person?> FindByIdAsync(string userId, CancellationToken cancellationToken);
        Task<Person?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken);
        Task<Person?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
        Task<string> GetUserIdAsync(Person person, CancellationToken cancellationToken);
        Task<string?> GetUserNameAsync(Person person, CancellationToken cancellationToken);
        Task SetUserNameAsync(Person person, string? userName, CancellationToken cancellationToken);
        Task<string?> GetNormalizedUserNameAsync(Person person, CancellationToken cancellationToken);
        Task SetNormalizedUserNameAsync(Person person, string? normalizedName, CancellationToken cancellationToken);
        Task<string?> GetPasswordHashAsync(Person person, CancellationToken cancellationToken);
        Task SetPasswordHashAsync(Person person, string? passwordHash, CancellationToken cancellationToken);
        Task<bool> HasPasswordAsync(Person person, CancellationToken cancellationToken);
        string CreatePasswordHash(Person person, string password);
        bool VerifyPasswordHash(Person person, string password);
        Task<IdentityResult> CreateRoleAsync(string roleName);
        Task<IdentityResult> AddToRoleAsync(Person person, string role);
    }

    public interface IBackroundJobsService
    {
        void CreateSchedule();
    }

    public interface ISignInService
    {
        Task<SignInResult> PasswordSignIn(string email, string password, bool isPersistent = false, bool lockoutOnFailure = false);
        Task<SignInResult> CheckPasswordSignInAsync(Person person, string password, bool lockoutOnFailure = false);
        Task SignOut(HttpContext context);
    }

    public interface IApiService
    {
        Task<TObject?> SendRequestAsync<TObject>(HttpMethod method, ApiEndpoint endpoint, object data = null!);
    }
}
