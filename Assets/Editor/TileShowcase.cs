using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public static class TileShowcase
{
    [MenuItem("Tools/Show All Tiles")]
    public static void ShowTiles()
    {
        var old = GameObject.Find("TileShowcase");
        if (old != null) Object.DestroyImmediate(old);

        var root = new GameObject("TileShowcase");

        Object[] all = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/freecute_tileset/Foreground/Tileset.png");
        int count = 0;
        foreach (Object o in all)
        {
            if (!(o is Sprite)) continue;
            Sprite s = (Sprite)o;
            
            // Extract col and row from name: tile_col_row
            string[] parts = s.name.Split('_');
            if (parts.Length == 3 && int.TryParse(parts[1], out int col) && int.TryParse(parts[2], out int row))
            {
                var go = new GameObject(s.name);
                go.transform.SetParent(root.transform);
                // Grid positioning: col goes right, row goes down
                go.transform.position = new Vector3(col * 1.2f - 5.5f, -row * 1.2f + 3f, 0f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = s;
                sr.sortingOrder = 100;
                count++;
            }
        }

        Debug.Log($"[TileShowcase] Arrayed {count} tiles in a grid.");
    }

    [MenuItem("Tools/Remove Tile Showcase")]
    public static void RemoveShowcase()
    {
        var old = GameObject.Find("TileShowcase");
        if (old != null) Object.DestroyImmediate(old);
    }
}
