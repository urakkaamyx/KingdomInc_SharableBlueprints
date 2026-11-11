using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SharableBlueprints
{
    public sealed partial class BlueprintLibraryStore
    {
        public LibraryBlueprint Add(string json, string name, string bookId, int buildings, int pipes, int decorations, BlueprintSuggestions suggestions = null, string iconName = null)
        {
            RequireBook(bookId);
            if (iconName != null && (iconName.Length > 256 || iconName.Any(char.IsControl))) throw new InvalidOperationException("Invalid blueprint icon.");
            var entry = new LibraryBlueprint { Id = Guid.NewGuid().ToString("N"), Name = Name(name), BookId = bookId ?? "",
                Buildings = buildings, Pipes = pipes, Decorations = decorations, CreatedUtc = DateTime.UtcNow,
                Suggestions = BlueprintSuggestions.Create(suggestions?.Labels, suggestions?.Icons), IconName = iconName ?? suggestions?.DefaultIcon ?? "" };
            entry.FileName = "Blueprint-" + entry.Id + ".kiblueprint.json";
            Directory.CreateDirectory(DirectoryPath);
            using (var writer = new StreamWriter(new FileStream(FilePath(entry), FileMode.CreateNew, FileAccess.Write))) writer.Write(json);
            Blueprints.Add(entry);
            try { SaveIndex(); }
            catch
            {
                // Keep the successful export; the next Load adopts it as an unfiled blueprint.
                Blueprints.Remove(entry); throw;
            }
            return entry;
        }
    }
}
