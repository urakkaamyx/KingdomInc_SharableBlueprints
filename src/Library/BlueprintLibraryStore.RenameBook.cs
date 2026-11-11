using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SharableBlueprints
{
    public sealed partial class BlueprintLibraryStore
    {
        public void RenameBook(string id, string name)
        {
            var book = Books.First(b => b.Id == id); string before = book.Name; book.Name = Name(name);
            try { SaveIndex(); } catch { book.Name = before; throw; }
        }
    }
}
