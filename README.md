# Sharable Blueprints

**Current release: 0.5.3 — for Kingdom Inc game version 1.0.14.**

[GitHub releases](https://github.com/urakkaamyx/KingdomInc_SharableBlueprints/releases) · [Nexus Mods](https://www.nexusmods.com/kingdominc/mods/6) · Version 0.5.3

Capture factory layouts in Kingdom Inc, organize them into books, and share them with other players.

## Features

- **Rectangular Capture:** Capture buildings, internal pipes, labels, stickers, and container frames with an intuitive selection tool.
- **Searchable Blueprint Books:** Organize layout captures into custom books with personal names and icons.
- **Compact Share Codes:** Export and import layouts using compact KIB2 codes, numbered Discord message parts, or portable JSON.
- **Research Validation:** Automatically checks your kingdom's tech tree unlocks before allowing placement in the workshop.
- **Independent Storage:** Blueprint libraries are stored in `BepInEx/config/SharableBlueprints/Blueprints/`, persisting across game launches and different kingdom saves.
- **Native Alignment:** Preserves placement costs, collisions, snapping, and connection rules.

## Using the Mod

Click the schematic icon in the bottom-left edit toolbar to open the library.

- **Capture:** Choose New Blueprint and drag a rectangle around a layout. Move the pointer to a screen edge while dragging to pan.
- **Organize:** Use New Book and Manage Book to organize captures. Deleting a book moves its blueprints to Unfiled.
- **Share:** Click Copy Share Code to copy the active layout. Click Paste Blueprint to import a code or JSON string. Paste all numbered parts together for a multipart code.
- **Place:** Click Place in Workshop. The mod validates tech tree research and applies native construction costs and snapping.

## Installation

1. Install [BepInEx 5 (Windows x64 Mono)](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.6).
2. Install [Foundation](https://www.nexusmods.com/kingdominc/mods/18) (0.14.0 or newer).
3. Extract the release ZIP into your Kingdom Inc game folder.
4. Confirm the file is located at `BepInEx/plugins/SharableBlueprints/SharableBlueprints.dll`.
5. Launch the game and access the blueprint library from the workshop edit toolbar!

## Compatibility

| Class.Method | Hook Type | Impact |
|---|---|---|
| WorldFlowGhost.GetCopyCost | Foundation hook (prefix) | Sums construction currency costs across all buildings and pipes in the imported blueprint. |
| WorldFlowGhost.OnDragDropEnd | Foundation hook (prefix) | Validates player tech tree unlocks against blueprint machine requirements before allowing blueprint placement. |

## Removal

Close the game and delete `BepInEx/plugins/SharableBlueprints/`. Your saved blueprint library in `BepInEx/config/SharableBlueprints/Blueprints/` will remain safe.

## Credits and Source

Created by **Urakka** (rockyamyx). [Source code on GitHub](https://github.com/urakkaamyx/KingdomInc_SharableBlueprints). The schematic icon and thumbnail are original generated artwork.