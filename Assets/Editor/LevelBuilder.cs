using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LevelBuilder {
    [MenuItem("Tools/Build Level")]
    public static void Build() {
        string path = "Assets/Art/freecute_tileset/Foreground/Tileset.png";
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        
        List<Sprite> spritesList = new List<Sprite>();
        foreach (var asset in assets) {
            if (asset is Sprite) {
                spritesList.Add((Sprite)asset);
            }
        }
        
        if (spritesList.Count == 0) {
            Debug.LogError("No sprites found in " + path);
            return;
        }
        
        Sprite[] sprites = spritesList.ToArray();

        // We will just use the first sprite for blocks for now.
        Sprite blockSprite = sprites[0]; 

        GameObject parent = new GameObject("Environment");

        // Create a floor
        for (int i = -10; i < 10; i++) {
            GameObject block = new GameObject("Block_" + i);
            block.transform.parent = parent.transform;
            block.transform.position = new Vector3(i, -2, 0);
            
            SpriteRenderer sr = block.AddComponent<SpriteRenderer>();
            sr.sprite = blockSprite;
            
            block.AddComponent<BoxCollider2D>();
        }
        
        // Create some platforms
        for (int i = -3; i < 3; i++) {
            GameObject block = new GameObject("Platform_" + i);
            block.transform.parent = parent.transform;
            block.transform.position = new Vector3(i, 0, 0);
            
            SpriteRenderer sr = block.AddComponent<SpriteRenderer>();
            sr.sprite = blockSprite;
            
            block.AddComponent<BoxCollider2D>();
        }
        
        // Let's add a background if possible
        string bgPath = "Assets/Art/freecute_tileset/Background/Background.png";
        Object[] bgAssets = AssetDatabase.LoadAllAssetsAtPath(bgPath);
        Sprite bgSprite = null;
        foreach (var asset in bgAssets) {
            if (asset is Sprite) {
                bgSprite = (Sprite)asset;
                break;
            }
        }
        
        if (bgSprite != null) {
            GameObject bg = new GameObject("Background");
            bg.transform.parent = parent.transform;
            bg.transform.position = new Vector3(0, 0, 10);
            bg.transform.localScale = new Vector3(5, 5, 1);
            SpriteRenderer bgsr = bg.AddComponent<SpriteRenderer>();
            bgsr.sprite = bgSprite;
        }
    }
}
