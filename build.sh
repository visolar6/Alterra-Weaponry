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
ASSETBUNDLE_PATH="AlterraWeaponry/sn.alterraweaponry.assets"
BANNER_ASSETBUNDLE_PATH="AlterraWeaponry/sn.alterraweaponry_banners.assets"
LOCALIZATION_PATH="AlterraWeaponry/Localizations.xml"
ASSETS_PATH="AlterraWeaponry/Assets"

if [ -f "$DLL_PATH" ]; then
    mkdir -p "$PLUGIN_DIR"
    cp "$DLL_PATH" "$PLUGIN_DIR/"
    echo "✓ DLL copied to $PLUGIN_DIR"

    if [ -f "$LOCALIZATION_PATH" ]; then
        cp "$LOCALIZATION_PATH" "$PLUGIN_DIR/"
        echo "✓ Localizations.xml copied to $PLUGIN_DIR"
    else
        echo "✗ Localizations.xml not found at $LOCALIZATION_PATH"
    fi
    
    if [ -d "$ASSETS_PATH" ]; then
        cp -r "$ASSETS_PATH" "$PLUGIN_DIR/"
        echo "✓ Assets copied to $PLUGIN_DIR"
    else
        echo "✗ Assets not found at $ASSETS_PATH"
    fi

    if [ -f "$ASSETBUNDLE_PATH" ]; then
        cp "$ASSETBUNDLE_PATH" "$PLUGIN_DIR/"
        echo "✓ AssetBundle copied to $PLUGIN_DIR"
    else
        echo "✗ AssetBundle not found at $ASSETBUNDLE_PATH"
    fi

    if [ -f "$BANNER_ASSETBUNDLE_PATH" ]; then
        cp "$BANNER_ASSETBUNDLE_PATH" "$PLUGIN_DIR/"
        echo "✓ Banner assets copied to $PLUGIN_DIR"
    else
        echo "✗ Banner assets not found at $BANNER_ASSETBUNDLE_PATH"
    fi
else
    echo "✗ DLL not found at $DLL_PATH"
    echo "Make sure you have built the project in Release mode"
    exit 1
fi

echo "✓ Build and deployment complete!"