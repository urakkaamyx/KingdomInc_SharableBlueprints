"""Derive literal item/upgrade unlocks from this installed game's extracted tech definitions."""
from pathlib import Path
import re

project = Path(__file__).resolve().parents[1]
source = project.parents[1] / "Extracted" / "Source" / "Assets.Source.Player.Tech"
rows = set()
for path in sorted(source.glob("*.cs")):
    text = path.read_text(encoding="utf-8-sig")
    starts = list(re.finditer(r'new TechNode\("([^"]+)"\)', text))
    for index, match in enumerate(starts):
        block = text[match.end():starts[index+1].start() if index+1 < len(starts) else len(text)]
        tech = match.group(1)
        for element, upgrade in re.findall(r'AddAvailableFlowUpgrade\("([^"]+)",\s*"([^"]+)"\)', block):
            rows.add(("upgrade", element + "/" + upgrade, tech))
        for item in re.findall(r'AddResource\("([^"]+)"', block) + re.findall(r'UnlockItem\(ItemType.Get\("([^"]+)"\)', block):
            rows.add(("item", item, tech))
target = project / "Assets" / "research-unlocks.tsv"
target.write_text("# Derived from installed Kingdom .inc tech definitions; regenerate after game updates.\n" + "".join("\t".join(row)+"\n" for row in sorted(rows)), encoding="utf-8")
print(f"Wrote {len(rows)} unlock mappings to {target}")
