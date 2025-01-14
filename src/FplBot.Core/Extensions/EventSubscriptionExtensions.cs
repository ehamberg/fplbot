﻿using System;
using System.Collections.Generic;
using System.Linq;
using FplBot.Data.Models;

namespace FplBot.Core.Extensions
{
    public static class EventSubscriptionExtensions
    {
        public static bool ContainsSubscriptionFor(this IEnumerable<EventSubscription> eventSubscriptions,
            EventSubscription eventSubscription)
        {
            var events = eventSubscriptions as EventSubscription[] ?? eventSubscriptions.ToArray();

            return events.Contains(EventSubscription.All) || events.Contains(eventSubscription);
        }
    }

    public static class EventSubscriptionHelper
    {
        public static IEnumerable<EventSubscription> GetAllSubscriptionTypes()
        {
            return Enum.GetValues(typeof(EventSubscription)).Cast<EventSubscription>();
        }
    }
}
