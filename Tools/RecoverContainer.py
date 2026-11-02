"""Recover near-edge container frames into a new library entry using a read-only live snapshot."""
from pathlib import Path
from datetime import datetime, timezone
import argparse
import copy
import json
import os
import shutil
import urllib.request
import uuid
import xml.etree.ElementTree as ET

args = argparse.ArgumentParser()
args.add_argument("blueprint", type=Path)
options = args.parse_args()
source_path = options.blueprint.resolve(strict=True)
project = Path(__file__).resolve().parents[1]
library = project.parents[1] / "BepInEx" / "config" / "SharableBlueprints" / "Blueprints"
if source_path.parent != library.resolve():
    raise SystemExit("Choose a blueprint from this game's library.")
original_bytes = source_path.read_bytes()
original = json.loads(original_bytes)
with urllib.request.urlopen("http://127.0.0.1:17840/api/v1/snapshot", timeout=5) as response:
    native = json.load(response)["sections"]["native"]["data"]
flow = native["Flows"][native["CurrentFlow"]]
layout = original["layout"]
world_buildings = {(e["ElementType"], *e["Position"], e.get("StackSize", 1)) for e in flow["Elements"]}
labels = [d for d in layout["Decorations"] if d.get("Label")]
candidates = {}
for saved_label in labels:
    for live_label in flow["Decorations"]:
        if live_label.get("Label") != saved_label["Label"]:
            continue
        ox, oy = (live_label["Position"][axis] - saved_label["Position"][axis] for axis in (0, 1))
        if not all((e["ElementType"], e["Position"][0]+ox, e["Position"][1]+oy, e.get("StackSize", 1)) in world_buildings for e in layout["Elements"]):
            continue
        left, top = ox, oy
        right, bottom = ox + original["size"][0], oy - original["size"][1]
        recovered = []
        for live in flow["Decorations"]:
            if live.get("Label") is not None or live.get("Sticker") is not None:
                continue
            x, y = live["Position"]
            w, h = live["Size"]
            if not (x >= left-32 and y <= top+32 and x+w <= right+32 and y-h >= bottom-32):
                continue
            relative = [x-ox, y-oy]
            if any(d.get("Label") is None and d.get("Sticker") is None and d["Position"] == relative and d["Size"] == live["Size"] for d in layout["Decorations"]):
                continue
            frame = {key: copy.deepcopy(live[key]) for key in ("Position", "Size", "StyleIndex", "IsParent", "Locked") if key in live}
            frame["Position"] = relative
            recovered.append(frame)
        if recovered:
            key = json.dumps(sorted(recovered, key=lambda d: d["Position"]), sort_keys=True)
            candidates[key] = recovered
if len(candidates) != 1:
    raise SystemExit("No unique matching container layout found. No library files changed.")
recovered = next(iter(candidates.values()))
document = copy.deepcopy(original)
document["layout"]["Decorations"].extend(recovered)
left, top, right, bottom = 0, 0, original["size"][0], -original["size"][1]
for frame in recovered:
    x, y = frame["Position"]
    w, h = frame["Size"]
    left, top, right, bottom = min(left, x), max(top, y), max(right, x+w), min(bottom, y-h)
for element in document["layout"]["Elements"] + document["layout"]["Decorations"]:
    element["Position"] = [element["Position"][0]-left, element["Position"][1]-top]
for pipe in document["layout"]["Pipes"]:
    pipe["Nodes"] = [[p[0]-left, p[1]-top] for p in pipe["Nodes"]]
document["size"] = [right-left, top-bottom]
document["name"] = (original.get("name", "Blueprint")[:60] + " (container fixed)")[:80]
document["modVersion"] = "0.4.3"
document["createdAtUtc"] = datetime.now(timezone.utc).isoformat()
index_path = library / "Library.xml"
index_bytes = index_path.read_bytes()
index = ET.fromstring(index_bytes)
entries = index.find("Blueprints")
old_entry = next(e for e in entries if e.get("file") == source_path.name)
entry = copy.deepcopy(old_entry)
entry_id = uuid.uuid4().hex
target = library / ("Blueprint-" + entry_id + ".kiblueprint.json")
entry.set("id", entry_id)
entry.set("name", document["name"])
entry.set("file", target.name)
entry.set("decorations", str(len(document["layout"]["Decorations"])))
entry.set("createdUtc", document["createdAtUtc"])
entries.append(entry)
if source_path.read_bytes() != original_bytes or index_path.read_bytes() != index_bytes:
    raise SystemExit("The library changed during recovery. No files were written; try again.")
backup = project / "_Backup" / ("container-recovery-" + datetime.now().strftime("%Y%m%d-%H%M%S-%f"))
backup.mkdir(parents=True)
shutil.copy2(source_path, backup / source_path.name)
shutil.copy2(index_path, backup / "Library.xml")
with target.open("x", encoding="utf-8") as output:
    json.dump(document, output, indent=2, ensure_ascii=False)
staged_index = library / ("Library.xml.recovery-" + entry_id + ".tmp")
try:
    ET.ElementTree(index).write(staged_index, encoding="utf-8", xml_declaration=True)
    if index_path.read_bytes() != index_bytes:
        raise RuntimeError("The library index changed. The recovered blueprint file remains available for adoption on reopen.")
    os.replace(staged_index, index_path)
finally:
    if staged_index.exists():
        staged_index.unlink()
print(json.dumps({"name": document["name"], "file": str(target), "recoveredContainers": len(recovered), "decorations": len(document["layout"]["Decorations"]), "originalPreserved": source_path.read_bytes() == original_bytes}))
