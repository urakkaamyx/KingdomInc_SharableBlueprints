using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SharableBlueprints
{
    public sealed partial class BlueprintLibraryStore
    {
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
