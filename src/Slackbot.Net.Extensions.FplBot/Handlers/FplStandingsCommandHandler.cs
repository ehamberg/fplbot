using System;
using System.Threading.Tasks;
using Fpl.Client.Abstractions;
using Microsoft.Extensions.Logging;
using Slackbot.Net.Abstractions.Hosting;
using Slackbot.Net.Endpoints.Abstractions;
using Slackbot.Net.Endpoints.Models;
using Slackbot.Net.Extensions.FplBot.Abstractions;
using Slackbot.Net.Extensions.FplBot.GameweekLifecycle;
using Slackbot.Net.Extensions.FplBot.Helpers;

namespace Slackbot.Net.Extensions.FplBot.Handlers
{
    internal class FplStandingsCommandHandler : IHandleEvent
    {
        private readonly ISlackWorkSpacePublisher _workspacePublisher;
        private readonly IGameweekClient _gameweekClient;
        private readonly ILeagueClient _leagueClient;
        private readonly ITokenStore _tokenStore;
        private readonly IFetchFplbotSetup _setupFetcher;
        private readonly ILogger<FplStandingsCommandHandler> _logger;

        public FplStandingsCommandHandler(ISlackWorkSpacePublisher workspacePublisher, IGameweekClient gameweekClient, ILeagueClient leagueClient, ITokenStore tokenStore, IFetchFplbotSetup setupFetcher, ILogger<FplStandingsCommandHandler> logger)
        {
            _workspacePublisher = workspacePublisher;
            _gameweekClient = gameweekClient;
            _leagueClient = leagueClient;
            _tokenStore = tokenStore;
            _setupFetcher = setupFetcher;
            _logger = logger;
        }

        public async Task<EventHandledResponse> Handle(EventMetaData eventMetadata, SlackEvent slackEvent)
        {
            var appMentioned = slackEvent as AppMentionEvent;
            var standings = await GetStandings(eventMetadata.Team_Id);
            await _workspacePublisher.PublishToWorkspace(eventMetadata.Team_Id, appMentioned.Channel, standings);
            return new EventHandledResponse(standings);
        }

        private async Task<string> GetStandings(string teamId)
        {
            try
            {
                var token = await _tokenStore.GetTokenByTeamId(teamId);
                var setup = await _setupFetcher.GetSetupByToken(token);
                var league = await _leagueClient.GetClassicLeague(setup.LeagueId);
                var gameweeks = await _gameweekClient.GetGameweeks();
                var standings = Formatter.GetStandings(league, gameweeks);
                return standings;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message,e);
                return $"Oops, could not fetch standings.";
            }
        }
        public bool ShouldHandle(SlackEvent slackEvent) => slackEvent is AppMentionEvent @event && @event.Text.Contains("standings");
        
        public (string,string) GetHelpDescription() => ("standings", "Get current league standings");
     
    }
}