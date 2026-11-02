# Sharable Blueprints

Capture factory layouts in Kingdom Inc., organize them into books, and share them with other players. Sharable Blueprints adds a schematic button to the workshop toolbar and uses the game's native placement and research rules.

## Features

- Capture buildings, internal pipes, labels, stickers, and container frames with a rectangular selection.
- Organize blueprints into searchable books, with custom names and icons.
- Share layouts through compact clipboard codes or portable `.kiblueprint.json` files.
- Split large share codes into numbered Discord messages and import them together.
- Inspect required research before placing a blueprint in your current kingdom.
- Keep one blueprint library across game launches and kingdoms.

## Requirements

- Kingdom Inc. using the Mono runtime.
- BepInEx 5.x installed in the game directory.

## Installation

With the game closed, copy a compiled `SharableBlueprints.dll` into:

```text
<game>/BepInEx/plugins/SharableBlueprints/SharableBlueprints.dll
```

Launch the game normally. The schematic button appears in the bottom-left edit toolbar. If you are installing from this source repository, follow [Building from source](#building-from-source) below to produce and install the DLL.

To update, close the game and replace the DLL. Your blueprint library is stored separately from the plugin.

## Using the mod

### Capture a layout

1. Click the schematic button to open the library.
2. Select a book, or use **All Blueprints** to capture into **Unfiled**.
3. Choose **New Blueprint** and drag a rectangle around your layout.
4. Release to save the capture and return to the library.

Move the pointer to a screen edge while dragging to pan. The selection panel shows included buildings and decorations. Escape or right-click cancels the selection.

Buildings, labels, and stickers must be fully enclosed. Container frames have a 32-world-unit border tolerance. Internal pipe connections and bends are preserved; pipes leading outside the selection are omitted. Building settings, mirroring, stacks, and upgrades are retained. Inventory, production progress, and rewards are not copied.

### Organize your library

Use **New Book** and **Manage Book** to create, rename, or delete books. **Move to Book** moves the selected blueprint. Deleting a book keeps its blueprints in **Unfiled**.

Edit a blueprint's name, choose an icon, then use **Save Name & Icon**. Captured labels provide name suggestions. **Choose Icon** offers captured building/sticker icons and the game's icon catalog; **Use Schematic** restores the default icon.

Deleting a blueprint requires confirmation and moves its files into the library's `_Deleted` folder.

### Share and import

Select a blueprint and choose **Copy Share Code**. Paste the code into a message or another player's **Paste Blueprint** dialog.

If the code needs multiple Discord messages, the mod opens **Share Blueprint in Discord** with a **Copy Part** button for each part. Send every part; recipients paste them together into **Paste Blueprint**, then choose **Store Blueprint**. Parts can arrive in any order, but all parts are required.

Import accepts compact `KIB2:` codes, multipart codes, legacy Base64 codes, and raw `.kiblueprint.json` text. Compact codes require Sharable Blueprints 0.5.0 or later. Codes preserve the blueprint name, icon, layout, and settings. The companion Discord viewer can also preview shared layouts.

### Place a blueprint

Select a blueprint and choose **Place in Workshop**. The mod checks building, recipe, item, stacking, upgrade, and decoration unlocks against the current kingdom. Click its research status to inspect missing requirements.

Locked blueprints can still be stored, organized, and shared. Placement becomes available when the requirements are met. It uses the game's normal costs, snapping, collision checks, connections, and cancellation behavior. Hold Shift to place another copy.

## Library files

Blueprints are stored in:

```text
<game>/BepInEx/config/SharableBlueprints/Blueprints/
```

Each layout has a portable `.kiblueprint.json` file. `Library.xml` stores books and library metadata. Back up this directory to preserve your library or move it to another installation.

Blueprints are shared across your kingdoms. Sharing an entire book as one file is not currently supported.

## Building from source

Building the plugin requires a .NET SDK with .NET Standard 2.1 support. The standalone tests require the .NET 10 SDK.

Clone into `_Mods/SharableBlueprints` inside your game installation. From the game directory:

```powershell
git clone https://github.com/urakkaamyx/KingdomInc_SharableBlueprints.git _Mods/SharableBlueprints
dotnet build _Mods/SharableBlueprints/SharableBlueprints.csproj -c Release
```

The project resolves game and BepInEx assemblies from this directory layout. Those assemblies come from your local installation and are not included in this repository.

With the game closed, install the build:

```powershell
& _Mods/SharableBlueprints/Install.ps1
```

The installer copies the DLL into `BepInEx/plugins/SharableBlueprints` and backs up an existing DLL under the project's `_Backup` directory.

Run the standalone checks from the game directory:

```powershell
dotnet run --project _Mods/SharableBlueprints/Tests/GeometryTests.csproj -c Release
```

Unlock mappings are included in `Assets/research-unlocks.tsv`. To regenerate them after a game update, `Tools/GenerateUnlockMap.py` requires extracted technology definitions in `<game>/Extracted/Source/Assets.Source.Player.Tech`. Building and recipe availability are read from the running game.

<details>
<summary>KIB2 share-code format for tool developers</summary>

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

</details>
