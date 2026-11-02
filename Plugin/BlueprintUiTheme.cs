using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SharableBlueprints
{
    // Use the sprites already loaded by the native game UI, including their nine-slice borders.
    // No copies of the game's artwork are shipped with the mod.
    internal sealed class BlueprintUiTheme
    {
        readonly Dictionary<string, Sprite> sprites;
        public BlueprintUiTheme()
        {
            sprites = Resources.FindObjectsOfTypeAll<Sprite>().Where(s => s)
                .GroupBy(s => s.name).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        }
        public Sprite Sprite(string name) => sprites.TryGetValue(name, out var value) ? value : null;
        public void Image(Image image, string name, Color fallback)
        {
            image.sprite = Sprite(name);
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.color = image.sprite ? Color.white : fallback;
        }
        public void Button(Button button, bool accent = false, bool danger = false)
        {
            Image(button.GetComponent<Image>(), accent ? "Button02" : "Button03", new Color(.24f, .19f, .15f));
            button.GetComponent<Image>().color = danger ? new Color(1, .68f, .61f) : Color.white;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1, .94f, .76f);
            colors.pressedColor = new Color(.72f, .66f, .53f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(.65f, .62f, .59f, .48f);
            colors.fadeDuration = .08f;
            button.colors = colors;
        }
    }
}
