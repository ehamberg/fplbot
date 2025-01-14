﻿using System.Collections.Generic;

namespace FplBot.Core
{
    internal static class PlayerNickNames
    {
        public static readonly IDictionary<string, string> NickNameToRealNameMap = new Dictionary<string, string>
        {
            { "Lord", "John Lundstram" },
            { "Lord Lundstram", "John Lundstram" },
            { "Kun", "Sergio Agüero" },
            { "Kun Agüero", "Sergio Agüero" }
        };
    }
}
