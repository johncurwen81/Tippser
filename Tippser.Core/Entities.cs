using Microsoft.AspNetCore.Identity;
using static System.Net.Mime.MediaTypeNames;

namespace Tippser.Core.Entities
{
    public abstract class BaseEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public bool Active { get; set; } = true;
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public string CreatedByPersonId { get; set; } = Constants.SystemUserId;
        public virtual Person? CreatedBy { get; set; }
        public DateTime ModifiedUtc { get; set; } = DateTime.UtcNow;
        public string ModifiedByPersonId { get; set; } = Constants.SystemUserId;
        public virtual Person? ModifiedBy { get; set; } 
    }

    public class Person : IdentityUser
    {
        public Person()
        {

        }

        public Person(string text, string personId)
        {
            Name = text;
            Email = text.ToLower();
            NormalizedEmail = text.ToUpper();
            UserName = text.ToLower();
            NormalizedUserName = text.ToUpper();
            CreatedByPersonId = personId;
            ModifiedByPersonId = personId;
        }

        public Person(string name, string email, string passwordClear, string personId)
        {
            Name = name;
            Email = email.ToLower();
            NormalizedEmail = email.ToUpper();
            UserName = email.ToLower();
            NormalizedUserName = email.ToUpper();
            CreatedByPersonId = personId;
            ModifiedByPersonId = personId;
            PasswordHash = passwordClear;
        }

        public bool Active { get; set; } = true;
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public string CreatedByPersonId { get; set; } = Constants.SystemUserId;
        public virtual Person? CreatedBy { get; set; }
        public DateTime ModifiedUtc { get; set; } = DateTime.UtcNow;
        public string ModifiedByPersonId { get; set; } = Constants.SystemUserId;
        public virtual Person? ModifiedBy { get; set; }
        public string Name { get; set; } = string.Empty;
        public virtual ICollection<Bet> Bets { get; set; } = new HashSet<Bet>();
    }

    public class Bet : BaseEntity
    {        
        public Bet()
        {

        }

        public Bet(string personId, string matchId, int homeGoals, int awayGoals, string createdByPersonId)
        {
            PersonId = personId;
            MatchId = matchId;
            AssessedHomeGoals = homeGoals;
            AssessedAwayGoals = awayGoals;
            CreatedByPersonId = createdByPersonId;
            ModifiedByPersonId = createdByPersonId;

        }
        public string UniqueName { get; set;} = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public int AssessedHomeGoals { get; set; }
        public int AssessedAwayGoals { get; set; }

        public string PersonId { get; set; }= string.Empty;
        public virtual Person Person { get; set; }
        public string MatchId { get; set; }= string.Empty;
        public virtual Match Match { get; set; }
        public string CompetitionId { get; set; }= string.Empty;
        public virtual Competition Competition { get; set; }
    }

    public class Competition : BaseEntity
    {
        public Competition()
        {

        }

        public Competition(string name, DateOnly start, DateOnly end, string personId)
        {
            Name = name;
            CreatedByPersonId = personId;
            ModifiedByPersonId = personId;
        }

        public string Name { get; set; } = string.Empty;
        public string ScheduleUrl { get; set; } = string.Empty;
        public string ResultsUrl { get; set; } = string.Empty;
        public DateTime Start { get; set; } = DateTime.UtcNow;
        public DateTime End { get; set; } = DateTime.UtcNow.AddMonths(1);
        public virtual ICollection<Team> Teams { get; set; } = new HashSet<Team>();
        public virtual ICollection<Match> Matches { get; set; } = new HashSet<Match>();
        public virtual ICollection<Bet> Bets { get; set; } = new HashSet<Bet>();
    }

    public class Match : BaseEntity
    {
        public Match()
        {

        }

        public Match(Team homeTeam, Team awayTeam, DateTime kickoff, string personId)
        {
            Name = $"{homeTeam.Name} vs {awayTeam.Name}";
            HomeTeamId = homeTeam.Id;
            AwayTeamId = awayTeam.Id;
            Kickoff = kickoff;
            CreatedByPersonId = personId;
            ModifiedByPersonId = personId;
        }

        public Match(DateTime kickoffDate, TimeSpan kickoffTime, string venueId, Team homeTeam, Team awayTeam, string personId)
        {
            Kickoff = kickoffDate.AddTicks(kickoffTime.Ticks);
            VenueId = venueId;
            Name = $"{homeTeam.Name} vs {awayTeam.Name}";
            HomeTeamId = homeTeam.Id;
            AwayTeamId = awayTeam.Id;
            ActualHomeGoals = null;
            ActualAwayGoals = null;
            CreatedByPersonId = personId;
            ModifiedByPersonId = personId;
        }

        public string Name { get; set; } = string.Empty;
        public string HomeTeamId { get; set; }= string.Empty;
        public virtual Team HomeTeam { get; set; }
        public string AwayTeamId { get; set; }= string.Empty;
        public virtual Team AwayTeam { get; set; }
        public DateTime Kickoff { get; set; } = DateTime.UtcNow;
        public int? ActualHomeGoals { get; set; }
        public int? ActualAwayGoals { get; set; }
        public string VenueId { get; set; }= string.Empty;
        public virtual Venue Venue { get; set; }
        public string CompetitionId { get; set; }= string.Empty;
        public virtual Competition Competition { get; set; }
        public virtual ICollection<Bet> Bets { get; set; } = new HashSet<Bet>();
    }

    public class Team : BaseEntity
    {
        public Team()
        {

        }

        public Team(string name, string personId)
        {
            Name = name;
            CreatedByPersonId = personId;
            ModifiedByPersonId = personId;
        }

        public string Name { get; set; } = string.Empty;
        public string GroupId { get; set; }= string.Empty;
        public virtual Group Group { get; set; }
        public string CompetitionId { get; set; }= string.Empty;
        public virtual Competition Competition { get; set; }
        public virtual ICollection<Match> HomeMatches { get; set; } = new HashSet<Match>();
        public virtual ICollection<Match> AwayMatches { get; set; } = new HashSet<Match>();
    }

    public class Group : BaseEntity
    {
        public Group() 
        {

        }

        public Group(string identifier, string personId)
        {
            Name = identifier;
            CreatedByPersonId = personId;
            ModifiedByPersonId = personId;
        }

        public string Name { get; set; } = string.Empty;
        public virtual ICollection<Team> Teams { get; set; } = new HashSet<Team>();
    }

    public class Venue : BaseEntity
    {
        public Venue()
        {


            CreatedByPersonId = Constants.SystemUserId;
            ModifiedByPersonId = Constants.SystemUserId;
        }

        public Venue(string name, string personId) 
        {
            Name = name;
            CreatedByPersonId = personId;
            ModifiedByPersonId = personId;
        }

        public string Name { get; set; } = string.Empty;
        public virtual ICollection<Match> Matches { get; set; } = new HashSet<Match>();
    }

}