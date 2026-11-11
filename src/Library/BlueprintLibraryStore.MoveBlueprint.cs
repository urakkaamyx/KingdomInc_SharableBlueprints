using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SharableBlueprints
{
    public sealed partial class BlueprintLibraryStore
    {
        public void MoveBlueprint(string id, string bookId)
        {
            RequireBook(bookId);
            var entry = Blueprints.First(b => b.Id == id); string before = entry.BookId; entry.BookId = bookId ?? "";
            try { SaveIndex(); } catch { entry.BookId = before; throw; }
        }
    }
}
