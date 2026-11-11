using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SharableBlueprints
{
    public sealed partial class BlueprintLibraryStore
    {
        public void Load()
        {
            var books = new List<BlueprintBook>(); var blueprints = new List<LibraryBlueprint>();
            if (File.Exists(IndexPath))
            {
                var root = XDocument.Load(IndexPath).Root;
                if (root == null || root.Name != "BlueprintLibrary" || (string)root.Attribute("version") != "1")
                    throw new InvalidDataException("The blueprint library index is not supported. Your blueprint files have been preserved.");
                foreach (var node in root.Element("Books")?.Elements("Book") ?? Enumerable.Empty<XElement>())
                {
                    string id = (string)node.Attribute("id");
                    if (string.IsNullOrWhiteSpace(id) || books.Any(b => b.Id == id)) throw new InvalidDataException("Invalid book in library index.");
                    books.Add(new BlueprintBook { Id = id, Name = Name((string)node.Attribute("name")) });
                }
                foreach (var node in root.Element("Blueprints")?.Elements("Blueprint") ?? Enumerable.Empty<XElement>())
                {
                    string file = (string)node.Attribute("file"), id = (string)node.Attribute("id");
                    if (!SafeFileName(file) || string.IsNullOrWhiteSpace(id) || blueprints.Any(b => b.Id == id || b.FileName == file))
                        throw new InvalidDataException("Invalid blueprint in library index.");
                    string book = (string)node.Attribute("book") ?? "";
                    blueprints.Add(new LibraryBlueprint { Id = id, Name = Name((string)node.Attribute("name")), FileName = file,
                        BookId = books.Any(b => b.Id == book) ? book : "", Buildings = (int?)node.Attribute("buildings") ?? 0,
                        Pipes = (int?)node.Attribute("pipes") ?? 0, Decorations = (int?)node.Attribute("decorations") ?? 0,
                        CreatedUtc = (DateTime?)node.Attribute("createdUtc") ?? DateTime.MinValue,
                        IconName = (string)node.Attribute("icon") ?? "",
                        Suggestions = BlueprintSuggestions.Create(node.Element("Labels")?.Elements("Label").Select(l => l.Value),
                            node.Element("Icons")?.Elements("Icon").Select(i => new BlueprintIconChoice { Id = (string)i.Attribute("id"), Name = (string)i.Attribute("name") })) });
                }
            }
            // Adopt exports from the earlier version without moving or rewriting them.
            if (Directory.Exists(DirectoryPath))
                foreach (string path in Directory.EnumerateFiles(DirectoryPath, "*.kiblueprint.json", SearchOption.TopDirectoryOnly).OrderBy(p => p))
                {
                    string file = Path.GetFileName(path);
                    if (blueprints.Any(b => string.Equals(b.FileName, file, StringComparison.OrdinalIgnoreCase))) continue;
                    string name = file.Substring(0, file.Length - ".kiblueprint.json".Length);
                    blueprints.Add(new LibraryBlueprint { Id = Guid.NewGuid().ToString("N"), FileName = file,
                        Name = string.IsNullOrWhiteSpace(name) ? "Imported blueprint" : name.Substring(0, Math.Min(name.Length, 80)),
                        BookId = "", CreatedUtc = File.GetCreationTimeUtc(path) });
                }
            Books = books; Blueprints = blueprints;
        }
    }
}
