using System;
using System.Collections.Generic;
using System.IO;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Source.Flow;
using Assets.Source.Player;
using HarmonyLib;
using KingdomFoundation.Api;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SharableBlueprints
{
    public sealed partial class BlueprintPlacement : MonoBehaviour
    {
        public CurrencyCollection Cost()
        {
            var total = new CurrencyCollection();
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var element in source.Elements)
            {
                counts.TryGetValue(element.ElementType, out int count);
                total.Add(element.GetCopyCost(count)); counts[element.ElementType] = count + Math.Max(1, element.StackSize);
            }
            foreach (var deco in source.Decorations) total.Add(deco.GetCopyCost(false));
            return total;
        }
    }
}
