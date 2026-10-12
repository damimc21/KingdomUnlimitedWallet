"""Build the reviewed ZIP from supplied wallet DLLs and the exact text payload."""
from pathlib import Path, PurePosixPath
import argparse
import hashlib
import json
import zipfile

ROOT = Path(__file__).resolve().parent
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--il2cpp-dll", type=Path, required=True)
parser.add_argument("--mono-dll", type=Path, required=True)
parser.add_argument("--output", type=Path, required=True)
args = parser.parse_args()
manifest = json.loads((ROOT / "Packaging/release-manifest.json").read_text(encoding="utf-8"))
payload = {}
matches = {}
for entry in manifest["entries"]:
    name = entry["name"]
    parts = PurePosixPath(name)
    if parts.is_absolute() or ".." in parts.parts:
        raise ValueError(f"Invalid package member: {name}")
    if name.endswith(".dll"):
        if parts.name != "KingdomUnlimitedWallet.dll":
            raise ValueError(f"Unexpected assembly: {name}")
        source = args.il2cpp_dll if parts.parts[1] == "IL2CPP" else args.mono_dll
    else:
        source = ROOT / "Packaging/files" / name
    data = source.read_bytes()
    digest = hashlib.sha256(data).hexdigest()
    matches[name] = digest == entry["sha256"]
    if not name.endswith(".dll") and not matches[name]:
        raise ValueError(f"Reviewed text payload has changed: {name}")
    payload[name] = (data, int(entry["unix_mode"], 8))
args.output.parent.mkdir(parents=True, exist_ok=True)
with zipfile.ZipFile(args.output, "w", zipfile.ZIP_DEFLATED, compresslevel=6) as archive:
    for name, (data, mode) in sorted(payload.items()):
        item = zipfile.ZipInfo(name, (2026, 10, 11, 0, 0, 0))
        item.compress_type = zipfile.ZIP_DEFLATED
        item.external_attr = mode << 16
        archive.writestr(item, data)
digest = hashlib.sha256(args.output.read_bytes()).hexdigest()
print(json.dumps({"archive": str(args.output), "sha256": digest,
                  "matches_release_zip": digest == manifest["sha256"],
                  "payload_matches_release": matches}, indent=2))
