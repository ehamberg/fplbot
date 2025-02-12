using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fpl.Client.Abstractions;
using Fpl.Client.Models;
using FplBot.Core.Extensions;
using FplBot.Data.Abstractions;
using FplBot.Data.Models;
using MediatR;

namespace FplBot.Core.Handlers.InternalCommands
{
    public record UpdateAllEntryStats : INotification;
    public record UpdateEntryStats(int EntryId) : INotification;

    public class UpdateEntryStatsCommandHandler : INotificationHandler<UpdateAllEntryStats>, INotificationHandler<UpdateEntryStats>
    {
        private readonly IEntryHistoryClient _entryHistoryClient;
        private readonly IGlobalSettingsClient _settingsClient;
        private readonly IVerifiedEntriesRepository _verifiedEntriesRepository;
        private readonly IEntryClient _entryClient;

        public UpdateEntryStatsCommandHandler(IEntryHistoryClient entryHistoryClient, IGlobalSettingsClient settingsClient, IEntryClient entryClient, IVerifiedEntriesRepository verifiedEntriesRepository)
        {
            _entryHistoryClient = entryHistoryClient;
            _settingsClient = settingsClient;
            _verifiedEntriesRepository = verifiedEntriesRepository;
            _entryClient = entryClient;
        }

        public async Task Handle(UpdateAllEntryStats notification, CancellationToken cancellationToken)
        {
            var allEntries = await _verifiedEntriesRepository.GetAllVerifiedEntries();
            var settings = await _settingsClient.GetGlobalSettings();
            var players = settings.Players;
            foreach (var entry in allEntries)
            {
                var newStats = await GetUpdatedStatsForEntry(entry.EntryId, players);
                await _verifiedEntriesRepository.UpdateAllStats(entry.EntryId, newStats);
            }
        }

        public async Task Handle(UpdateEntryStats notification, CancellationToken cancellationToken)
        {
            var settings = await _settingsClient.GetGlobalSettings();
            var players = settings.Players;
            var newStats = await GetUpdatedStatsForEntry(notification.EntryId, players);
            await _verifiedEntriesRepository.UpdateAllStats(notification.EntryId, newStats);
        }

        private async Task<VerifiedEntryStats> GetUpdatedStatsForEntry(int entryId, ICollection<Player> players)
        {
            var history = await _entryHistoryClient.GetHistory(entryId);
            var latestReportedGameweek = history.GameweekHistory.LastOrDefault();
            var picks = await _entryClient.GetPicks(entryId, latestReportedGameweek.Event);
            var captainId = picks.Picks.SingleOrDefault(p => p.IsCaptain)?.PlayerId;
            var viceCaptainId = picks.Picks.SingleOrDefault(p => p.IsViceCaptain)?.PlayerId;
            var lastGw = history.GameweekHistory.Count > 1 ? history.GameweekHistory.ElementAtOrDefault(history.GameweekHistory.Count - 2) : latestReportedGameweek;
            return new VerifiedEntryStats(
                CurrentGwTotalPoints: latestReportedGameweek.TotalPoints,
                LastGwTotalPoints: lastGw.TotalPoints,
                OverallRank: latestReportedGameweek.OverallRank ?? 0,
                PointsThisGw: latestReportedGameweek.Points,
                ActiveChip: picks.ActiveChip,
                Captain: players.Get(captainId).WebName,
                ViceCaptain: players.Get(viceCaptainId).WebName,
                Gameweek: latestReportedGameweek.Event);
        }
    }
}
