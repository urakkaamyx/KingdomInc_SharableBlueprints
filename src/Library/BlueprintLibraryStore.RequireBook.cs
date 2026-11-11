using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SharableBlueprints
{
    public sealed partial class BlueprintLibraryStore
    {
        void RequireBook(string id)
        {
            if (!string.IsNullOrEmpty(id) && !Books.Any(b => b.Id == id)) throw new InvalidOperationException("That blueprint book no longer exists.");
        }
    }
}
