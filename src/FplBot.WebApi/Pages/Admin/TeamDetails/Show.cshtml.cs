using System;
using System.Linq;
using System.Threading.Tasks;
using Fpl.Client.Abstractions;
using Fpl.Client.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Slackbot.Net.Abstractions.Hosting;
using Slackbot.Net.Extensions.FplBot.Abstractions;
using Slackbot.Net.SlackClients.Http;

namespace FplBot.WebApi.Pages.Admin.TeamDetails
{
    public class TeamDetailsIndex : PageModel
    {
        private readonly IState _stateDetails;
        private readonly ISlackTeamRepository _teamRepo;
        private readonly ITokenStore _tokenStore;
        private readonly ISlackClientBuilder _builder;
        private readonly ILeagueClient _leagueClient;
        private readonly IOptions<SlackAppOptions> _slackAppOptions;
        private readonly ILogger<TeamDetailsIndex> _logger;

        public TeamDetailsIndex(IState state, ISlackTeamRepository teamRepo, ITokenStore tokenStore, ILogger<TeamDetailsIndex> logger, IOptions<SlackAppOptions> slackAppOptions, ISlackClientBuilder builder, ILeagueClient leagueClient)
        {
            _stateDetails = state;
            _teamRepo = teamRepo;
            _tokenStore = tokenStore;
            _logger = logger;
            _slackAppOptions = slackAppOptions;
            _builder = builder;
            _leagueClient = leagueClient;
        }
        
        public async Task OnGet(string teamId)
        {
            var teamIdToUpper = teamId.ToUpper();
            var team = await _teamRepo.GetTeam(teamIdToUpper);
            if (team != null)
            {
                Team = team;
                var league = await _leagueClient.GetClassicLeague((int) team.FplbotLeagueId);
                League = league;
                var slackClient = await CreateSlackClient(teamIdToUpper);
                try
                {
                    var channels = await slackClient.ConversationsListPublicChannels(500);
                    ChannelStatus = channels.Channels.FirstOrDefault(c => team.FplBotSlackChannel == $"#{c.Name}") != null;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }
                var ctx = _stateDetails.GetGameweekLeagueContext(teamIdToUpper);
                GameweekLeagueContext = JsonConvert.SerializeObject(new
                {
                    TransfersForLeagueCount = ctx.TransfersForLeague.Count(),
                    PlayersCount = ctx.Players.Count(),
                    TeamsCount = ctx.Teams.Count(),
                    EntriesCount = ctx.GameweekEntries.Count(),
                    CurrentGameweek = ctx.CurrentGameweek
                }, Formatting.Indented);
            }
        }

        public ClassicLeague League { get; set; }
        public bool? ChannelStatus { get; set; }

        public async Task<IActionResult> OnPost(string teamId)
        {
            _logger.LogInformation($"Deleting {teamId}");
            var slackClient = await CreateSlackClient(teamId);
            var res = await slackClient.AppsUninstall(_slackAppOptions.Value.Client_Id, _slackAppOptions.Value.Client_Secret);
            if (res.Ok)
            {
                TempData["msg"] = "Uninstall queued, and will be handled at some point";
            }
            else
            {
                TempData["msg"] = $"Uninstall failed '{res.Error}'";
            }
            
            return RedirectToPage("Index");
        }

        private async Task<ISlackClient> CreateSlackClient(string teamId)
        {
            var token = await _tokenStore.GetTokenByTeamId(teamId);
            var slackClient = _builder.Build(token: token);
            return slackClient;
        }

        public string GameweekLeagueContext { get; set; }
        public SlackTeam Team { get; set; }
    }
}