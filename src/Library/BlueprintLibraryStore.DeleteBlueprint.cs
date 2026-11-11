using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SharableBlueprints
{
    public sealed partial class BlueprintLibraryStore
    {
        public string DeleteBlueprint(string id)
        {
            var entry = Blueprints.FirstOrDefault(b => b.Id == id) ?? throw new InvalidOperationException("This blueprint no longer exists.");
            string file = FilePath(entry);
            string archive = Path.Combine(DirectoryPath, "_Deleted", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(archive);
            if (File.Exists(IndexPath)) File.Copy(IndexPath, Path.Combine(archive, "Library-before-delete.xml"));
            var moved = new List<(string original, string archived)>();
            int position = Blueprints.IndexOf(entry);
            try
            {
                foreach (string candidate in new[] { file, file + ".bak" })
                {
                    if (!File.Exists(candidate)) continue;
                    string archived = Path.Combine(archive, Path.GetFileName(candidate));
                    File.Move(candidate, archived); moved.Add((candidate, archived));
                }
                Blueprints.RemoveAt(position); SaveIndex();
                return archive;
            }
            catch (Exception error)
            {
                if (!Blueprints.Contains(entry)) Blueprints.Insert(position, entry);
                try { foreach (var item in moved.AsEnumerable().Reverse()) File.Move(item.archived, item.original); }
                catch (Exception rollback) { throw new AggregateException("Deletion failed. The recoverable files are in " + archive, error, rollback); }
                throw;
            }
        }
    }
}
