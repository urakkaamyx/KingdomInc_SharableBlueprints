using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SharableBlueprints
{
    public sealed partial class BlueprintLibraryStore
    {
        public void RenameBlueprint(string id, string name)
        {
            var entry = Blueprints.First(b => b.Id == id); string before = entry.Name; entry.Name = Name(name);
            try { SaveIndex(); } catch { entry.Name = before; throw; }
        }
    }
}
