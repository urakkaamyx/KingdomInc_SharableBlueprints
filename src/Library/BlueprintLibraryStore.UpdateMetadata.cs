using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SharableBlueprints
{
    public sealed partial class BlueprintLibraryStore
    {
        public void UpdateMetadata(string id, string name, string iconName, string updatedJson)
        {
            name = Name(name); iconName = iconName ?? "";
            if (iconName.Length > 256 || iconName.Any(char.IsControl)) throw new InvalidOperationException("Invalid blueprint icon.");
            var entry = Blueprints.First(b => b.Id == id);
            string file = FilePath(entry), originalJson = File.ReadAllText(file), oldName = entry.Name, oldIcon = entry.IconName;
            WriteAtomic(file, updatedJson);
            entry.Name = name; entry.IconName = iconName;
            try { SaveIndex(); }
            catch (Exception error)
            {
                entry.Name = oldName; entry.IconName = oldIcon;
                try { WriteAtomic(file, originalJson); }
                catch (Exception rollback) { throw new AggregateException("Could not restore blueprint metadata. Previous file is retained as .bak.", error, rollback); }
                throw;
            }
        }
    }
}
