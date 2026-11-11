using System;
using System.Collections.Generic;
using System.Linq;

namespace SharableBlueprints
{
    public sealed partial class BlueprintSuggestions
    {
        public static BlueprintSuggestions Create(IEnumerable<string> labels, IEnumerable<BlueprintIconChoice> icons)
        {
            var result = new BlueprintSuggestions();
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string label in labels ?? Enumerable.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(label)) continue;
                string name = string.Join(" ", new string(label.Where(c => !char.IsControl(c) || char.IsWhiteSpace(c)).ToArray())
                    .Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
                name = name.Substring(0, Math.Min(80, name.Length)).Trim();
                if (name.Length > 0 && char.IsHighSurrogate(name[name.Length - 1])) name = name.Substring(0, name.Length - 1);
                if (name.Length > 0 && names.Add(name)) result.Labels.Add(name);
            }
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var icon in icons ?? Enumerable.Empty<BlueprintIconChoice>())
                if (icon != null && !string.IsNullOrWhiteSpace(icon.Id) && icon.Id.Length <= 256 && !icon.Id.Any(char.IsControl) && ids.Add(icon.Id))
                    result.Icons.Add(new BlueprintIconChoice { Id = icon.Id, Name = string.IsNullOrWhiteSpace(icon.Name) ? icon.Id : icon.Name });
            return result;
        }
    }
}
