using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SharableBlueprints
{
    public sealed partial class BlueprintLibraryStore
    {
        public static string Name(string value)
        {
            value = (value ?? "").Trim();
            if (value.Length == 0 || value.Length > 80 || value.Any(char.IsControl))
                throw new InvalidOperationException("Use a name between 1 and 80 characters.");
            return value;
        }
    }
}
