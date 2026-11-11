using System;
using System.Collections.Generic;
using System.Linq;

namespace SharableBlueprints
{
    public sealed partial class BlueprintSuggestions
    {
        public List<string> Labels = new List<string>();
        public List<BlueprintIconChoice> Icons = new List<BlueprintIconChoice>();
        public string DefaultIcon => Icons.FirstOrDefault()?.Id ?? "";
        public string DefaultName(string fallback) => Labels.FirstOrDefault() ?? fallback;
    }
}
