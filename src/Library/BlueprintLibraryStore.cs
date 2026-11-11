using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SharableBlueprints
{
    public sealed partial class BlueprintLibraryStore
    {
        public string DirectoryPath { get; }
        public List<BlueprintBook> Books { get; private set; } = new List<BlueprintBook>();
        public List<LibraryBlueprint> Blueprints { get; private set; } = new List<LibraryBlueprint>();
        string IndexPath => Path.Combine(DirectoryPath, "Library.xml");
        public BlueprintLibraryStore(string directory) { DirectoryPath = Path.GetFullPath(directory); }
    }
}
