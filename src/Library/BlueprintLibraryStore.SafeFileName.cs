using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SharableBlueprints
{
    public sealed partial class BlueprintLibraryStore
    {
        static bool SafeFileName(string file) => !string.IsNullOrWhiteSpace(file) && file == Path.GetFileName(file)
            && file.IndexOfAny(new[] { '/', '\\', ':' }) < 0 && file.EndsWith(".kiblueprint.json", StringComparison.OrdinalIgnoreCase);
    }
}
