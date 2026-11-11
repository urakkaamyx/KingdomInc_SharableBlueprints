using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SharableBlueprints
{
    internal sealed partial class BlueprintUiTheme
    {
        public Sprite Sprite(string name) => sprites.TryGetValue(name, out var value) ? value : null;
    }
}
