using KingdomFoundation.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SharableBlueprints
{
    public sealed class LibraryBlueprint
    {
        public string Id, Name, FileName, BookId;
        public string IconName = "";
        public BlueprintSuggestions Suggestions = new BlueprintSuggestions();
        public int Buildings, Pipes, Decorations;
        public DateTime CreatedUtc;
    }
}
