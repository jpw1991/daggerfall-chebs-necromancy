#!/bin/bash
JSON_SRC="ChebsNecromancy.dfmod.json"
DFMOD_SRC_NIX="../../../Untracked/ModBuilder/StandaloneLinux64/cheb's necromancy.dfmod"
DFMOD_SRC_WIN="../../../Untracked/ModBuilder/StandaloneWindows/cheb's necromancy.dfmod"
README_SRC="README.md"
BIOG_SRC="BIOG18T0.TXT"

DEST="Dist"
DFMOD_DEST="$DEST/Mods"
BIOG_DEST="$DEST/BIOGs"

if [ ! -f "$JSON_SRC" ]; then
    echo "$JSON_SRC missing"
    exit 1
fi

if [ ! -f "$DFMOD_SRC_NIX" ]; then
    echo "$DFMOD_SRC_NIX missing"
    exit 1
fi

if [ ! -f "$DFMOD_SRC_WIN" ]; then
    echo "$DFMOD_SRC_WIN missing"
    exit 1
fi

if [ ! -f "$BIOG_SRC" ]; then
    echo "$BIOG_SRC missing"
    exit 1
fi

if [ ! -f "$README_SRC" ]; then
    echo "$README_SRC missing"
    exit 1
fi

if [ -d "$DEST" ]; then
    read -p "$DEST exists, delete existing files? (y/N)" -n 1 -r
    if [[ $REPLY =~ ^[Yy]$ ]]
    then
        rm -r "$DEST"
    else
        echo "Aborting..."
        exit 0
    fi
fi

VERSION=$(jq -r '.ModVersion' "$JSON_SRC")

mkdir -p "$DFMOD_DEST"
mkdir -p "$DFMOD_DEST/Linux"
mkdir -p "$DFMOD_DEST/Windows"
mkdir -p "$BIOG_DEST"

cp "$README_SRC" "$DEST"
cp "$BIOG_SRC" "$BIOG_DEST"
cp "$DFMOD_SRC_NIX" "$DFMOD_DEST/Linux"
cp "$DFMOD_SRC_WIN" "$DFMOD_DEST/Windows"

cd "$DEST"
zip -r "chebs-necromancy-dfu-$VERSION.zip" .
