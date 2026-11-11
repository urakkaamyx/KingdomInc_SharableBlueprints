using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SharableBlueprints
{
    public sealed partial class BlueprintLibraryStore
    {
        public string FilePath(LibraryBlueprint entry)
        {
            if (!SafeFileName(entry.FileName)) throw new InvalidDataException("Invalid blueprint filename.");
            return Path.Combine(DirectoryPath, entry.FileName);
        }
    }
}
