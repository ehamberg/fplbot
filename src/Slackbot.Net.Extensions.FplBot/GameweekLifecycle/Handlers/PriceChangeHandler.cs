using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Slackbot.Net.Extensions.FplBot.Abstractions;
using Slackbot.Net.Extensions.FplBot.Extensions;
using Slackbot.Net.Extensions.FplBot.Helpers;
using Slackbot.Net.Extensions.FplBot.Models;

namespace Slackbot.Net.Extensions.FplBot.GameweekLifecycle.Handlers
{
    public class PriceChangeHandler
    {
        private readonly ISlackWorkSpacePublisher _publisher;
        private readonly ISlackTeamRepository _slackTeamRepo;
        private readonly ILogger<PriceChangeHandler> _logger;

        public PriceChangeHandler(ISlackWorkSpacePublisher publisher, ISlackTeamRepository slackTeamRepo, ILogger<PriceChangeHandler> logger)
        {
            _publisher = publisher;
            _slackTeamRepo = slackTeamRepo;
            _logger = logger;
        }

        public async Task OnPriceChanges(IEnumerable<PlayerUpdate> priceChanges)
        {
            _logger.LogInformation($"Handling {priceChanges.Count()} price updates");
            var slackTeams = await _slackTeamRepo.GetAllTeams();
            foreach (var slackTeam in slackTeams)
            {
                if (slackTeam.Subscriptions.ContainsSubscriptionFor(EventSubscription.PriceChanges))
                {
                    var filtered = priceChanges.Where(c => c.ToPlayer.IsRelevant());
                    if (filtered.Any())
                    {
                        var formatted = Formatter.FormatPriceChanged(filtered);
                        await _publisher.PublishToWorkspace(slackTeam.TeamId, slackTeam.FplBotSlackChannel, formatted);
                    }
                    else
                    {
                        _logger.LogInformation("All price changes are all irrelevant, so not sending any notification");
                    }
                }
            }
        }
    }
}