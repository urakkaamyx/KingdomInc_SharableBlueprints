using System;
using SharableBlueprints;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LightJson;

if (args.Length == 2 && args[0] == "--validate")
{
    var checkedBlueprint = PortableBlueprint.Parse(File.ReadAllText(args[1]));
    Console.WriteLine($"Valid blueprint: {checkedBlueprint.Name}; {checkedBlueprint.Buildings} buildings, {checkedBlueprint.Pipes} pipes, {checkedBlueprint.Decorations} decorations.");
    return;
}

if (args.Length == 3 && args[0] == "--share-code-roundtrip")
{
    var imported = PortableBlueprint.Parse(File.ReadAllText(args[1]));
    string legacy = BlueprintShareCode.Encode(imported.Document.ToString(false));
    string compact = BlueprintShareCode.EncodeCompact(imported.Document.ToString(false));
    var reconstructed = PortableBlueprint.Parse(compact);
    if (!BlueprintCompactCode.Pack(imported.Document).SequenceEqual(BlueprintCompactCode.Pack(reconstructed.Document)))
        throw new Exception("Real blueprint share-code roundtrip changed the layout/settings.");
    File.WriteAllText(args[2], compact);
    Console.WriteLine("Imported " + imported.Name + ": legacy=" + legacy.Length + " chars; compact=" + compact.Length + " chars; Discord parts=" + BlueprintShareCode.ForDiscord(compact).Count);
    return;
}

int checks = 0;
void Check(bool condition, string message)
{
    checks++;
    if (!condition) throw new Exception(message);
}
foreach (var box in new[] {
    new SelectionBox(10, -90, 110, 10), new SelectionBox(110, 10, 10, -90),
    new SelectionBox(10, 10, 110, -90), new SelectionBox(110, -90, 10, 10) })
{
    Check(box.Left == 10 && box.Top == 10 && box.Width == 100 && box.Height == 100, "Drag direction changed bounds.");
    Check(box.Encloses(10, 10, 100, 100), "Exact frame must fit.");
    Check(box.Encloses(30, -10, 20, 30), "Nested frame must fit.");
    Check(!box.Encloses(9, 10, 100, 100), "Partial left frame included.");
    Check(!box.Encloses(10, 11, 100, 100), "Partial top frame included.");
    Check(!box.Encloses(10, 10, 101, 100), "Partial right frame included.");
    Check(!box.Encloses(10, 10, 100, 101), "Partial bottom frame included.");
    Check(!box.Encloses(10, 10, 0, 10), "Zero width included.");
}
Check(!new SelectionBox(0, 0, 0, 0).HasArea, "A click is an area.");
Check(!new SelectionBox(0, 0, 4, 100).HasArea, "Tiny width accepted.");
Check(new SelectionBox(0, 0, 5, 5).HasArea, "Minimum area rejected.");
var extreme = new SelectionBox(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue);
Check(extreme.Width == 4294967295L && extreme.Height == 4294967295L, "Bounds overflowed.");
Check(!extreme.Encloses(int.MaxValue, 0, 10, 10), "Right overflow included.");
Check(!extreme.Encloses(0, int.MinValue, 10, 10), "Bottom overflow included.");
var goldSelection = new SelectionBox(9978957, 10004092, 9984362, 10001481);
Check(!goldSelection.Encloses(9979008, 10004096, 5376, 2560), "GOLD fixture no longer reproduces the missed container border.");
Check(goldSelection.EnclosesContainer(9979008, 10004096, 5376, 2560), "Container four units above and 22 units beyond GOLD selection still omitted.");
var goldOutput = goldSelection.IncludeFrame(9979008, 10004096, 5376, 2560);
Check(goldOutput.Top == 10004096 && goldOutput.Left == goldSelection.Left && goldOutput.Height == goldSelection.Height + 4 && goldOutput.Width == goldSelection.Width + 22,
    "Output bounds did not expand to fit the captured container.");
Check(goldOutput.Encloses(9979008, 10004096, 5376, 2560), "Expanded output bounds do not contain the full frame.");
var containerBox = new SelectionBox(0, 0, 200, -200);
Check(containerBox.EnclosesContainer(0, 32, 200, 200), "Top border tolerance failed.");
Check(containerBox.EnclosesContainer(-32, 0, 200, 200), "Left border tolerance failed.");
Check(containerBox.EnclosesContainer(32, 0, 200, 200), "Right border tolerance failed.");
Check(containerBox.EnclosesContainer(0, -32, 200, 200), "Bottom border tolerance failed.");
Check(!containerBox.EnclosesContainer(0, 33, 200, 200), "Overlapping container outside the border tolerance included.");
Check(!containerBox.EnclosesContainer(-100, 100, 400, 400), "A much larger ancestor container was pulled into the selection.");
Check(containerBox.EnclosesContainer(20, -20, 128, 128), "Fully selected nested container omitted.");
Check(!new SelectionBox().EnclosesContainer(0, 0, 16, 16), "Empty click selected a container.");
Check(!containerBox.EnclosesContainer(0, 0, 0, 128), "Invalid container frame included.");
Check(!containerBox.Encloses(0, 16, 200, 200), "Container tolerance leaked into the building selection rule.");
Check(SelectionEdgePan.Axis(500, 1000, 50) == 0, "Camera pans in the screen center.");
Check(SelectionEdgePan.Axis(0, 1000, 50) == -1, "Left/bottom edge direction wrong.");
Check(SelectionEdgePan.Axis(1000, 1000, 50) == 1, "Right/top edge direction wrong.");
Check(SelectionEdgePan.Axis(25, 1000, 50) == -.5 && SelectionEdgePan.Axis(975, 1000, 50) == .5, "Edge acceleration not proportional.");
Check(SelectionEdgePan.Axis(50, 1000, 50) == 0 && SelectionEdgePan.Axis(950, 1000, 50) == 0, "Camera pans outside the edge zone.");
Check(SelectionEdgePan.Axis(-1, 1000, 50) == 0 && SelectionEdgePan.Axis(1001, 1000, 50) == 0, "Outside-window pointer pans camera.");
Check(SelectionEdgePan.Axis(25, 50, 100) == 0, "Opposite edge zones overlap on a small screen.");
Check(SelectionEdgePan.Axis(0, 0, 50) == 0 && SelectionEdgePan.Axis(0, 1000, 0) == 0, "Invalid screen dimensions pan camera.");
Check(SelectionEdgePan.Axis(double.NaN, 1000, 50) == 0 && SelectionEdgePan.Axis(0, double.PositiveInfinity, 50) == 0, "Invalid coordinates propagate to camera.");
var previewBuildings = new[] {
    new SelectionBuilding { Type = "Furnace", Name = "Furnace", X = 10, Y = 0, Width = 20, Height = 20, StackSize = 3 },
    new SelectionBuilding { Type = "Furnace", Name = "Furnace", X = 40, Y = 0, Width = 20, Height = 20 },
    new SelectionBuilding { Type = "Manifold", Name = "Manifold", X = 70, Y = 0, Width = 15, Height = 15 },
    new SelectionBuilding { Type = "Shop", Name = "Shop", X = 140, Y = 0, Width = 20, Height = 20 },
    new SelectionBuilding { Type = "Furnace", Name = "Furnace", X = 95, Y = 0, Width = 20, Height = 20 }
};
var preview = SelectionPreview.Count(previewBuildings, new SelectionBox(0, 20, 100, -100));
Check(preview.Nodes == 3 && preview.Buildings == 5, "Preview includes partial buildings or ignores stacks.");
Check(preview.Groups.Count == 2 && preview.Groups.Single(g => g.Type == "Furnace").Buildings == 4, "Repeated buildings not grouped.");
Check(preview.Groups.Single(g => g.Type == "Manifold").Buildings == 1, "Utility building missing from preview.");
Check(preview.Groups[0].Name == "Furnace" && preview.Groups[1].Name == "Manifold", "Preview order not stable.");
var extended = SelectionPreview.Count(previewBuildings, new SelectionBox(0, 20, 200, -100));
Check(extended.Nodes == 5 && extended.Buildings == 7 && extended.Groups.Any(g => g.Type == "Shop"), "Extended selection does not include offscreen buildings.");
var shrunken = SelectionPreview.Count(previewBuildings, new SelectionBox(0, 20, 35, -100));
Check(shrunken.Nodes == 1 && shrunken.Buildings == 3 && shrunken.Groups.Count == 1, "Shrinking selection retained stale groups/counts.");
Check(SelectionPreview.Count(previewBuildings, new SelectionBox()).Buildings == 0, "Click includes preview buildings.");
Check(SelectionPreview.Count(previewBuildings, new SelectionBox(200, -100, 0, 20)).Buildings == extended.Buildings, "Reversed drag changes preview counts.");
var sameNames = new[] {
    new SelectionBuilding { Type = "TypeA", Name = "Same name", X = 0, Y = 0, Width = 10, Height = 10, StackSize = 0 },
    new SelectionBuilding { Type = "TypeB", Name = "Same name", X = 0, Y = 0, Width = 10, Height = 10 }
};
var groupedTypes = SelectionPreview.Count(sameNames, new SelectionBox(0, 0, 20, -20));
Check(groupedTypes.Groups.Count == 2 && groupedTypes.Buildings == 2, "Localized labels merged distinct types, or invalid stack removed a building.");
var suggestions = BlueprintSuggestions.Create(new[] { "  Iron Works  ", "iron works", "Smelting\n Hall", " ", null, new string('x', 100) }, new[] {
    new BlueprintIconChoice { Id = "icon_iron", Name = "Iron" },
    new BlueprintIconChoice { Id = "icon_iron", Name = "Duplicate iron" },
    new BlueprintIconChoice { Id = "icon_furnace", Name = "Furnace" },
    new BlueprintIconChoice { Id = "", Name = "Empty" }, null });
Check(suggestions.Labels.Count == 3 && suggestions.Labels[0] == "Iron Works" && suggestions.Labels[1] == "Smelting Hall", "Label suggestions not normalized/deduplicated in selection order.");
Check(suggestions.Labels[2].Length == 80, "Long label not shortened for the name field.");
Check(suggestions.DefaultName("Fallback") == "Iron Works", "Automatic label name not selected.");
Check(suggestions.Icons.Count == 2 && suggestions.DefaultIcon == "icon_iron", "Selection icons not deduplicated or defaulted.");
Check(BlueprintSuggestions.Create(null, null).DefaultName("Blueprint 1") == "Blueprint 1", "No-label fallback not preserved.");
Check(BlueprintSuggestions.Create(null, null).DefaultIcon == "", "No-icon fallback not schematic.");
Check(BlueprintSuggestions.Create(new[] { new string('x', 79) + "😀" }, null).Labels.Single().Length == 79, "Label truncation split a Unicode character.");
JsonObject PortableFixture()
{
    JsonObject Building(string id, int x, int stack) => new JsonObject {
        {"Guid", id}, {"ElementType", "Forge"}, {"Position", new JsonArray(x, 0)}, {"StackSize", stack},
        {"Inputs", new JsonArray {new JsonObject {{"Position", 0}, {"BufferedItems", new JsonArray()}}}},
        {"Outputs", new JsonArray {new JsonObject {{"Position", 0}, {"BufferedItems", new JsonArray()}}}},
        {"Recipe", "locked_recipe"}, {"Upgrades", new JsonArray()} };
    return new JsonObject { {"format", "kingdominc.sharable-blueprint"}, {"formatVersion", 1}, {"name", "Future factory"},
        {"layout", new JsonObject {
            {"Elements", new JsonArray {Building("one", 0, 3), Building("two", 300, 1)}},
            {"Pipes", new JsonArray {new JsonObject {
                {"ConnectedOutput", new JsonObject {{"Guid", "one"}, {"Index", 0}}},
                {"ConnectedInput", new JsonObject {{"Guid", "two"}, {"Index", 0}}}, {"Nodes", new JsonArray {new JsonArray(200, -40)}} }}},
            {"Decorations", new JsonArray {new JsonObject {{"Position", new JsonArray(0, 40)}, {"Size", new JsonArray(120, 30)}, {"Label", "Future factory"}}}}
        }} };
}
var portable = PortableBlueprint.Parse(PortableFixture().ToString());
string shareJson = PortableFixture().ToString();
string shareCode = BlueprintShareCode.Encode(shareJson);
Check(shareCode.StartsWith(BlueprintShareCode.Prefix), "Share code prefix missing.");
Check(BlueprintShareCode.Decode(shareCode) == shareJson, "Gzip Base64 round trip changed JSON.");
Check(PortableBlueprint.Parse(shareCode).Buildings == 4, "Share code cannot be imported into library.");
Check(PortableBlueprint.Parse("```base64\n" + shareCode + "\n```").Buildings == 4, "Discord code block cannot be imported.");
string plainCode = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(shareJson));
Check(BlueprintShareCode.Decode(plainCode.TrimEnd('=')) == shareJson, "Unpadded plain Base64 cannot be decoded.");
Check(BlueprintShareCode.Decode(plainCode.Replace('+', '-').Replace('/', '_')) == shareJson, "URL-safe Base64 cannot be decoded.");
string unicodeJson = shareJson.Replace("Future factory", "Fábrica 🏰");
Check(BlueprintShareCode.Decode(BlueprintShareCode.Encode(unicodeJson)) == unicodeJson, "Share code lost Unicode text.");
string compactCode = BlueprintShareCode.EncodeCompact(shareJson);
var compactDocument = PortableBlueprint.Parse(compactCode).Document;
Check(compactCode.StartsWith(BlueprintShareCode.CompactPrefix), "Compact code prefix missing.");
Check(BlueprintCompactCode.Pack(PortableFixture()).SequenceEqual(BlueprintCompactCode.Pack(compactDocument)), "Compact roundtrip changed settings, geometry, decoration or connections.");
Check(PortableBlueprint.Parse("```\n" + compactCode + "\n```").Buildings == 4, "Discord-wrapped compact code cannot be imported.");
var oddLayout = PortableFixture();
oddLayout["layout"]["Elements"][0].AsJsonObject["Position"] = new JsonArray(-73, 231);
oddLayout["layout"]["Elements"][1].AsJsonObject["Position"] = new JsonArray(314, -99);
oddLayout["layout"]["Pipes"][0].AsJsonObject["Nodes"] = new JsonArray {new JsonArray(-153, 31), new JsonArray(111, -443)};
oddLayout["layout"]["Elements"][0].AsJsonObject["CustomSetting"] = new JsonObject {{"Nested", new JsonArray(1, 2, 3)}};
Check(BlueprintCompactCode.Pack(oddLayout).SequenceEqual(BlueprintCompactCode.Pack(PortableBlueprint.Parse(BlueprintShareCode.EncodeCompact(oddLayout.ToString())).Document)),
    "Compact code snapped or changed irregular coordinates or custom settings.");
var discordParts = BlueprintShareCode.ForDiscord(compactCode, 150);
Check(discordParts.Count > 1 && discordParts.All(p => p.Length + 8 <= 2000), "Discord parts exceed the message budget.");
string joined = string.Join("\n", discordParts.AsEnumerable().Reverse().Select(p => "```\n" + p + "\n```"));
Check(BlueprintCompactCode.Pack(compactDocument).SequenceEqual(BlueprintCompactCode.Pack(PortableBlueprint.Parse(joined).Document)), "Out-of-order Discord parts did not reconstruct the layout.");
Check(BlueprintShareCode.ForDiscord(new string('x', 1956)).Count == 1, "One-message Discord code was split unnecessarily.");
for (int size = 1; size <= 32; size++)
{
    byte[] bytes = Enumerable.Range(0, size).Select(i => (byte)(i * 53)).ToArray();
    Check(BlueprintBase85.Decode(BlueprintBase85.Encode(bytes)).SequenceEqual(bytes), "Base85 block/padding roundtrip failed.");
}
void RejectCode(string code, string message)
{
    bool rejected = false;
    try { PortableBlueprint.Parse(code); } catch (InvalidDataException) { rejected = true; }
    Check(rejected, message);
}
RejectCode("not a blueprint!", "Invalid Base64 was accepted.");
RejectCode(string.Join("\n", discordParts.Skip(1)), "Missing Discord part accepted.");
RejectCode(joined + "\n" + discordParts[0], "Duplicate Discord part accepted.");
var otherParts = BlueprintShareCode.ForDiscord(BlueprintShareCode.EncodeCompact(unicodeJson), 150);
RejectCode(discordParts[0] + "\n" + otherParts[0], "Parts from two blueprints accepted.");
RejectCode(BlueprintShareCode.CompactPrefix + "4abcde", "Invalid compact padding accepted.");
RejectCode(BlueprintShareCode.CompactPrefix + "0#####", "Overflowing Base85 block accepted.");
byte[] compressed = Convert.FromBase64String(shareCode.Substring(BlueprintShareCode.Prefix.Length));
RejectCode(BlueprintShareCode.Prefix + Convert.ToBase64String(compressed.Take(compressed.Length - 4).ToArray()), "Truncated gzip was accepted.");
compressed[compressed.Length - 8] ^= 1;
RejectCode(BlueprintShareCode.Prefix + Convert.ToBase64String(compressed), "Corrupted gzip checksum was accepted.");
using (var bombOutput = new MemoryStream())
{
    using (var gzip = new System.IO.Compression.GZipStream(bombOutput, System.IO.Compression.CompressionLevel.Optimal, true))
    {
        byte[] block = new byte[8192];
        for (int i = 0; i <= 1024; i++) gzip.Write(block, 0, block.Length);
    }
    RejectCode(BlueprintShareCode.Prefix + Convert.ToBase64String(bombOutput.ToArray()), "Oversized gzip expansion was accepted.");
}
Check(portable.Name == "Future factory" && portable.Buildings == 4 && portable.Pipes == 1 && portable.Decorations == 1, "Portable blueprint did not retain counts/name.");
Check(portable.Document["layout"]["Elements"][0]["Recipe"].AsString == "locked_recipe", "Import changed a locked recipe setting.");
var futureDocument = PortableFixture(); futureDocument["layout"]["Elements"][0].AsJsonObject["ElementType"] = "FutureModBuilding";
var futurePortable = PortableBlueprint.Parse(futureDocument.ToString());
Check(futurePortable.Buildings == 4, "Storage parser incorrectly requires building research/types from this kingdom.");
void RejectPortable(Action<JsonObject> mutation, string message)
{
    var document = PortableFixture(); mutation(document); bool rejected = false;
    try { PortableBlueprint.Parse(document.ToString()); } catch (InvalidDataException) { rejected = true; }
    Check(rejected, message);
}
RejectPortable(d => d["formatVersion"] = 2, "Unsupported format version accepted.");
RejectPortable(d => d["format"] = "save-game", "Non-blueprint format accepted.");
RejectPortable(d => d["layout"]["Elements"][1].AsJsonObject["Guid"] = "one", "Repeated building GUID accepted.");
RejectPortable(d => d["layout"]["Elements"][0].AsJsonObject["StackSize"] = 6, "Oversized stack accepted.");
RejectPortable(d => d["layout"]["Elements"][0].AsJsonObject["StackSize"] = 1.5, "Fractional stack accepted.");
RejectPortable(d => d["layout"]["Elements"][0]["Position"].AsJsonArray[0] = .5, "Fractional position accepted.");
RejectPortable(d => d["layout"]["Pipes"][0]["ConnectedInput"].AsJsonObject["Guid"] = "missing", "Connection to missing node accepted.");
RejectPortable(d => d["layout"]["Pipes"][0]["ConnectedInput"].AsJsonObject["Index"] = -1, "Negative port accepted.");
RejectPortable(d => d["layout"]["Pipes"][0]["ConnectedInput"].AsJsonObject["Index"] = 3, "Connection to missing port accepted.");
RejectPortable(d => d["layout"]["Pipes"].AsJsonArray.Add(d["layout"]["Pipes"][0]), "Duplicate use of a port accepted.");
RejectPortable(d => d["layout"]["Decorations"][0]["Size"].AsJsonArray[0] = 0, "Zero decoration size accepted.");
RejectPortable(d => d["layout"].AsJsonObject["Elements"] = new JsonArray(), "Empty building list accepted.");
bool invalidPaste = false; try { PortableBlueprint.Parse("not JSON"); } catch (InvalidDataException) { invalidPaste = true; }
Check(invalidPaste, "Invalid paste not rejected.");
var references = PortableBlueprint.References(new JsonObject {
    {"Guid", "ignore_guid"}, {"Label", "ignore_label"}, {"IconName", "ignore_icon"},
    {"Recipe", "iron_sword"}, {"Resource", "iron_ore"}, {"Upgrades", new JsonArray {new JsonObject {{"Item", "iron_ingot"}}}}
}).ToList();
Check(references.Count == 3 && references.Any(r => r.key == "Recipe" && r.value == "iron_sword")
    && references.Any(r => r.value == "iron_ingot"), "Requirement scanner missed nested items or included cosmetic metadata.");
var missingResearch = new BlueprintResearchReport();
Check(missingResearch.CanPlace, "No-lock blueprint blocked.");
missingResearch.Add("forge", "Forge research", "Unlock the forge.", "Forge A");
missingResearch.Add("forge", "Forge research", "Unlock the forge.", "Forge B");
Check(!missingResearch.CanPlace && missingResearch.Issues.Count == 1 && missingResearch.Issues.Single().Affected.Count == 2,
    "Missing research did not block the whole blueprint or was duplicated per building.");
string testRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "SharableBlueprints-tests-" + Guid.NewGuid().ToString("N")));
try
{
    var deleteStore = new BlueprintLibraryStore(Path.Combine(testRoot, "Deletion")); deleteStore.Load();
    var deleteBook = deleteStore.CreateBook("Delete this book"); var keepBook = deleteStore.CreateBook("Keep this book");
    var deleteEntry = deleteStore.Add("recoverable payload", "Delete me", deleteBook.Id, 1, 0, 0);
    var keepEntry = deleteStore.Add("keep payload", "Keep me", deleteBook.Id, 1, 0, 0);
    var unrelatedEntry = deleteStore.Add("unrelated payload", "Unrelated", keepBook.Id, 1, 0, 0);
    File.WriteAllText(deleteStore.FilePath(deleteEntry) + ".bak", "previous payload");
    string archived = deleteStore.DeleteBlueprint(deleteEntry.Id);
    Check(!File.Exists(deleteStore.FilePath(deleteEntry)) && File.ReadAllText(Path.Combine(archived, deleteEntry.FileName)) == "recoverable payload",
        "Delete did not archive the selected blueprint payload.");
    Check(File.ReadAllText(Path.Combine(archived, deleteEntry.FileName + ".bak")) == "previous payload" && File.Exists(Path.Combine(archived, "Library-before-delete.xml")),
        "Delete lost the previous payload or recovery metadata.");
    deleteStore.Load(); deleteStore.Load();
    Check(deleteStore.Blueprints.Count == 2 && !deleteStore.Blueprints.Any(b => b.Id == deleteEntry.Id), "Deleted blueprint reappeared on library reload.");
    Check(File.ReadAllText(deleteStore.FilePath(keepEntry)) == "keep payload" && deleteStore.Books.Count == 2, "Deleting a blueprint changed other entries/books.");
    deleteStore.DeleteBook(deleteBook.Id); deleteStore.Load();
    Check(deleteStore.Books.Single().Id == keepBook.Id && deleteStore.Blueprints.Single(b => b.Id == keepEntry.Id).BookId == "",
        "Book deletion did not preserve contents in Unfiled.");
    Check(deleteStore.Blueprints.Single(b => b.Id == unrelatedEntry.Id).BookId == keepBook.Id && File.ReadAllText(deleteStore.FilePath(unrelatedEntry)) == "unrelated payload",
        "Book deletion changed unrelated membership or files.");
    bool deleteRejected = false;
    try { deleteStore.DeleteBlueprint("missing"); } catch (InvalidOperationException) { deleteRejected = true; }
    Check(deleteRejected && deleteStore.Blueprints.Count == 2, "Deleting an unknown blueprint changed the library.");
    if (OperatingSystem.IsWindows())
    {
        bool blocked = false;
        using (var locked = new FileStream(Path.Combine(deleteStore.DirectoryPath, "Library.xml"), FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            try { deleteStore.DeleteBlueprint(unrelatedEntry.Id); } catch (IOException) { blocked = true; }
        }
        Check(blocked && deleteStore.Blueprints.Any(b => b.Id == unrelatedEntry.Id) && File.ReadAllText(deleteStore.FilePath(unrelatedEntry)) == "unrelated payload",
            "Failed deletion did not restore the blueprint file and index entry.");
        blocked = false;
        using (var locked = new FileStream(Path.Combine(deleteStore.DirectoryPath, "Library.xml"), FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            try { deleteStore.DeleteBook(keepBook.Id); } catch (IOException) { blocked = true; }
        }
        Check(blocked && deleteStore.Books.Single().Id == keepBook.Id && deleteStore.Blueprints.Single(b => b.Id == unrelatedEntry.Id).BookId == keepBook.Id,
            "Failed book deletion did not restore book membership.");
    }
    File.Delete(deleteStore.FilePath(keepEntry)); deleteStore.DeleteBlueprint(keepEntry.Id); deleteStore.Load();
    Check(deleteStore.Blueprints.Single().Id == unrelatedEntry.Id, "An already missing blueprint file could not be removed from the list.");
    var importedLibrary = new BlueprintLibraryStore(Path.Combine(testRoot, "Imports")); importedLibrary.Load();
    importedLibrary.Add(futurePortable.Document.ToString(), futurePortable.Name, "", futurePortable.Buildings, futurePortable.Pipes, futurePortable.Decorations);
    importedLibrary.Load();
    Check(!missingResearch.CanPlace && importedLibrary.Blueprints.Single().Name == "Future factory", "Research lock prevented storing/importing a blueprint.");
    var library = new BlueprintLibraryStore(testRoot);
    library.Load();
    Check(library.Books.Count == 0 && library.Blueprints.Count == 0, "New library not empty.");
    var book = library.CreateBook("Ore & Iron <Plans>");
    string content = "{\"format\":\"kingdominc.sharable-blueprint\",\"name\":\"Smelter\"}";
    var entry = library.Add(content, "Smelter", book.Id, 12, 14, 2, suggestions);
    var reloaded = new BlueprintLibraryStore(testRoot); reloaded.Load();
    Check(reloaded.Books.Single().Name == "Ore & Iron <Plans>", "Book name did not survive XML roundtrip.");
    var stored = reloaded.Blueprints.Single();
    Check(stored.Id == entry.Id && stored.BookId == book.Id && stored.Buildings == 12 && stored.Pipes == 14 && stored.Decorations == 2,
        "Blueprint metadata did not survive reload.");
    Check(File.ReadAllText(reloaded.FilePath(stored)) == content, "Stored blueprint payload changed.");
    Check(stored.IconName == "icon_iron" && stored.Suggestions.Labels.SequenceEqual(suggestions.Labels)
        && stored.Suggestions.Icons.Select(i => i.Id).SequenceEqual(suggestions.Icons.Select(i => i.Id)), "Icon or suggestion choices not persistent.");
    string edited = "{\"format\":\"kingdominc.sharable-blueprint\",\"name\":\"My manual name\",\"icon\":\"icon_gold\"}";
    reloaded.UpdateMetadata(entry.Id, "My manual name", "icon_gold", edited); reloaded.Load();
    Check(reloaded.Blueprints.Single().Name == "My manual name" && reloaded.Blueprints.Single().IconName == "icon_gold"
        && File.ReadAllText(reloaded.FilePath(stored)) == edited, "Manual name/icon not saved with portable payload.");
    Check(File.ReadAllText(reloaded.FilePath(stored) + ".bak") == content, "Previous portable payload not backed up.");
    if (OperatingSystem.IsWindows())
    {
        bool refused = false;
        using (var lockedIndex = new FileStream(Path.Combine(testRoot, "Library.xml"), FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            try { reloaded.UpdateMetadata(entry.Id, "Should roll back", "bad_icon", "attempted payload"); }
            catch (IOException) { refused = true; }
        }
        Check(refused && reloaded.Blueprints.Single().Name == "My manual name" && reloaded.Blueprints.Single().IconName == "icon_gold"
            && File.ReadAllText(reloaded.FilePath(stored)) == edited, "Failed index write did not restore name, icon and payload.");
    }
    reloaded.UpdateMetadata(entry.Id, "Schematic choice", "", edited); reloaded.Load();
    Check(reloaded.Blueprints.Single().IconName == "", "Clearing the selected icon did not persist.");
    reloaded.MoveBlueprint(entry.Id, "");
    reloaded.RenameBlueprint(entry.Id, "Iron plates");
    reloaded.RenameBook(book.Id, "Metals");
    reloaded.Load();
    Check(reloaded.Blueprints.Single().BookId == "" && reloaded.Blueprints.Single().Name == "Iron plates", "Move/name not persistent.");
    Check(reloaded.Books.Single().Name == "Metals", "Book rename not persistent.");
    Check(File.Exists(Path.Combine(testRoot, "Library.xml.bak")), "Previous index backup not kept.");
    int filesBefore = Directory.GetFiles(testRoot).Length;
    bool rejected = false;
    try { reloaded.Add(content, "Wrong book", "missing", 1, 0, 0); } catch (InvalidOperationException) { rejected = true; }
    Check(rejected && Directory.GetFiles(testRoot).Length == filesBefore, "Invalid book wrote an export.");
    rejected = false;
    try { reloaded.MoveBlueprint(entry.Id, "missing"); } catch (InvalidOperationException) { rejected = true; }
    Check(rejected && reloaded.Blueprints.Single().BookId == "", "Invalid destination changed membership.");
    rejected = false;
    try { reloaded.RenameBlueprint(entry.Id, "   "); } catch (InvalidOperationException) { rejected = true; }
    Check(rejected && reloaded.Blueprints.Single().Name == "Iron plates", "Blank rename changed blueprint.");
    rejected = false;
    try { BlueprintLibraryStore.Name(new string('x', 81)); } catch (InvalidOperationException) { rejected = true; }
    Check(rejected, "Oversized name accepted.");
    string legacy = Path.Combine(testRoot, "Old-export.kiblueprint.json"); File.WriteAllText(legacy, content);
    reloaded.Load();
    Check(reloaded.Blueprints.Count == 2 && reloaded.Blueprints.Any(b => b.Name == "Old-export" && b.BookId == ""), "Earlier export not adopted.");
    reloaded.CreateBook("Adoption checkpoint"); reloaded.Load(); reloaded.Load();
    Check(reloaded.Blueprints.Count == 2 && File.ReadAllText(legacy) == content, "Earlier export duplicated or rewritten.");
    string index = Path.Combine(testRoot, "Library.xml"), before = File.ReadAllText(index);
    File.WriteAllText(index, "broken index"); rejected = false;
    try { reloaded.Load(); } catch (System.Xml.XmlException) { rejected = true; }
    Check(rejected && File.ReadAllText(index) == "broken index" && File.ReadAllText(legacy) == content, "Corrupt index was silently overwritten.");
    File.WriteAllText(index, before);
    var document = XDocument.Load(index); document.Root.Element("Blueprints").Elements("Blueprint").First().SetAttributeValue("file", "../escape.kiblueprint.json");
    document.Save(index); rejected = false;
    try { reloaded.Load(); } catch (InvalidDataException) { rejected = true; }
    Check(rejected, "Index allowed a path outside the library.");
    File.WriteAllText(index, before);
    File.Delete(legacy); reloaded.Load();
    Check(reloaded.Blueprints.Count == 2, "Missing export silently removed library metadata.");
}
finally
{
    // Remove only this run's freshly created, explicitly named temporary directory.
    if (Path.GetDirectoryName(testRoot) == Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar)
        && Path.GetFileName(testRoot).StartsWith("SharableBlueprints-tests-", StringComparison.Ordinal)
        && Directory.Exists(testRoot)) Directory.Delete(testRoot, true);
}
Console.WriteLine($"Passed {checks} selection, import-validation, research-policy and library-persistence checks.");
