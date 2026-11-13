using KingdomFoundation.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using KingdomFoundation.Json;
using UnityEngine;

namespace SharableBlueprints
{
    public sealed class CapturedBlueprint
    {
        public JsonObject Document;
        public int Buildings, Pipes, Decorations;
        public BlueprintSuggestions Suggestions;
    }
}
