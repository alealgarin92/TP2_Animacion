using UnityEngine;
using UnityEditor;

/// <summary>
/// Slices Tileset.png into individual sprites so we can pick
/// the right tile for platforms.
/// 
/// From the TilesExamples.png image, we can see the tileset layout:
/// The Tileset.png appears to be roughly 96x48px (6 cols x 3 rows of 16px tiles).
/// After reimporting as Multiple with 16x16 grid, we get named sprites.
/// </summary>
public static class TilesetSlicer
{
    [MenuItem("Tools/Slice Tileset PNG")]
    public static void Slice()
    {
        string tilesetPath = "Assets/Art/freecute_tileset/Foreground/Tileset.png";
        TextureImporter imp = AssetImporter.GetAtPath(tilesetPath) as TextureImporter;
        if (imp == null) { Debug.LogError("[Slicer] Tileset not found!"); return; }

        // First: get actual dimensions
        int w, h;
        imp.GetSourceTextureWidthAndHeight(out w, out h);
        Debug.Log($"[Slicer] Tileset size: {w}x{h}px");

        // Set up for multiple sprite slicing at 16x16 grid
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.spritePixelsPerUnit = 16f;
        imp.filterMode = FilterMode.Point;
        imp.mipmapEnabled = false;

        // Auto-slice by grid
        var textureSettings = new TextureImporterSettings();
        imp.ReadTextureSettings(textureSettings);
        textureSettings.spriteMeshType = SpriteMeshType.FullRect;
        textureSettings.spriteExtrude = 0;
        imp.SetTextureSettings(textureSettings);

        // Create sprite metadata for 16x16 grid
        int cols = w / 16;
        int rows = h / 16;
        Debug.Log($"[Slicer] Grid: {cols} cols x {rows} rows");

        var sprites = new System.Collections.Generic.List<SpriteMetaData>();
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                // Unity Rect: x from left, y from BOTTOM
                var meta = new SpriteMetaData();
                meta.name = $"tile_{col}_{row}";
                meta.rect = new Rect(
                    col * 16,
                    (rows - 1 - row) * 16,  // flip Y: row 0 = top visually = bottom in Unity coords
                    16, 16
                );
                meta.alignment = 0;
                meta.pivot = new Vector2(0.5f, 0.5f);
                sprites.Add(meta);
            }
        }

        imp.spritesheet = sprites.ToArray();
        imp.SaveAndReimport();

        AssetDatabase.Refresh();
        Debug.Log($"[Slicer] Done! Created {sprites.Count} tile sprites from Tileset.png");
    }
}
