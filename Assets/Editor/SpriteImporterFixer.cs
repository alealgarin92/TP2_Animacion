using UnityEngine;
using UnityEditor;

/// <summary>
/// Fixes sprite importer settings for freecute_tileset assets so they
/// load correctly as Single sprites (for background layers) or properly
/// sliced (for the tileset).
/// Also reports actual sprite bounds found in scene.
/// </summary>
public static class SpriteImporterFixer
{
    [MenuItem("Tools/Fix Tileset Sprite Importers")]
    public static void FixImporters()
    {
        // Background layers: each is a full-image single sprite
        string[] bgPaths = {
            "Assets/Art/freecute_tileset/Background/BGBack.png",
            "Assets/Art/freecute_tileset/Background/BGFront.png",
            "Assets/Art/freecute_tileset/Background/CloudsBack.png",
            "Assets/Art/freecute_tileset/Background/CloudsFront.png",
            "Assets/Art/freecute_tileset/Background/Layer_0000_9.png",
            "Assets/Art/freecute_tileset/Background/Layer_0001_8.png",
            "Assets/Art/freecute_tileset/Background/Layer_0002_7.png",
            "Assets/Art/freecute_tileset/Background/Layer_0003_6.png",
            "Assets/Art/freecute_tileset/Background/Layer_0004_Lights.png",
            "Assets/Art/freecute_tileset/Background/Layer_0005_5.png",
            "Assets/Art/freecute_tileset/Background/Layer_0006_4.png",
            "Assets/Art/freecute_tileset/Background/Layer_0007_Lights.png",
            "Assets/Art/freecute_tileset/Background/Layer_0008_3.png",
            "Assets/Art/freecute_tileset/Background/Layer_0009_2.png",
            "Assets/Art/freecute_tileset/Background/Layer_0010_1.png",
            "Assets/Art/freecute_tileset/Background/Layer_0011_0.png",
        };

        // Foreground sprites (single image used as-is)
        string[] fgPaths = {
            "Assets/Art/freecute_tileset/Foreground/Tileset.png",
            "Assets/Art/freecute_tileset/Foreground/Trees.png",
            "Assets/Art/freecute_tileset/Foreground/TilesExamples.png",
        };

        bool anyChanged = false;

        foreach (string path in bgPaths)
        {
            if (FixSingleSprite(path, pixelsPerUnit: 16f))
                anyChanged = true;
        }

        foreach (string path in fgPaths)
        {
            if (FixSingleSprite(path, pixelsPerUnit: 16f))
                anyChanged = true;
        }

        if (anyChanged)
        {
            AssetDatabase.Refresh();
            Debug.Log("[SpriteImporterFixer] Done. Reimported changed assets.");
        }
        else
        {
            Debug.Log("[SpriteImporterFixer] No changes needed.");
        }

        // Report bounds
        ReportBounds();
    }

    static bool FixSingleSprite(string path, float pixelsPerUnit)
    {
        TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null)
        {
            Debug.LogWarning("[SpriteImporterFixer] Not found: " + path);
            return false;
        }

        bool changed = false;

        if (imp.textureType != TextureImporterType.Sprite)
        {
            imp.textureType = TextureImporterType.Sprite;
            changed = true;
        }

        if (imp.spriteImportMode != SpriteImportMode.Single)
        {
            imp.spriteImportMode = SpriteImportMode.Single;
            changed = true;
        }

        if (!Mathf.Approximately(imp.spritePixelsPerUnit, pixelsPerUnit))
        {
            imp.spritePixelsPerUnit = pixelsPerUnit;
            changed = true;
        }

        // Disable mip maps for pixel art
        if (imp.mipmapEnabled)
        {
            imp.mipmapEnabled = false;
            changed = true;
        }

        // Point filter for crisp pixel art
        TextureImporterSettings settings = new TextureImporterSettings();
        imp.ReadTextureSettings(settings);
        if (settings.filterMode != FilterMode.Point)
        {
            settings.filterMode = FilterMode.Point;
            imp.SetTextureSettings(settings);
            changed = true;
        }

        // Clear any bad spritesheet data
        if (imp.spritesheet != null && imp.spritesheet.Length > 0)
        {
            imp.spritesheet = new SpriteMetaData[0];
            changed = true;
        }

        if (changed)
        {
            imp.SaveAndReimport();
            Debug.Log("[SpriteImporterFixer] Fixed: " + path);
        }

        return changed;
    }

    static void ReportBounds()
    {
        string[] paths = {
            "Assets/Art/freecute_tileset/Foreground/Tileset.png",
            "Assets/Art/freecute_tileset/Foreground/Trees.png",
            "Assets/Art/freecute_tileset/Background/BGBack.png",
        };

        foreach (string p in paths)
        {
            Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>(p);
            if (spr == null) { Debug.LogError("Still null: " + p); continue; }
            int w, h;
            (AssetImporter.GetAtPath(p) as TextureImporter).GetSourceTextureWidthAndHeight(out w, out h);
            Debug.Log($"[Bounds] {System.IO.Path.GetFileName(p)}: tex={w}x{h}px, bounds={spr.bounds.size}units, ppu={spr.pixelsPerUnit}");
        }
    }
}
