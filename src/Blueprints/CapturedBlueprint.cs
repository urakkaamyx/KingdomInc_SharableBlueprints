using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Source.Flow;
using LightJson;
using UnityEngine;

namespace SharableBlueprints
{
    public sealed partial class CapturedBlueprint
    {
        public JsonObject Document;
        public int Buildings, Pipes, Decorations;
        public BlueprintSuggestions Suggestions;
    }
}
