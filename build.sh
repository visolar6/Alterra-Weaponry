#!/bin/sh

dotnet build -c Release
if [ $? -ne 0 ]; then
    echo "✗ Build failed"
    exit 1
fi

# Copy DLL and assets to Subnautica BepInEx plugins folder
SUBNAUTICA_PATH="D:/SteamLibrary/steamapps/common/Subnautica"
PLUGIN_DIR="$SUBNAUTICA_PATH/BepInEx/plugins/AlterraWeaponry"
DLL_PATH="AlterraWeaponry/bin/built/AlterraWeaponry/AlterraWeaponry.dll"
ASSETS_PATH="AlterraWeaponry/bin/built/AlterraWeaponry/sn.alterraweaponry.assets"
LOCALIZATION_PATH="AlterraWeaponry/bin/built/AlterraWeaponry/Localizations.xml"

if [ -f "$DLL_PATH" ]; then
    mkdir -p "$PLUGIN_DIR"
    cp "$DLL_PATH" "$PLUGIN_DIR/"
    echo "✓ DLL copied to $PLUGIN_DIR"
    
    if [ -f "$ASSETS_PATH" ]; then
        cp "$ASSETS_PATH" "$PLUGIN_DIR/"
        echo "✓ Assets copied to $PLUGIN_DIR"
    else
        echo "✗ Assets not found at $ASSETS_PATH"
    fi
    
    if [ -f "$LOCALIZATION_PATH" ]; then
        cp "$LOCALIZATION_PATH" "$PLUGIN_DIR/"
        echo "✓ Localizations.xml copied to $PLUGIN_DIR"
    else
        echo "✗ Localizations.xml not found at $LOCALIZATION_PATH"
    fi
else
    echo "✗ DLL not found at $DLL_PATH"
    exit 1
fi