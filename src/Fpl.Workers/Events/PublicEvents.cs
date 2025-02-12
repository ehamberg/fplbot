using System;
using Fpl.Client.Models;
using MediatR;
using System.Collections.Generic;

namespace FplBot.Core.Models
{
    // Public events:
    public record TwentyFourHoursToDeadline(Gameweek Gameweek) : INotification;
    public record OneHourToDeadline(Gameweek Gameweek) : INotification;
    public record PriceChangeOccured(IEnumerable<PlayerUpdate> PlayerWithPriceChanges) : INotification;
    public record InjuryUpdateOccured(IEnumerable<PlayerUpdate> PlayersWithInjuryUpdates) : INotification;
    public record GameweekJustBegan(Gameweek Gameweek) : INotification;
    public record LineupReady(Lineups Lineups) : INotification;
    public record FixtureEventsOccured(FixtureUpdates FixtureEvents) : INotification;
    public record FixturesFinished(IEnumerable<FinishedFixture> FinishedFixture) : INotification;
    public record GameweekFinished(Gameweek Gameweek) : INotification;
    public record BonusAdded(int Event, DateTime MatchDayDate) : INotification;
    public record PointsReady(int Event, DateTime MatchDayDate) : INotification;
}