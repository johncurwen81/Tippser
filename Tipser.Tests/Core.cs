using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Tippser.Core;

namespace Tipser.Tests.Core
{
    public class Person
    {
        [Fact]
        public void Create()
        {
            var person = new Tippser.Core.Entities.Person
            {
                Id = Constants.SuperAdminUserId,
                CreatedUtc = new DateTime(1900, 1, 1),
                ModifiedUtc = new DateTime(1900, 1, 1),
                CreatedByPersonId = Constants.SystemUserId,
                ModifiedByPersonId = Constants.SystemUserId,
                Email = "john.curwen.81@gmail.com",
                Name = "John Curwen",
                PasswordHash = "AQAAAAIAAYagAAAAENEVr0S6Ttwd5jYURtshVMJtPPn7oRyt+yx8jkRfuXKWv1byoPj935VT12l8XEsouA=="
            };

            var passwordHasher = new PasswordHasher<Tippser.Core.Entities.Person>();
            passwordHasher.VerifyHashedPassword(person, person.PasswordHash, "12345678!").Should().Be(PasswordVerificationResult.Success);
        }
    }
}