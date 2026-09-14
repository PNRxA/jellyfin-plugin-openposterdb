#!/usr/bin/env python3
"""Add (or refresh) a released plugin version in manifest.json.

Used by .github/workflows/release.yml after the plugin zip has been attached to
a GitHub Release. Reads the jprm ``<zip>.meta.json`` written next to the zip,
computes the zip's MD5, and upserts a version entry (newest first) so the
Jellyfin plugin catalog can serve the update.

Usage:
    scripts/publish-manifest.py --zip artifacts/openposterdb_1.2.0.0.zip \
        --url https://github.com/OWNER/REPO/releases/download/v1.2.0.0/openposterdb_1.2.0.0.zip
"""

import argparse
import hashlib
import json
import sys
from pathlib import Path


def md5_of(path: Path) -> str:
    digest = hashlib.md5()
    with path.open("rb") as fh:
        for chunk in iter(lambda: fh.read(1 << 20), b""):
            digest.update(chunk)
    return digest.hexdigest()


def latest_changelog(changelog: str, version: str) -> str:
    """Return only this version's section of the build.yaml changelog.

    build.yaml carries the full history as ``### <version>`` sections; the
    manifest keeps one short note per version.
    """
    sections = ("\n" + changelog.strip()).split("\n### ")
    for section in sections[1:]:
        heading, _, body = section.partition("\n")
        if heading.strip() == version:
            return tidy(body)
    # No matching heading: fall back to everything before the second heading.
    return tidy(sections[1].partition("\n")[2]) if len(sections) > 1 else tidy(changelog)


def tidy(body: str) -> str:
    """A single bullet becomes a plain sentence; longer sections keep their markdown list."""
    lines = [line.strip() for line in body.strip().splitlines() if line.strip()]
    if len(lines) == 1 and lines[0].startswith("- "):
        return lines[0][2:].strip()
    return "\n".join(lines)


def version_key(version: str):
    return tuple(int(part) for part in version.split("."))


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--zip", required=True, type=Path, help="plugin zip built by jprm")
    parser.add_argument("--url", required=True, help="public download URL of that zip")
    parser.add_argument("--manifest", default="manifest.json", type=Path)
    args = parser.parse_args()

    meta_path = Path(str(args.zip) + ".meta.json")
    if not args.zip.is_file() or not meta_path.is_file():
        print(f"error: need both {args.zip} and {meta_path}", file=sys.stderr)
        return 1

    meta = json.loads(meta_path.read_text(encoding="utf-8"))
    manifest = json.loads(args.manifest.read_text(encoding="utf-8"))
    plugin = next((p for p in manifest if p.get("guid") == meta["guid"]), None)
    if plugin is None:
        print(f"error: no plugin with guid {meta['guid']} in {args.manifest}", file=sys.stderr)
        return 1

    entry = {
        "version": meta["version"],
        "changelog": latest_changelog(meta.get("changelog", ""), meta["version"]),
        "targetAbi": meta["targetAbi"],
        "sourceUrl": args.url,
        "checksum": md5_of(args.zip),
        "timestamp": meta["timestamp"],
    }

    versions = [v for v in plugin.get("versions", []) if v.get("version") != entry["version"]]
    versions.append(entry)
    versions.sort(key=lambda v: version_key(v["version"]), reverse=True)
    plugin["versions"] = versions

    args.manifest.write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(f"{args.manifest}: {plugin['name']} {entry['version']} -> {entry['sourceUrl']} (md5 {entry['checksum']})")
    return 0


if __name__ == "__main__":
    sys.exit(main())
