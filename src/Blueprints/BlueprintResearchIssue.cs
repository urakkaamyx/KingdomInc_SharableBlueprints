using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LightJson;

namespace SharableBlueprints
{
    public sealed partial class BlueprintResearchIssue
    {
        public string Id, Name, Description;
        public readonly HashSet<string> Affected = new HashSet<string>(StringComparer.Ordinal);
    }
}
