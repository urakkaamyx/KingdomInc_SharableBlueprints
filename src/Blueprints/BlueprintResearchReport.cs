using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LightJson;

namespace SharableBlueprints
{
    public sealed partial class BlueprintResearchReport
    {
        public readonly List<BlueprintResearchIssue> Issues = new List<BlueprintResearchIssue>();
        public bool CanPlace => Issues.Count == 0;
        public void Add(string id, string name, string description, string affected)
        {
            var issue = Issues.FirstOrDefault(i => i.Id == id);
            if (issue == null) { issue = new BlueprintResearchIssue { Id = id, Name = name, Description = description }; Issues.Add(issue); }
            issue.Affected.Add(affected);
        }
    }
}
