﻿using System.Net.Http;
using System.Threading.Tasks;
using Fpl.Client.Abstractions;
using FplBot.Core.Abstractions;
using FplBot.Data.Abstractions;
using Microsoft.Extensions.Logging;
using Slackbot.Net.Endpoints.Abstractions;
using Slackbot.Net.Endpoints.Models.Events;

namespace FplBot.Core.Handlers
{
    public class FplChangeLeagueIdHandler : HandleAppMentionBase
    {
        private readonly ISlackTeamRepository _slackTeamRepository;
        private readonly ILeagueClient _leagueClient;
        private readonly ISlackWorkSpacePublisher _publisher;
        private readonly ILogger<FplChangeLeagueIdHandler> _logger;

        public FplChangeLeagueIdHandler(ISlackTeamRepository slackTeamRepository, ILeagueClient leagueClient, ISlackWorkSpacePublisher publisher, ILogger<FplChangeLeagueIdHandler> logger)
        {
            _slackTeamRepository = slackTeamRepository;
            _leagueClient = leagueClient;
            _publisher = publisher;
            _logger = logger;
        }

        public override string[] Commands => new[] { "updateleagueid" };

        public override async Task<EventHandledResponse> Handle(EventMetaData eventMetadata, AppMentionEvent message)
        {
            var newLeagueId = ParseArguments(message);

            if (string.IsNullOrEmpty(newLeagueId))
            {
                var help = $"No leagueId provided. Usage: `@fplbot updateleagueid 123`";
                await _publisher.PublishToWorkspace(eventMetadata.Team_Id, message.Channel, help);
                return new EventHandledResponse(help);
            }

            var couldParse = int.TryParse(newLeagueId, out var theLeagueId);

            if (!couldParse)
            {
                var res = $"Could not update league to id '{newLeagueId}'. Make sure it's a valid number.";
                await _publisher.PublishToWorkspace(eventMetadata.Team_Id, message.Channel, res);
                return new EventHandledResponse(res);
            }

            var failure = $"Could not find league {newLeagueId} :/ Could you find it at https://fantasy.premierleague.com/leagues/{newLeagueId}/standings/c ?";
            try
            {
                var league = await _leagueClient.GetClassicLeague(theLeagueId);

                if (league?.Properties != null)
                {
                    await _slackTeamRepository.UpdateLeagueId(eventMetadata.Team_Id, theLeagueId);
                    var success = $"Thanks! You're now following the '{league.Properties.Name}' league (leagueId: {theLeagueId})";
                    await _publisher.PublishToWorkspace(eventMetadata.Team_Id, message.Channel, success);
                    return new EventHandledResponse(success);
                }
                await _publisher.PublishToWorkspace(eventMetadata.Team_Id, message.Channel, failure);
                return new EventHandledResponse(failure);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e.Message, e);
                await _publisher.PublishToWorkspace(eventMetadata.Team_Id, message.Channel, failure);
                return new EventHandledResponse(failure);
            }
        }

        public override (string, string) GetHelpDescription() => ($"{CommandsFormatted} {{new league id}}", "Change which league to follow");
    }
}
