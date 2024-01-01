using Tippser.Core.Entities;
using Tippser.Core.Models;

namespace Tippser.Presentation.Client.Models
{
    public class BaseViewModel
    {
        public Person? User { get; set; } = null;
    }

    public class TeamViewModel
    {
        public TeamViewModel()
        {
                
        }
        public TeamViewModel(TeamDto teamDto)
        {
            Group = teamDto.Group;
            Team = teamDto.Team;
            Played = teamDto.Played;
            GoalsFor = teamDto.GoalsFor;
            GoalsAgainst = teamDto.GoalsAgainst;
            GoalDifference = teamDto.GoalDifference;
            Points = teamDto.Points;
        }
        public string Group { get; set; } = string.Empty;
        public string Team { get; set; } = string.Empty;
        public int Played { get; set; } = 0;
        public int GoalsFor { get; set; } = 0;
        public int GoalsAgainst { get; set; } = 0;
        public int GoalDifference { get; set; } = 0;
        public int Points { get; set; } = 0;
    }

    public class StandingsViewModel : BaseViewModel
    {
        public StandingsViewModel()
        {
                
        }

        public StandingsViewModel(StandingsDto standings)
        {
            Name = standings.Name;
            Table = standings.Table;
        }

        public string Name { get; set; } = string.Empty;
        public IEnumerable<TeamDto> Table { get; set; } = Enumerable.Empty<TeamDto>();
    }

    public class ErrorViewModel : BaseViewModel
    {
        public ErrorViewModel()
        {
                
        }

        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    public class SignInModel(string email, string password, bool rememberMe)
    {
        public string Email { get; set; } = email;
        public string Password { get; set; } = password;
        public bool RememberMe { get; set; } = rememberMe;
    }

    public class CreateUserModel(string name, string email, string password, string confirm)
    {
        public string Name { get; set; } = name;
        public string Email { get; set; } = email;
        public string Password { get; set; } = password;
        public string Confirm { get; set; } = confirm;
    }

    public class SignInResult(bool success, bool locked)
    {
        public bool Success { get; set; } = success;
        public bool Locked { get; set; } = locked;
    }
}
