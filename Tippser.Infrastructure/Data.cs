using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tippser.Core;
using Tippser.Core.Entities;

namespace Tippser.Infrastructure.Data
{
    public class TippserDbContext : IdentityDbContext<Person>
    {
        public TippserDbContext(DbContextOptions<TippserDbContext> options)
        : base(options)
        {

        }

        public DbSet<Group> Groups { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<Competition> Competitions { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Bet> Bets { get; set; }
        public DbSet<Person> Persons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 

            modelBuilder.Entity<Match>(entity =>
            {
                entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedByPersonId)
                .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.ModifiedBy)
                .WithMany()
                .HasForeignKey(e => e.ModifiedByPersonId)
                .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.HomeTeam)
                .WithMany(e => e.HomeMatches)
                .HasForeignKey(e => e.HomeTeamId)
                .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.AwayTeam)
                .WithMany(e => e.AwayMatches)
                .HasForeignKey(e => e.AwayTeamId)
                .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Venue)
                .WithMany(e => e.Matches)
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Competition)
                .WithMany(e => e.Matches)
                .HasForeignKey(e => e.CompetitionId)
                .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Bet>(entity => {
                entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedByPersonId)
                .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.ModifiedBy)
                .WithMany()
                .HasForeignKey(e => e.ModifiedByPersonId)
                .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Match)
                .WithMany(e => e.Bets)
                .HasForeignKey(e => e.MatchId)
                .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Person)
                .WithMany(e => e.Bets)
                .HasForeignKey(e => e.PersonId)
                .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Competition)
                .WithMany(e => e.Bets)
                .HasForeignKey(e => e.CompetitionId)
                .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Team>(entity => {
                entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedByPersonId)
                .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.ModifiedBy)
                .WithMany()
                .HasForeignKey(e => e.ModifiedByPersonId)
                .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Group)
                .WithMany(e => e.Teams)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Competition)
                .WithMany(e => e.Teams)
                .HasForeignKey(e => e.CompetitionId)
                .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedByPersonId)
                .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.ModifiedBy)
                .WithMany()
                .HasForeignKey(e => e.ModifiedByPersonId)
                .OnDelete(DeleteBehavior.NoAction);

                entity.HasData(
                    new Person() { 
                        Id = Constants.SystemUserId, 
                        CreatedUtc = new DateTime(1900, 1, 1), 
                        ModifiedUtc = new DateTime(1900, 1, 1), 
                        CreatedByPersonId = Constants.SystemUserId, 
                        ModifiedByPersonId = Constants.SystemUserId, 
                        Email = string.Empty,
                        NormalizedEmail = string.Empty,
                        UserName = "SYSTEM_USER",
                        NormalizedUserName = "SYSTEM_USER",
                        EmailConfirmed = true,
                        Name = "SYSTEM_USER", 
                        PasswordHash = string.Empty 
                    },
                    new Person()
                    {
                        Id = Constants.SuperAdminUserId,
                        CreatedUtc = new DateTime(1900,1,1),
                        ModifiedUtc = new DateTime(1900, 1, 1),
                        CreatedByPersonId = Constants.SystemUserId,
                        ModifiedByPersonId = Constants.SystemUserId,
                        Email = "john.curwen.81@gmail.com",
                        NormalizedEmail = "JOHN.CURWEN.81@GMAIL.COM",
                        UserName = "john.curwen.81@gmail.com",
                        NormalizedUserName = "JOHN.CURWEN.81@GMAIL.COM",
                        EmailConfirmed = true,
                        Name = "John Curwen",
                        PasswordHash = "AQAAAAIAAYagAAAAENEVr0S6Ttwd5jYURtshVMJtPPn7oRyt+yx8jkRfuXKWv1byoPj935VT12l8XEsouA=="
                    });
            });
            
            modelBuilder.Entity<Group>(entity => {
                entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedByPersonId)
                .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.ModifiedBy)
                .WithMany()
                .HasForeignKey(e => e.ModifiedByPersonId)
                .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Venue>(entity => {
                entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedByPersonId)
                .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.ModifiedBy)
                .WithMany()
                .HasForeignKey(e => e.ModifiedByPersonId)
                .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Competition>(entity => {
                entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedByPersonId)
                .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.ModifiedBy)
                .WithMany()
                .HasForeignKey(e => e.ModifiedByPersonId)
                .OnDelete(DeleteBehavior.NoAction);

                entity.HasData(  
                    new Competition()
                    {
                        Id = "0b3e703e-bd46-44b2-b868-1ffac7b4674f",
                        Active = true,
                        CreatedUtc = DateTime.UtcNow,
                        ModifiedUtc = DateTime.UtcNow,
                        CreatedByPersonId = Constants.SystemUserId,
                        ModifiedByPersonId = Constants.SystemUserId,
                        Name = "UEFA Euro 2024",
                        Start = new DateTime(2024, 6, 14),
                        End = new DateTime(2024, 7, 2),
                        ScheduleUrl = "https://www.uefa.com/euro2024/news/0275-151eb1c333ea-d30deec67b13-1000--uefa-euro-2024-fixtures-when-and-where-are-the-matches/",
                        ResultsUrl = "https://www.uefa.com/euro2024/news/0275-151eb1c333ea-d30deec67b13-1000--uefa-euro-2024-fixtures-when-and-where-are-the-matches/"
            });
            });

            modelBuilder.Entity<IdentityRole>(entity =>
            {
                entity.HasData(
                    new IdentityRole()
                    {
                        Id = Constants.SuperAdminRoleId,
                        Name = "SuperAdmin",
                        NormalizedName = "SUPERADMIN"
                    },
                    new IdentityRole()
                    {
                        Id = Constants.UserRoleId,
                        Name = "User",
                        NormalizedName = "USER"
                    }
                );
            });

            modelBuilder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.HasData(
                    new IdentityUserRole<string>
                    {
                        RoleId = Constants.SuperAdminRoleId,
                        UserId = Constants.SuperAdminUserId
                    }
                );
            });              
        }
    }
}
