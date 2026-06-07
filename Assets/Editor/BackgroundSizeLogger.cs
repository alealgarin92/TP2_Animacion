using UnityEngine;
using UnityEditor;

/// <summary>
/// Logs actual pixel dimensions of all background images so we can
/// compute correct scale factors to fill the camera view.
/// Camera: orthographicSize=9 => 18 units tall, at 16:9 => 32 units wide.
/// A sprite at PPU=16 occupies (width_px/16) x (height_px/16) units at scale=1.
/// </summary>
public static class BackgroundSizeLogger
{
    [MenuItem("Tools/Log Background Sizes")]
    public static void LogSizes()
    {
        Camera cam = Camera.main;
        float camH = cam != null ? cam.orthographicSize * 2f : 18f;
        float camW = camH * (16f / 9f); // assume 16:9
        Debug.Log($"[BgSizes] Camera visible area: {camW:F2} x {camH:F2} units");

        string[] paths = {
            "Assets/Art/freecute_tileset/Background/CloudsBack.png",
            "Assets/Art/freecute_tileset/Background/CloudsFront.png",
            "Assets/Art/freecute_tileset/Background/BGBack.png",
            "Assets/Art/freecute_tileset/Background/BGFront.png",
            "Assets/Art/freecute_tileset/Background/Layer_0000_9.png",
            "Assets/Art/freecute_tileset/Background/Layer_0003_6.png",
            "Assets/Art/freecute_tileset/Foreground/Tileset.png",
            "Assets/Art/freecute_tileset/Foreground/Trees.png",
        };

        foreach (string path in paths)
        {
            TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) { Debug.LogWarning("[BgSizes] Not found: " + path); continue; }

            int pw, ph;
            imp.GetSourceTextureWidthAndHeight(out pw, out ph);
            float ppu = imp.spritePixelsPerUnit;
            float nw = pw / ppu; // natural width in units at scale=1
            float nh = ph / ppu; // natural height in units at scale=1
            float scaleW = camW / nw; // scale needed to fill camera width
            float scaleH = camH / nh; // scale needed to fill camera height
            float scaleFit = Mathf.Max(scaleW, scaleH); // scale to cover screen

            Debug.Log($"[BgSizes] {System.IO.Path.GetFileName(path)}: " +
                $"{pw}x{ph}px, PPU={ppu}, natural={nw:F2}x{nh:F2}u, " +
                $"scaleToFitW={scaleW:F3}, scaleToFitH={scaleH:F3}, scaleCover={scaleFit:F3}");
        }
    }
}
