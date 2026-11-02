# Sharable Blueprints — 0.5.2

0.5.2 gives the book sidebar more width and a taller list. Its footer now contains just **New Book** and **Manage Book**. Creating and renaming books use a full-size name dialog; deleting a book is available inside Manage Book, with the existing confirmation and content-preservation behavior. Built-in All Blueprints and Unfiled cannot be renamed or deleted.

0.5.1 restyles the blueprint manager using the game's own loaded panel, gold button, section header, item-slot and scrollbar sprites, including their native nine-slice borders. A larger, screen-fitted layout separates book management, blueprint browsing and the selected blueprint's details. Buttons stay on one line, the inspector has room for metadata and actions, long list names have tooltips, and all pickers share the same centered style. The selection summary uses the native frame too. No game artwork is bundled with the mod.

0.5.0 replaces the default clipboard output with compact `KIB2:` codes. Repeated building settings are stored once, connection GUIDs become short local indices, and exact coordinates use variable-length integers before gzip and Base85 text encoding. The saved JSON format remains unchanged. Labels, building/upgrade settings, container frames, port mirroring, pipe bends, name and chosen icon are retained. Suggestions are rebuilt on import from the layout; export timestamps and producer version are omitted from codes.

The copied code is wrapped in a Discord code block. Codes fitting one message are copied directly, with a character count. Larger builds open **Share Blueprint in Discord**, with a **Copy Part** button for each message. Send each part separately; recipients paste all parts together into **Paste Blueprint**. Parts can be in any order; missing, duplicate, mixed or damaged parts are rejected. Each message, including the code-block wrapper, stays below Discord's standard 2,000-character limit. Legacy Base64 codes and raw JSON still import. Recipients need mod 0.5.0 or later for KIB2 codes.

The locally captured GOLD test blueprint shrank from 6,710 characters to 1,956 (1,964 including the Discord wrapper), fitting one message. Arbitrarily large factories cannot be guaranteed to fit one message; the part picker handles those without requiring a hosted service. See [KIB2 share-code format](#kib2-share-code-format) for the interchange specification. The companion Discord viewer decoder also accepts KIB2 and combined message parts; restart its process if already running.

0.4.4 adds **Delete** beside the selected blueprint's Save button and beside Rename in the book panel. Both require confirmation. Deleting a blueprint removes it from the library and moves its file, previous payload and recovery index into `Blueprints/_Deleted/<timestamp-id>/`; it will not reappear on reload. Deleting a book preserves its blueprints in Unfiled. Failed index writes restore the files and membership.

0.4.3 includes near-edge container frames when the selection misses their border by up to 32 world units (half a placement grid cell). The live selection panel now shows container and label/icon counts. Captures expand their output bounds to retain the complete frame, while building selection still uses the original box. Nested containers are evaluated separately; large partly selected ancestors are excluded.

0.4.2 fixes the area-selection rectangle appearing far above the cursor. The overlay now inherits the native clone tool's anchors, pivot and transform. The drag anchor also samples the current mouse projection at press time.

A separate Kingdom .inc BepInEx mod with a parchment schematic button in the bottom-left edit toolbar. It uses the game's button frame, tooltip and selected border, plus an original transparent pixel-style medieval icon.

Click the schematic to open the blueprint library. It has a scrollable book list, blueprint search and a details panel. Create and rename books, rename blueprints, and use **Move to Book** to organize them. **All Blueprints** shows the whole library; **Unfiled** holds plans without a book. Earlier exports are automatically listed under Unfiled.

**Paste Blueprint** opens a text box, initially filled from the clipboard. Paste a compact share code, all numbered message parts, a legacy Base64 code or shared `.kiblueprint.json` text and choose **Store Blueprint**. Import checks the document format and layout structure, but does not require its buildings or items to be researched. Blueprints containing locked or unavailable content can remain in the library, be organized into books, and be shared using **Copy Share Code**. These actions do not place anything, unlock research, or spend currency.

**Copy Share Code** uses the compact `KIB2:` format described above. Paste it into another library or the companion Discord viewer in `_Mods/BlueprintDiscordBot`. Legacy gzip/Base64 codes, plain Base64 of UTF-8 JSON, URL-safe Base64, unpadded codes, and Discord code blocks are also accepted on import. Codes retain the blueprint name, icon, settings, and upgrades; suggestions are rebuilt from the layout. Decoding is limited to 8 MiB and verifies gzip integrity. The JSON files on disk remain portable and readable.

**Place in Workshop** checks the entire blueprint against the active kingdom before creating a native placement preview. Missing building, recipe, configured-item, stacking, upgrade and decoration unlocks block placement. Selecting a locked blueprint or clicking its research status opens a scrollable report containing required research names, descriptions, prerequisites and affected content. Unknown game content is blocked from placement while remaining available for storage/sharing. Cosmetic blueprint icons do not require item research.

Research and the active kingdom/floor are checked again when committing placement, before any native placement or spending starts. Whole-layout cost and existing-building collisions are checked as well. Placement uses the game's normal costs, snapping, connections and cancel behavior. Hold Shift to repeat placement, as with the native clone tool. Harmony patches are scoped to previews tagged by this mod. If the placement guards cannot initialize, placement stays disabled while the library remains available.

Choose **New Blueprint**, then drag a rectangle around buildings. Release to store a `.kiblueprint.json` file in `BepInEx/config/SharableBlueprints/Blueprints`. The library reopens with the new blueprint selected, ready to name. Captures go into the selected book, or Unfiled when All Blueprints is selected. Escape, right-click, changing floors or opening another tool cancels selection.

Names are suggested from label decorations fully inside the captured area. One label becomes the initial name automatically. With multiple labels, the first label in top-to-bottom, left-to-right order becomes the initial name, and **Labels** opens a dropdown of the distinct names. The name field always accepts your own text. Without a label, the initial name is `Blueprint N`. Blank labels are ignored; whitespace is normalized and names are limited to 80 characters.

**Choose Icon** opens a searchable icon grid. **From Selection** lists the captured sticker and building icons, with duplicates removed. **All Game Icons** uses the game's own icon catalog. The first selected sticker/building icon is used initially; **Use Schematic** restores the mod's default icon. The selected icon also appears beside the blueprint in the library list. Use **Save Name & Icon** to keep manual edits. Choosing a label or browsing icons does not discard a manually typed name. Earlier captures recover suggestions from their stored labels, stickers and building types when opened.

While holding the drag, move the pointer to any screen edge to pan the workshop and extend the box beyond the original view. Panning ramps up toward the edge, keeps the world-space starting point fixed, respects the game's camera bounds, and stops when you release, cancel or move away from the edge. Losing application focus cancels the selection.

A themed panel on the left updates the selected building counts by type as the box grows or shrinks. Fully enclosed buildings count; partially enclosed buildings do not. Utilities and objective buildings are included. Stacked machines count individually, with the placed-node total shown separately. The same inclusion rules are used for the final capture. The panel lets mouse clicks through so it does not obstruct left-edge panning; use the wheel over the panel to scroll long lists. The wheel continues to zoom the workshop elsewhere.

Blueprints and books persist across launches and are shared across your kingdoms. Book membership, names, icons and suggestions are saved in `Library.xml` alongside the portable blueprint files, with the previous index retained as `Library.xml.bak`. Saving a name/icon also updates the portable file's `name` and `icon` fields and preserves its previous contents as `.bak`. JSON exports include `nameOptions` and `iconOptions`; compact codes rebuild these suggestions on import. A failed index update restores the previous metadata and payload. Older library indexes remain supported. Missing files are marked in the list. An unreadable index produces an error without overwriting existing files.

Only fully enclosed buildings, labels and stickers are included. Container frames allow a 32-unit border tolerance as described above. Internal pipes retain their connections and bends; connections to buildings outside the selection are omitted. Containers within the selected rectangle are included as decorations, preserving their size, style and parent flag. Building settings, mirroring, stacks and upgrades use the game's native copy behavior. Inventory, production progress and rewards are not copied. Exporting does not place buildings, spend currency or save the game.

This build provides the toolbar, library, books, deletion, area capture, edge panning, live selection counts, label naming, icon selection, compact/legacy/JSON clipboard import/export and workshop placement with research checks. Sharing a whole book as one file is not implemented yet. Native popup appearance, camera motion and actual placement still need an in-game check after launch; compilation and 183 standalone selection, share-code, import-validation, research-policy, deletion, suggestions and persistence checks pass. These automated checks do not exercise Unity placement or the live research catalog.

Literal item and upgrade unlock mappings are derived from the installed game's extracted tech definitions and embedded as `Assets/research-unlocks.tsv`. After a game update, regenerate them using `Tools/GenerateUnlockMap.py` and rebuild. Building/recipe availability and current unlock state are read from the game at runtime.

## Build and install

Requires a local Kingdom Inc. Mono installation with BepInEx 5.x. Building the plugin requires a .NET SDK with .NET Standard 2.1 support; running the standalone tests requires the .NET 10 SDK.

Clone this repository into `_Mods/SharableBlueprints` inside the game installation. Run these commands from the game directory for a fresh checkout:

```powershell
git clone https://github.com/urakkaamyx/KingdomInc_SharableBlueprints.git _Mods/SharableBlueprints
```

The project resolves game and BepInEx assemblies from that directory layout. These assemblies are supplied by your local installation and are not included in this repository. The unlock mapping is included; regenerating it after a game update additionally needs the local extraction described above.

From the game directory:

```powershell
dotnet build _Mods/SharableBlueprints/SharableBlueprints.csproj -c Release
dotnet run --project _Mods/SharableBlueprints/Tests/GeometryTests.csproj -c Release
& _Mods/SharableBlueprints/Install.ps1
```

Close the game before installation, then launch normally. Only `BepInEx/plugins/SharableBlueprints/SharableBlueprints.dll` is installed. Existing versions of that DLL are backed up under this project's `_Backup`. Other mods are unaffected.

`Install.ps1 -StageForRestart` can atomically replace the on-disk DLL while an existing game process keeps its already loaded version. The update takes effect on the next launch. If Windows locks the file, close the game and rerun the installer.

The original schematic icon in `Assets/medieval-schematic.png` is embedded in the DLL. Native game UI sprites are read from the running game.

## KIB2 share-code format

Text is `KIB2:` plus one padding digit (`0`–`3`) and Z85 text. Z85 uses `0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.-:+=^!/*?&<>()[]{}@%$#`; four big-endian bytes map to five digits. The decoded compressed bytes are zero-padded to a multiple of four; the initial digit gives the padding count. Remove padding, then decompress gzip with checksum and an 8 MiB expansion limit. Discord code fences protect punctuation during sharing.

The uncompressed binary stream contains the following fields in order. Unsigned integers use 32-bit ULEB128; signed integers use ZigZag then ULEB128. Strings are UTF-8 prefixed by unsigned byte length. JSON strings are compact and have sorted object keys.

1. Four ASCII bytes `KIB2`.
2. Root metadata JSON, excluding layout, derived name/icon suggestions, creation timestamp and producer version.
3. Unknown layout fields as JSON, excluding Elements/Pipes/Decorations.
4. Template count, followed by that many JSON strings. Templates are element objects excluding Guid/Position.
5. Positive node-coordinate unit, initial signed X and Y, and node count. The unit is the GCD of coordinate differences from the first node (minimum 1).
6. Per node: template index, signed X delta/unit, signed Y delta/unit. Deltas are from the previous node, with the first node using zero deltas. Reconstruct unique local IDs `n0`, `n1`, etc.
7. Positive bend-coordinate unit, followed by pipe count. The unit is the GCD of bend deltas (minimum 1).
8. Per pipe: source node index, output port index, target node index, input port index, bend count. Each bend stores signed X/Y deltas divided by the bend unit, starting from the source node position. Finish with JSON for unknown pipe properties. Rebuild endpoint references using the local IDs.
9. Decoration-array JSON. No trailing bytes are accepted.

All counts and reconstructed byte sizes are bounded. The reconstructed format-v1 JSON passes the normal structural validator before import. IDs are local to a blueprint; placement creates new game IDs. No position is snapped or rearranged by this format.

Multipart messages have `KIBPART/2:<key>:<part>:<total>:<slice>`. The key is the first 12 lowercase hexadecimal characters of SHA-256 over the complete KIB2 code. Part numbers start at 1. Require every part exactly once, with a matching total/key, concatenate in order, and verify the digest before decoding. The mod limits message contents to 1,990 characters before adding the eight-character Discord code-block wrapper.
