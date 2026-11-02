using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SharableBlueprints
{
    public sealed class BlueprintBook
    {
        public string Id, Name;
    }
    public sealed class LibraryBlueprint
    {
        public string Id, Name, FileName, BookId;
        public string IconName = "";
        public BlueprintSuggestions Suggestions = new BlueprintSuggestions();
        public int Buildings, Pipes, Decorations;
        public DateTime CreatedUtc;
    }

    // The library index is separate from both game saves and portable blueprint files.
    public sealed class BlueprintLibraryStore
    {
        public string DirectoryPath { get; }
        public List<BlueprintBook> Books { get; private set; } = new List<BlueprintBook>();
        public List<LibraryBlueprint> Blueprints { get; private set; } = new List<LibraryBlueprint>();
        string IndexPath => Path.Combine(DirectoryPath, "Library.xml");
        public BlueprintLibraryStore(string directory) { DirectoryPath = Path.GetFullPath(directory); }

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

        public BlueprintBook CreateBook(string name)
        {
            var book = new BlueprintBook { Id = Guid.NewGuid().ToString("N"), Name = Name(name) };
            Books.Add(book);
            try { SaveIndex(); } catch { Books.Remove(book); throw; }
            return book;
        }
        public void RenameBook(string id, string name)
        {
            var book = Books.First(b => b.Id == id); string before = book.Name; book.Name = Name(name);
            try { SaveIndex(); } catch { book.Name = before; throw; }
        }
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
        public void RenameBlueprint(string id, string name)
        {
            var entry = Blueprints.First(b => b.Id == id); string before = entry.Name; entry.Name = Name(name);
            try { SaveIndex(); } catch { entry.Name = before; throw; }
        }
        public void MoveBlueprint(string id, string bookId)
        {
            RequireBook(bookId);
            var entry = Blueprints.First(b => b.Id == id); string before = entry.BookId; entry.BookId = bookId ?? "";
            try { SaveIndex(); } catch { entry.BookId = before; throw; }
        }
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
        public void DeleteBook(string id)
        {
            var book = Books.FirstOrDefault(b => b.Id == id) ?? throw new InvalidOperationException("This book no longer exists.");
            int position = Books.IndexOf(book);
            var contents = Blueprints.Where(b => b.BookId == id).ToList();
            Books.RemoveAt(position);
            foreach (var entry in contents) entry.BookId = "";
            try { SaveIndex(); }
            catch
            {
                Books.Insert(position, book);
                foreach (var entry in contents) entry.BookId = id;
                throw;
            }
        }
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
        static void WriteAtomic(string file, string contents)
        {
            string temporary = file + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                File.WriteAllText(temporary, contents);
                if (File.Exists(file)) File.Replace(temporary, file, file + ".bak"); else File.Move(temporary, file);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
        public string FilePath(LibraryBlueprint entry)
        {
            if (!SafeFileName(entry.FileName)) throw new InvalidDataException("Invalid blueprint filename.");
            return Path.Combine(DirectoryPath, entry.FileName);
        }
        void RequireBook(string id)
        {
            if (!string.IsNullOrEmpty(id) && !Books.Any(b => b.Id == id)) throw new InvalidOperationException("That blueprint book no longer exists.");
        }
        static bool SafeFileName(string file) => !string.IsNullOrWhiteSpace(file) && file == Path.GetFileName(file)
            && file.IndexOfAny(new[] { '/', '\\', ':' }) < 0 && file.EndsWith(".kiblueprint.json", StringComparison.OrdinalIgnoreCase);
        public static string Name(string value)
        {
            value = (value ?? "").Trim();
            if (value.Length == 0 || value.Length > 80 || value.Any(char.IsControl))
                throw new InvalidOperationException("Use a name between 1 and 80 characters.");
            return value;
        }
        void SaveIndex()
        {
            Directory.CreateDirectory(DirectoryPath);
            var blueprintNodes = Blueprints.Select(b => new XElement("Blueprint", new XAttribute("id", b.Id),
                    new XAttribute("name", b.Name), new XAttribute("file", b.FileName), new XAttribute("book", b.BookId ?? ""),
                    new XAttribute("buildings", b.Buildings), new XAttribute("pipes", b.Pipes), new XAttribute("decorations", b.Decorations),
                    new XAttribute("createdUtc", b.CreatedUtc), new XAttribute("icon", b.IconName ?? ""),
                    new XElement("Labels", b.Suggestions.Labels.Select(l => new XElement("Label", l))),
                    new XElement("Icons", b.Suggestions.Icons.Select(i => new XElement("Icon", new XAttribute("id", i.Id), new XAttribute("name", i.Name))))));
            var document = new XDocument(new XElement("BlueprintLibrary", new XAttribute("version", 1),
                new XElement("Books", Books.Select(b => new XElement("Book", new XAttribute("id", b.Id), new XAttribute("name", b.Name)))),
                new XElement("Blueprints", blueprintNodes)));
            string temporary = IndexPath + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                document.Save(temporary);
                if (File.Exists(IndexPath)) File.Replace(temporary, IndexPath, IndexPath + ".bak");
                else File.Move(temporary, IndexPath);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
    }
}
