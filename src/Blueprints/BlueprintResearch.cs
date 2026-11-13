using KingdomFoundation.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using KingdomFoundation.Json;

namespace SharableBlueprints
{
    public static class BlueprintResearch
    {
        static readonly Dictionary<string, List<string>> Unlocks = LoadUnlocks();
        public static BlueprintResearchReport Check(PortableBlueprint blueprint)
        {
            var report = new BlueprintResearchReport();
            var player = FoundationApi.Kingdom;
            if (player == null)
            {
                report.Add("no-kingdom", "Load a kingdom", "Open a kingdom before placing a blueprint.", blueprint.Name);
                return report;
            }

            var tech = FoundationApi.ResearchAccess.Nodes.ToDictionary(t => t.Identifier, StringComparer.Ordinal);
            var elements = FoundationApi.WorkspaceAccess.DiscoverCatalog().ToDictionary(e => e.TypeId, StringComparer.Ordinal);
            var items = FoundationApi.ItemAccess.Items.ToDictionary(i => i.Identifier, StringComparer.Ordinal);
            var recipes = FoundationApi.ItemAccess.Recipes.ToDictionary(r => r.Identifier, StringComparer.Ordinal);
            Action<GameResearchNode, string> require = null;
            var visited = new HashSet<string>(StringComparer.Ordinal);
            require = (node, subject) =>
            {
                if (node == null || player.HasResearch(node))
                    return;
                report.Add(node.Identifier, FoundationApi.GameInfo.Translate(node.Name), FoundationApi.GameInfo.Translate(node.Description), subject);
                if (!visited.Add(node.Identifier))
                    return;
                if (node.Root != node)
                    require(node.Root, subject);
                foreach (var prerequisite in node.Requirements)
                    require(prerequisite, subject);
                if (node.HasCustomRequirement && !node.CheckCustomRequirement(player) && !string.IsNullOrEmpty(node.CustomRequirementDescription))
                    report.Add(node.Identifier + "/condition", FoundationApi.GameInfo.Translate(node.Name) + " — condition", FoundationApi.GameInfo.Translate(node.CustomRequirementDescription), subject);
            };
            Action<string, string> requireId = (id, subject) =>
            {
                if (tech.TryGetValue(id, out var node))
                    require(node, subject);
                else
                    report.Add("unknown-tech/" + id, "Unavailable research: " + id, "This research is not present in the installed game. Placement is blocked until the blueprint and game versions are compatible.", subject);
            };
            Action<GameItemType, string> requireItem = (item, subject) =>
            {
                if (item.IsUnlocked)
                    return;
                bool found = false;
                if (Unlocks.TryGetValue("item/" + item.Identifier, out var unlocks))
                    foreach (string id in unlocks.Where(id => tech.ContainsKey(id) && !player.HasResearch(tech[id])))
                    {
                        requireId(id, subject);
                        found = true;
                    }

                if (!found && item.DefaultRecipe?.RequiredResearch != null)
                {
                    require(item.DefaultRecipe.RequiredResearch, subject);
                    found = !player.HasResearch(item.DefaultRecipe.RequiredResearch);
                }

                if (!found)
                {
                    var candidates = tech.Values.Where(t => t.Identifier == item.Identifier || t.Identifier.EndsWith("_" + item.Identifier, StringComparison.Ordinal)).ToList();
                    foreach (var node in candidates.Where(t => !player.HasResearch(t)))
                    {
                        require(node, subject);
                        found = true;
                    }
                }

                if (!found && item.HasSourceQuest)
                    foreach (var quest in item.SourceQuestResearch)
                        if (quest != null && !player.HasResearch(quest))
                        {
                            require(quest, subject);
                            found = true;
                        }

                if (!found)
                    report.Add("item/" + item.Identifier, "Unlock " + FoundationApi.GameInfo.Translate(item.GetDisplayName()), "This item is not unlocked in this kingdom. Unlock its resource, recipe or adventure reward before placing a blueprint that uses it.", subject);
            };
            foreach (var data in blueprint.Document["layout"]["Elements"].AsJsonArray)
            {
                string type = data["ElementType"].AsString;
                if (!elements.TryGetValue(type, out var element))
                {
                    report.Add("building/" + type, "Unavailable building: " + type, "This building is not present in the installed game. The blueprint can stay in your library, but cannot be placed.", type);
                    continue;
                }

                string subject = FoundationApi.GameInfo.Translate(element.Label), baseId = FoundationApi.GameInfo.ToIdentifier(type);
                require(element.RequiredResearch, subject);
                if (PortableBlueprint.Field(data, "StackSize").AsInteger > 1)
                    requireId("element_stacking", subject);
                var upgrades = PortableBlueprint.Array(PortableBlueprint.Field(data, "Upgrades")).ToList();
                var available = new HashSet<string>(StringComparer.Ordinal);
                if (upgrades.Any(u => !u.IsNull))
                    try
                    {
                        available.UnionWith(player.AvailableUpgrades(type));
                    }
                    catch (ArgumentNullException)
                    {
                    }

                for (int index = 0; index < upgrades.Count; index++)
                {
                    if (upgrades[index].IsNull)
                        continue;
                    string upgrade = PortableBlueprint.Field(upgrades[index], "Type").AsString;
                    if (index >= player.UpgradeSlotCount(type))
                        requireId(baseId + "_automation" + (index == 0 ? "" : "_" + (index + 1)), subject);
                    if (upgrade != null && available.Contains(upgrade))
                        continue;
                    var ids = new List<string>();
                    if (upgrade != null && Unlocks.TryGetValue("upgrade/" + type + "/" + upgrade, out var mapped))
                        ids.AddRange(mapped);
                    string suffix = upgrade == "Worker" ? "automation" : upgrade == "Overseer" ? "productivity" : upgrade == "Coffers" ? "coffers" : upgrade != null && upgrade.Contains("Artisan") ? "quality" : null;
                    if (suffix != null)
                        ids.Add(baseId + "_" + suffix);
                    bool identified = false;
                    foreach (string id in ids.Distinct())
                        if (tech.TryGetValue(id, out var node) && !player.HasResearch(node))
                        {
                            require(node, subject + " / " + upgrade);
                            identified = true;
                        }

                    if (!identified)
                        report.Add("upgrade/" + type + "/" + upgrade, "Unlock " + (upgrade ?? "unknown upgrade") + " for " + subject, "This upgrade is not available for this building in the current kingdom. Its upgrade research must be unlocked before placement.", subject);
                }

                foreach (var reference in PortableBlueprint.References(data))
                {
                    string id = reference.value.Split(':')[0];
                    if (reference.key == "Recipe")
                    {
                        if (recipes.TryGetValue(id, out var recipe))
                            require(recipe.RequiredResearch, subject + " / " + FoundationApi.GameInfo.Translate(recipe.DisplayName));
                        else
                            report.Add("recipe/" + id, "Unavailable recipe: " + id, "This recipe is not recognized by the installed game.", subject);
                    }
                    else if (items.TryGetValue(id, out var item))
                        requireItem(item, subject + " / " + FoundationApi.GameInfo.Translate(item.GetDisplayName()));
                    else if (new[]
                    {
                        "Resource",
                        "SelectedResource",
                        "FilterType",
                        "OutputType"
                    }.Contains(reference.key))
                        report.Add("item/" + id, "Unavailable item: " + id, "This configured item is not recognized by the installed game.", subject);
                }
            }

            if (blueprint.Decorations > 0)
                requireId("tech_deco", "Blueprint decorations");
            foreach (var deco in blueprint.Document["layout"]["Decorations"].AsJsonArray)
            {
                var sticker = PortableBlueprint.Field(deco, "Sticker");
                if (!sticker.IsNull && PortableBlueprint.Field(sticker, "Type").AsString != "IconSticker")
                    report.Add("unsupported-sticker", "Unsupported decoration", "This decoration's sticker type is not recognized by the installed game.", "Blueprint decorations");
            }

            return report;
        }

        static Dictionary<string, List<string>> LoadUnlocks()
        {
            var result = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("research-unlocks.tsv")))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("#") || line.Length == 0)
                        continue;
                    var parts = line.Split('\t');
                    if (parts.Length != 3)
                        continue;
                    string key = parts[0] + "/" + parts[1];
                    if (!result.TryGetValue(key, out var ids))
                        result.Add(key, ids = new List<string>());
                    ids.Add(parts[2]);
                }
            }

            return result;
        }
    }
}
