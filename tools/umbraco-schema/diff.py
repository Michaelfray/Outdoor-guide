#!/usr/bin/env python3
"""Compare two snapshots from snapshot.py and print what changed, grouped for writing a migration.

Usage: python diff.py old.json new.json
"""
import json, sys

def walk(a, b, path, out):
    if isinstance(a, dict) and isinstance(b, dict):
        for k in sorted(set(a) | set(b)):
            if k == "key":  # GUIDs differ between machines; never a reason for a migration
                continue
            if k not in a: out.append(("+", path + [k], b[k]))
            elif k not in b: out.append(("-", path + [k], a[k]))
            else: walk(a[k], b[k], path + [k], out)
    elif a != b:
        out.append(("~", path, (a, b)))

def main(old, new):
    a, b = (json.load(open(p, encoding="utf-8")) for p in (old, new))
    out = []
    walk(a, b, [], out)
    if not out:
        print("Inga skillnader."); return
    sections = {"contentTypes": "Dokument-/element-/medlemstyper", "dataTypes": "Data types",
                "dictionary": "Dictionary items", "templates": "Mallar", "languages": "Språk",
                "content": "Sidor i innehållsträdet"}
    for sec, title in sections.items():
        rows = [o for o in out if o[1] and o[1][0] == sec]
        if not rows: continue
        print(f"\n## {title}")
        for op, path, val in rows:
            where = " > ".join(map(str, path[1:])) or "(hela listan)"
            if op == "~":
                print(f"  ~ {where}: {json.dumps(val[0], ensure_ascii=False)} -> {json.dumps(val[1], ensure_ascii=False)}")
            else:
                text = json.dumps(val, ensure_ascii=False, indent=2) if isinstance(val, (dict, list)) else json.dumps(val, ensure_ascii=False)
                print(f"  {op} {where}: " + text.replace("\n", "\n      "))

if __name__ == "__main__":
    if len(sys.argv) != 3:
        sys.exit(__doc__)
    sys.stdout.reconfigure(encoding="utf-8")
    main(*sys.argv[1:3])
