using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tippser.Core.Entities;

namespace Tippser.Core.Models
{
    public class StandingsDto
    {
        public StandingsDto()
        {
            
        }
        public StandingsDto(Competition competition, IEnumerable<TeamDto> table)
        {
            Name = competition.Name;
            Table = table;
        }
        public string Name { get; set; }
        public IEnumerable<TeamDto> Table { get; set; }
    }

    public class TeamDto
    {
        public TeamDto()
        {
            
        }
        public TeamDto(Group group, Team team, int goalsFor, int goalsAgainst, int points)
        {
            Group = group.Name;
            Team = team.Name;
            Played = team.HomeMatches.Count + team.AwayMatches.Count;
            GoalsFor = goalsFor;
            GoalsAgainst = goalsAgainst;
            GoalDifference = goalsFor - goalsAgainst;
            Points = points;
        }
        public string Group { get; set; }
        public string Team { get; set; }
        public int Played { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int GoalDifference { get; set; }
        public int Points { get; set; }
    }
}
