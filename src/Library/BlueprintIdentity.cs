using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Source.Flow;
using Assets.Source.Flow.Stickers;
using Assets.Source.Util;
using LightJson;
using UnityEngine;

namespace SharableBlueprints
{
    public static partial class BlueprintIdentity
    {
        public static BlueprintSuggestions FromSelection(FlowDiagram flow, SelectionBox bounds)
        {
            var decorations = SelectionContents.Decorations(flow, bounds).OrderByDescending(d => d.Position.y).ThenBy(d => d.Position.x).ToList();
            var catalog = Catalog().ToDictionary(i => i.Id, i => i.Name, StringComparer.Ordinal);
            var icons = new List<BlueprintIconChoice>();
            foreach (var deco in decorations)
                if (deco.Sticker is IconSticker sticker) icons.Add(Choice(sticker.IconName, catalog));
            foreach (var building in SelectionContents.Buildings(flow, bounds).OrderByDescending(e => e.Position.y).ThenBy(e => e.Position.x))
                icons.Add(new BlueprintIconChoice { Id = building.IconName, Name = Translation.Translate(building.Label) });
            return BlueprintSuggestions.Create(decorations.Select(d => d.Label), icons);
        }
    }
}
