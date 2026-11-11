using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SharableBlueprints
{
    public sealed partial class BlueprintLibraryStore
    {
        public BlueprintBook CreateBook(string name)
        {
            var book = new BlueprintBook { Id = Guid.NewGuid().ToString("N"), Name = Name(name) };
            Books.Add(book);
            try { SaveIndex(); } catch { Books.Remove(book); throw; }
            return book;
        }
    }
}
