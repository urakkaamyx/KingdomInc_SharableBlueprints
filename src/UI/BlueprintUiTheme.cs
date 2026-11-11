using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SharableBlueprints
{
    internal sealed partial class BlueprintUiTheme
    {
        readonly Dictionary<string, Sprite> sprites;
        public BlueprintUiTheme()
        {
            sprites = Resources.FindObjectsOfTypeAll<Sprite>().Where(s => s)
                .GroupBy(s => s.name).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        }
    }
}
