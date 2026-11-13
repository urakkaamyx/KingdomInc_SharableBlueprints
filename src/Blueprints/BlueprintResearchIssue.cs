using KingdomFoundation.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingdomFoundation.Json;

namespace SharableBlueprints
{
    public sealed class BlueprintResearchIssue
    {
        public string Id, Name, Description;
        public readonly HashSet<string> Affected = new HashSet<string>(StringComparer.Ordinal);
    }
}
