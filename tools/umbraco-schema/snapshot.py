#!/usr/bin/env python3
"""Read an Umbraco SQLite database (read-only) and write the schema as stable JSON.

Usage: python snapshot.py Umbraco.sqlite.db snapshot.json
Covers: languages, data types, document/element/member types (tabs, properties,
compositions, allowed children, templates), templates, dictionary items and the
content tree outline. Keyed by alias/name so two snapshots diff cleanly.
"""
import json, sqlite3, sys

DOC = "A2CB7800-F571-4787-9638-BC48539A0EFB"     # document type
MEMBER = "9B5416FB-E72F-45A9-A07B-5A9A2709CE43"  # member type
MEDIA = "4EA4382B-2F5A-4C2B-9587-AE9B3CF3602E"   # media type
DATATYPE = "30A2A501-1978-4DDB-A57B-F7EFED43BA3C"
TEMPLATE = "6FBDE604-4178-42CE-A10B-8A2600A2F07D"
CONTENT = "C66BA18E-EAF3-4CFF-8A22-41B16D66A972"
KIND = {DOC: "document", MEMBER: "member", MEDIA: "media"}

def main(db_path, out_path):
    c = sqlite3.connect(f"file:{db_path}?mode=ro", uri=True)
    c.row_factory = sqlite3.Row
    q = lambda sql, *a: c.execute(sql, a).fetchall()
    node = {r["id"]: r for r in q("select id, uniqueId, text, nodeObjectType, parentId from umbracoNode")}
    up = lambda s: (s or "").upper()

    langs = {r["id"]: r["languageISOCode"] for r in q("select * from umbracoLanguage")}
    snap = {"languages": [{"iso": r["languageISOCode"], "default": bool(r["isDefaultVariantLang"]),
                            "mandatory": bool(r["mandatory"])} for r in q("select * from umbracoLanguage order by id")]}

    dts = {}
    for r in q("select * from umbracoDataType"):
        n = node[r["nodeId"]]
        try: cfg = json.loads(r["config"]) if r["config"] else None
        except ValueError: cfg = r["config"]
        dts[r["nodeId"]] = n["text"]
        snap.setdefault("dataTypes", {})[n["text"]] = {
            "key": n["uniqueId"], "editorAlias": r["propertyEditorAlias"],
            "editorUiAlias": r["propertyEditorUiAlias"], "dbType": r["dbType"], "config": cfg}

    templates = {r["nodeId"]: r["alias"] for r in q("select nodeId, alias from cmsTemplate")}
    snap["templates"] = sorted(templates.values())

    ct_alias = {r["nodeId"]: r["alias"] for r in q("select nodeId, alias from cmsContentType")}
    types = {}
    for r in q("select * from cmsContentType"):
        n = node[r["nodeId"]]
        kind = KIND.get(up(n["nodeObjectType"]), "other")
        t = {"kind": kind, "name": n["text"], "key": n["uniqueId"], "icon": r["icon"],
             "description": r["description"], "isElement": bool(r["isElement"]),
             "allowAtRoot": bool(r["allowAtRoot"]), "variations": r["variations"],
             "folder": node[n["parentId"]]["text"] if n["parentId"] in node and n["parentId"] != -1 else None,
             "compositions": sorted(ct_alias[x["parentContentTypeId"]] for x in
                                    q("select * from cmsContentType2ContentType where childContentTypeId=?", r["nodeId"])),
             "allowedChildren": [ct_alias.get(x["AllowedId"]) for x in
                                 q("select * from cmsContentTypeAllowedContentType where Id=? order by SortOrder", r["nodeId"])],
             "templates": sorted(templates.get(x["templateNodeId"]) for x in
                                 q("select * from cmsDocumentType where contentTypeNodeId=?", r["nodeId"])),
             "defaultTemplate": next((templates.get(x["templateNodeId"]) for x in
                                      q("select * from cmsDocumentType where contentTypeNodeId=? and IsDefault=1", r["nodeId"])), None),
             "groups": {}, "properties": {}}
        groups = {}
        for g in q("select * from cmsPropertyTypeGroup where contenttypeNodeId=? order by sortorder", r["nodeId"]):
            groups[g["id"]] = g["alias"]
            t["groups"][g["alias"]] = {"name": g["text"], "type": "tab" if g["type"] == 1 else "group",
                                       "sortOrder": g["sortorder"]}
        for p in q("select * from cmsPropertyType where contentTypeId=? order by sortOrder", r["nodeId"]):
            t["properties"][p["Alias"]] = {
                "name": p["Name"], "dataType": dts.get(p["dataTypeId"]), "group": groups.get(p["propertyTypeGroupId"]),
                "sortOrder": p["sortOrder"], "mandatory": bool(p["mandatory"]), "variations": p["variations"],
                "description": p["Description"], "validationRegExp": p["validationRegExp"],
                "labelOnTop": bool(p["labelOnTop"])}
        types[r["alias"]] = t
    snap["contentTypes"] = types

    dic = {}
    keys = {r["id"]: r["key"] for r in q("select id, key from cmsDictionary")}
    for r in q("select * from cmsDictionary"):
        dic[r["key"]] = {"parent": keys.get(r["parent"]), "values": {
            langs.get(t["languageId"], t["languageId"]): t["value"]
            for t in q("select * from cmsLanguageText where UniqueId=?", r["id"])}}
    snap["dictionary"] = dict(sorted(dic.items()))

    def path_of(i):
        parts = []
        while i in node and i != -1:
            parts.append(node[i]["text"]); i = node[i]["parentId"]
        return "/".join(reversed(parts))
    content = {}
    for r in q("""select n.id, n.trashed, ct.alias from umbracoNode n join umbracoContent c on c.nodeId=n.id
                  join cmsContentType ct on ct.nodeId=c.contentTypeId where upper(n.nodeObjectType)=?""", CONTENT):
        if r["trashed"]: continue
        content[path_of(r["id"])] = {"type": r["alias"], "key": node[r["id"]]["uniqueId"]}
    snap["content"] = dict(sorted(content.items()))

    with open(out_path, "w", encoding="utf-8") as f:
        json.dump(snap, f, ensure_ascii=False, indent=2, sort_keys=True)
    print(f"{len(types)} content types, {len(snap.get('dataTypes', {}))} data types, "
          f"{len(dic)} dictionary items, {len(content)} pages -> {out_path}")

if __name__ == "__main__":
    if len(sys.argv) != 3:
        sys.exit(__doc__)
    main(*sys.argv[1:3])
