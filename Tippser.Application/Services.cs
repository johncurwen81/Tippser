using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Tippser.Core.Entities;
using Tippser.Core.Interfaces;
using Tippser.Core.Models;
using static Tippser.Core.Enums.Endpoint;

namespace Tippser.Application.Services
{
    public class DbService : IDbService
    {
        private readonly ILogger _logger;
        private readonly IBaseRepository _repo;

        public DbService(ILogger<DbService> logger, IBaseRepository repo)
        {
            _logger = logger;
            _repo = repo;
        }

        public async Task<TEntity> Create<TEntity>(TEntity item)
            where TEntity : BaseEntity
        {
            return await _repo.Create(item);
        }

        public async Task<Person> Create(Person item)
        {
            return await _repo.Create(item);
        }

        public async Task<IEnumerable<TEntity?>> Read<TEntity>(bool? active = null)
            where TEntity : BaseEntity
        {
            return await _repo.Read<TEntity>(active);
        }

        public async Task<IEnumerable<Person?>> Read(bool? active = null)
        {
            return await _repo.Read(active);
        }

        public async Task<TEntity?> Read<TEntity>(string id, bool? active = null)
            where TEntity : BaseEntity
        {
            return await _repo.Read<TEntity>(id, active);
        }

        public async Task<Person?> Read(string id, bool? active = null)
        {
            return await _repo.Read(id, active);
        }

        public async Task<TEntity> Update<TEntity>(TEntity item)
            where TEntity : BaseEntity
        {
            return await _repo.Update<TEntity>(item);
        }

        public async Task<Person> Update(Person item)
        {
            return await _repo.Update(item);
        }

        public async Task Delete<TEntity>(TEntity item)
            where TEntity : BaseEntity
        {
            await _repo.Delete(item);
        }

        public async Task Delete(Person item)
        {
            await _repo.Delete(item);
        }

        public async Task<IEnumerable<TEntity>?> Find<TEntity>(TEntity item)
            where TEntity : BaseEntity
        {
            return await _repo.Find(item);
        }
    }

    public class MatchDataService : IMatchDataService
    {
        private readonly ILogger _logger;
        private readonly IScraperService _scraper;
        private readonly IDbService _db;
        private bool _running = false;

        public MatchDataService(ILogger<MatchDataService> logger, IScraperService scraper, IDbService db)
        {
            _logger = logger;
            _scraper = scraper;
            _db = db;
        }

        public async Task ImportScheduleData()
        {
            if (_running)
            {
                return;
            }

            _running = true;

            var competitions = await _db.Read<Competition>(true);

            foreach (var competition in competitions)
            {
                var scheduleData = await GetScheduleData(competition.ScheduleUrl);
                if (scheduleData == null)
                {
                    return;
                }

                var extractedScheduleData = ExtractScheduleData(scheduleData);

                await CreateEntities(extractedScheduleData, competition);
            }

            _running = false;
        }

        public IEnumerable<StandingsDto> CollateCompetitionData(IEnumerable<Competition> competitions)
        {
            return competitions.Select(c =>
                    new StandingsDto(
                        competition: c,
                        table:
                            c.Teams.OrderBy(t => t.Group.Name).GroupBy(team => team.Group)
                            .SelectMany(
                                group => group.Select(
                                    team => new TeamDto(
                                        group: team.Group,
                                        team: team,
                                        goalsFor:
                                            team.HomeMatches
                                                .Sum(m => m.ActualHomeGoals ?? 0) +
                                            team.AwayMatches
                                                .Sum(m => m.ActualAwayGoals ?? 0),
                                        goalsAgainst:
                                            team.HomeMatches
                                                .Sum(m => m.ActualAwayGoals ?? 0) +
                                            team.AwayMatches
                                                .Sum(m => m.ActualHomeGoals ?? 0),
                                        points:
                                            (team.HomeMatches.Count(m => m.ActualHomeGoals > m.ActualAwayGoals) * 3) +
                                            (team.AwayMatches.Count(m => m.ActualAwayGoals > m.ActualHomeGoals) * 3) +
                                            (team.HomeMatches.Count(m => m.ActualHomeGoals == m.ActualAwayGoals) * 1) +
                                            (team.AwayMatches.Count(m => m.ActualHomeGoals == m.ActualAwayGoals) * 1)
                                )
                            )
                            .OrderByDescending(tableTeam => tableTeam.Points)
                            .ThenByDescending(tableTeam => tableTeam.GoalsFor)
                            .ThenBy(tableTeam => tableTeam.GoalDifference)
                        )
                    )
                );
        }

        private async Task<IEnumerable<string>?> GetScheduleData(string url)
        {
            var data = await _scraper.GetScheduleData(url);
            if (data == null)
            {
                _logger.LogInformation($"{nameof(IScraperService)}.{nameof(GetScheduleData)} returned no scheduleData");
                return null;
            }

            return data.Where(n => n.StartsWith("<b>"));
        }

        private IEnumerable<IEnumerable<IEnumerable<string>>> ExtractScheduleData(IEnumerable<string> scrape)
        {
            List<List<List<string>>> competitionResult = new List<List<List<string>>>();

            foreach (var data in scrape)
            {
                if (data != null)
                {
                    var dayData = data.Split("<br>");

                    List<List<string>> dayResult = new List<List<string>>();

                    var first = true;
                    var date = string.Empty;

                    foreach (var d in dayData)
                    {
                        if (first)
                        {
                            string pattern = @"<b>(.*?)</b>";

                            var matches = Regex.Matches(d, pattern);

                            date = matches[0].Groups[1].Value;

                            first = false;
                        }
                        else
                        {
                            string pattern = @"(.*?): <a href=\""(.*?)\"".*>(.*?) vs (.*?)<\/a>.*<a.*>(.*?)<\/a>, (.*?)\)";

                            var matches = Regex.Matches(d, pattern);

                            foreach (System.Text.RegularExpressions.Match match in matches)
                            {
                                List<string> matchResult = new List<string>
                                {
                                    date,
                                    match.Groups[1].Value,
                                    match.Groups[2].Value,
                                    match.Groups[3].Value,
                                    match.Groups[4].Value,
                                    match.Groups[5].Value,
                                    match.Groups[6].Value
                                };

                                dayResult.Add(matchResult);
                            }
                        }
                    }
                    competitionResult.Add(dayResult);
                }
            }

            return competitionResult;
        }

        private async Task CreateEntities(IEnumerable<IEnumerable<IEnumerable<string>>> data, Competition competition)
        {
            foreach (var day in data)
            {
                foreach (var match in day)
                {
                    try
                    {
                        var date = DateTime.Parse(match.Skip(0).FirstOrDefault()!);
                        if (date.Year != competition.Start.Year)
                        {
                            date = date.AddYears(competition.Start.Year - date.Year);
                        }
                        var groupIdentifier = match.Skip(1).FirstOrDefault()!.Replace("Group", "").Replace(" ", "");
                        var url = match.Skip(2).FirstOrDefault();
                        var homeTeam = match.Skip(3).FirstOrDefault();
                        var awayTeam = match.Skip(4).FirstOrDefault();
                        var venue = match.Skip(5).FirstOrDefault();
                        var time = TimeSpan.Parse(match.Skip(6).FirstOrDefault()!);

                        string entityId = string.Empty;

                        var groupEntity = new Core.Entities.Group(groupIdentifier!, Core.Constants.SystemUserId);
                        entityId = (await _db.Find(groupEntity))?.FirstOrDefault()?.Id!;
                        if (entityId == null)
                        {
                            await _db.Create(groupEntity);
                        }
                        else
                        {
                            groupEntity = await _db.Read<Core.Entities.Group>(entityId);
                        }

                        var venueEntity = new Core.Entities.Venue(venue!, Core.Constants.SystemUserId);
                        entityId = (await _db.Find(venueEntity))?.FirstOrDefault()?.Id!;
                        if (entityId == null)
                        {
                            await _db.Create(venueEntity);
                        }
                        else
                        {
                            venueEntity = await _db.Read<Venue>(entityId);
                        }

                        var homeTeamEntity = new Core.Entities.Team(homeTeam!, Core.Constants.SystemUserId);
                        entityId = (await _db.Find(homeTeamEntity))?.FirstOrDefault()?.Id!;
                        if (entityId == null)
                        {
                            homeTeamEntity.CompetitionId = competition.Id;
                            homeTeamEntity.GroupId = groupEntity!.Id;
                            await _db.Create(homeTeamEntity);
                        }
                        else
                        {
                            homeTeamEntity = await _db.Read<Core.Entities.Team>(entityId);
                        }

                        var awayTeamEntity = new Core.Entities.Team(awayTeam!, Core.Constants.SystemUserId);
                        entityId = (await _db.Find(awayTeamEntity))?.FirstOrDefault()?.Id!;
                        if (entityId == null)
                        {
                            awayTeamEntity.CompetitionId = competition.Id;
                            awayTeamEntity.GroupId = groupEntity!.Id;
                            await _db.Create(awayTeamEntity);
                        }
                        else
                        {
                            awayTeamEntity = await _db.Read<Core.Entities.Team>(entityId);
                        }

                        var matchEntity = new Core.Entities.Match(date, time, venueEntity!.Id, homeTeamEntity!, awayTeamEntity!, Core.Constants.SystemUserId);
                        entityId = (await _db.Find(matchEntity))?.FirstOrDefault()?.Id!;
                        if (entityId == null)
                        {
                            matchEntity.CompetitionId = competition.Id;
                            await _db.Create(matchEntity!);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                    
                }
            }
        }
    }
}