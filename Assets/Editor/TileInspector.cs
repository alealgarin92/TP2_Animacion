using UnityEngine;
using UnityEditor;
using System.IO;

public static class TileInspector
{
    [MenuItem("Tools/Inspect Tile Sizes")]
    public static void Run()
    {
        string[] paths = {
            "Assets/Art/freecute_tileset/Foreground/Tileset.png",
            "Assets/Art/freecute_tileset/Foreground/Trees.png",
            "Assets/Art/freecute_tileset/Background/BGBack.png",
            "Assets/Art/freecute_tileset/Background/BGFront.png",
            "Assets/Art/freecute_tileset/Background/CloudsBack.png",
            "Assets/Art/freecute_tileset/Background/Layer_0000_9.png",
            "Assets/Art/freecute_tileset/Background/Layer_0003_6.png",
        };

        foreach (string p in paths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(p) as TextureImporter;
            if (importer == null) { Debug.LogError("No importer: " + p); continue; }

            int w, h;
            importer.GetSourceTextureWidthAndHeight(out w, out h);
            Debug.Log("[TEXTURE] " + Path.GetFileName(p) + " => " + w + "x" + h
                + "  spriteMode=" + importer.spriteImportMode
                + "  PPU=" + importer.spritePixelsPerUnit);
        }
    }
}
