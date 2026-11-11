using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SharableBlueprints
{
    public sealed partial class BlueprintLibraryStore
    {
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
    }
}
