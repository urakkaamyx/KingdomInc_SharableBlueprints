using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SharableBlueprints
{
    internal sealed partial class BlueprintUiTheme
    {
        public void Image(Image image, string name, Color fallback)
        {
            image.sprite = Sprite(name);
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.color = image.sprite ? Color.white : fallback;
        }
    }
}
