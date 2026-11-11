using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Assets.Source.Flow;
using Assets.Source.Items;
using Assets.Source.Player;
using Assets.Source.Util;
using LightJson;

namespace SharableBlueprints
{
    public static partial class BlueprintResearch
    {
        static readonly Dictionary<string, List<string>> Unlocks = LoadUnlocks();
        public static BlueprintResearchReport Check(PortableBlueprint blueprint)
        {
            var report = new BlueprintResearchReport();
            var player = GamePlayer.Current;
            if (player == null) { report.Add("no-kingdom", "Load a kingdom", "Open a kingdom before placing a blueprint.", blueprint.Name); return report; }
            var tech = TechNode.Nodes.ToDictionary(t => t.Identifier, StringComparer.Ordinal);
            var elements = FlowElement.GetAllTypes().ToDictionary(e => e.ElementType, StringComparer.Ordinal);
            var items = ItemType.All.ToDictionary(i => i.Identifier, StringComparer.Ordinal);
            var recipes = CraftingRecipe.All.ToDictionary(r => r.Identifier, StringComparer.Ordinal);
            Action<TechNode, string> require = null;
            var visited = new HashSet<string>(StringComparer.Ordinal);
            require = (node, subject) => {
                if (node == null || player.HasTech(node)) return;
                report.Add(node.Identifier, Translation.Translate(node.Name), Translation.Translate(node.Description), subject);
                if (!visited.Add(node.Identifier)) return;
                if (node.Root != node) require(node.Root, subject);
                foreach (var prerequisite in node.AllRequirements) require(prerequisite, subject);
                if (node.CustomRequirement != null && !node.CustomRequirement(player) && !string.IsNullOrEmpty(node.CustomRequirementDesc))
                    report.Add(node.Identifier + "/condition", Translation.Translate(node.Name) + " — condition", Translation.Translate(node.CustomRequirementDesc), subject);
            };
            Action<string, string> requireId = (id, subject) => {
                if (tech.TryGetValue(id, out var node)) require(node, subject);
                else report.Add("unknown-tech/" + id, "Unavailable research: " + id, "This research is not present in the installed game. Placement is blocked until the blueprint and game versions are compatible.", subject);
            };
            Action<ItemType, string> requireItem = (item, subject) => {
                if (item.IsUnlocked) return;
                bool found = false;
                if (Unlocks.TryGetValue("item/" + item.Identifier, out var unlocks))
                    foreach (string id in unlocks.Where(id => tech.ContainsKey(id) && !player.HasTech(tech[id]))) { requireId(id, subject); found = true; }
                if (!found && item.DefaultRecipe?.RequiredTech != null) { require(item.DefaultRecipe.RequiredTech, subject); found = !player.HasTech(item.DefaultRecipe.RequiredTech); }
                if (!found)
                {
                    var candidates = tech.Values.Where(t => t.Identifier == item.Identifier || t.Identifier.EndsWith("_" + item.Identifier, StringComparison.Ordinal)).ToList();
                    foreach (var node in candidates.Where(t => !player.HasTech(t))) { require(node, subject); found = true; }
                }
                if (!found && item.SourceQuest)
                    foreach (var quest in item.GetSourceQuests()) if (quest.RequiredTech != null && !player.HasTech(quest.RequiredTech)) { require(quest.RequiredTech, subject); found = true; }
                if (!found) report.Add("item/" + item.Identifier, "Unlock " + Translation.Translate(item.GetDisplayName()),
                    "This item is not unlocked in this kingdom. Unlock its resource, recipe or adventure reward before placing a blueprint that uses it.", subject);
            };
            foreach (var data in blueprint.Document["layout"]["Elements"].AsJsonArray)
            {
                string type = data["ElementType"].AsString;
                if (!elements.TryGetValue(type, out var element))
                {
                    report.Add("building/" + type, "Unavailable building: " + type, "This building is not present in the installed game. The blueprint can stay in your library, but cannot be placed.", type); continue;
                }
                string subject = Translation.Translate(element.Label), baseId = type.ToSnakeCase();
                require(element.RequiredTech, subject);
                if (PortableBlueprint.Field(data, "StackSize").AsInteger > 1) requireId("element_stacking", subject);
                var upgrades = PortableBlueprint.Array(PortableBlueprint.Field(data, "Upgrades")).ToList();
                var available = new HashSet<string>(StringComparer.Ordinal);
                if (upgrades.Any(u => !u.IsNull))
                    try { available.UnionWith(player.GetAvailableFlowUpgrades(type)); } catch (ArgumentNullException) { }
                for (int index = 0; index < upgrades.Count; index++)
                {
                    if (upgrades[index].IsNull) continue;
                    string upgrade = PortableBlueprint.Field(upgrades[index], "Type").AsString;
                    if (index >= player.GetUpgradeSlotCount(type)) requireId(baseId + "_automation" + (index == 0 ? "" : "_" + (index + 1)), subject);
                    if (upgrade != null && available.Contains(upgrade)) continue;
                    var ids = new List<string>();
                    if (upgrade != null && Unlocks.TryGetValue("upgrade/" + type + "/" + upgrade, out var mapped)) ids.AddRange(mapped);
                    string suffix = upgrade == "Worker" ? "automation" : upgrade == "Overseer" ? "productivity" : upgrade == "Coffers" ? "coffers" : upgrade != null && upgrade.Contains("Artisan") ? "quality" : null;
                    if (suffix != null) ids.Add(baseId + "_" + suffix);
                    bool identified = false;
                    foreach (string id in ids.Distinct()) if (tech.TryGetValue(id, out var node) && !player.HasTech(node)) { require(node, subject + " / " + upgrade); identified = true; }
                    if (!identified) report.Add("upgrade/" + type + "/" + upgrade, "Unlock " + (upgrade ?? "unknown upgrade") + " for " + subject,
                        "This upgrade is not available for this building in the current kingdom. Its upgrade research must be unlocked before placement.", subject);
                }
                foreach (var reference in PortableBlueprint.References(data))
                {
                    string id = reference.value.Split(':')[0];
                    if (reference.key == "Recipe")
                    {
                        if (recipes.TryGetValue(id, out var recipe)) require(recipe.RequiredTech, subject + " / " + Translation.Translate(recipe.DisplayName));
                        else report.Add("recipe/" + id, "Unavailable recipe: " + id, "This recipe is not recognized by the installed game.", subject);
                    }
                    else if (items.TryGetValue(id, out var item)) requireItem(item, subject + " / " + Translation.Translate(item.GetDisplayName()));
                    else if (new[] { "Resource", "SelectedResource", "FilterType", "OutputType" }.Contains(reference.key))
                        report.Add("item/" + id, "Unavailable item: " + id, "This configured item is not recognized by the installed game.", subject);
                }
            }
            if (blueprint.Decorations > 0) requireId("tech_deco", "Blueprint decorations");
            foreach (var deco in blueprint.Document["layout"]["Decorations"].AsJsonArray)
            {
                var sticker = PortableBlueprint.Field(deco, "Sticker");
                if (!sticker.IsNull && PortableBlueprint.Field(sticker, "Type").AsString != "IconSticker")
                    report.Add("unsupported-sticker", "Unsupported decoration", "This decoration's sticker type is not recognized by the installed game.", "Blueprint decorations");
            }
            return report;
        }
    }
}
