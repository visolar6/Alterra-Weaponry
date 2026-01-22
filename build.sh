#!/bin/sh

dotnet build -c Release
if [ $? -ne 0 ]; then
    echo "✗ Build failed"
    exit 1
fi

# Copy DLL and assets to Subnautica BepInEx plugins folder
# Set SUBNAUTICA_PATH environment variable or edit here for your system
SUBNAUTICA_PATH="${SUBNAUTICA_PATH:-D:/SteamLibrary/steamapps/common/Subnautica}"
PLUGIN_DIR="$SUBNAUTICA_PATH/BepInEx/plugins/AlterraWeaponry"
DLL_PATH="AlterraWeaponry/bin/Release/net472/AlterraWeaponry.dll"
ASSETS_PATH="AlterraWeaponry/sn.alterraweaponry.assets"
BANNER_ASSETS_PATH="AlterraWeaponry/sn.alterraweaponry_banners.assets"
LOCALIZATION_PATH="AlterraWeaponry/Localizations.xml"
MOD_JSON_PATH="AlterraWeaponry/mod.json"

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

    if [ -f "$BANNER_ASSETS_PATH" ]; then
        cp "$BANNER_ASSETS_PATH" "$PLUGIN_DIR/"
        echo "✓ Banner assets copied to $PLUGIN_DIR"
    else
        echo "✗ Banner assets not found at $BANNER_ASSETS_PATH"
    fi
    
    if [ -f "$LOCALIZATION_PATH" ]; then
        cp "$LOCALIZATION_PATH" "$PLUGIN_DIR/"
        echo "✓ Localizations.xml copied to $PLUGIN_DIR"
    else
        echo "✗ Localizations.xml not found at $LOCALIZATION_PATH"
    fi
    
    if [ -f "$MOD_JSON_PATH" ]; then
        cp "$MOD_JSON_PATH" "$PLUGIN_DIR/"
        echo "✓ mod.json copied to $PLUGIN_DIR"
    else
        echo "✗ mod.json not found at $MOD_JSON_PATH"
    fi
else
    echo "✗ DLL not found at $DLL_PATH"
    echo "Make sure you have built the project in Release mode"
    exit 1
fi

echo "✓ Build and deployment complete!"