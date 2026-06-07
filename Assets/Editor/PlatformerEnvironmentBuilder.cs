using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Builds a coherent 2D platformer environment for "Escenario Prueba".
///
/// MEASURED ASSET DIMENSIONS (PPU = 16 for all):
///   CloudsBack.png   : 512x320px -> 32x20 units at scale=1
///   CloudsFront.png  : 512x320px -> 32x20 units at scale=1
///   BGBack.png       : 512x320px -> 32x20 units at scale=1
///   BGFront.png      : 512x320px -> 32x20 units at scale=1
///   Trees.png        : 256x128px -> 16x8 units at scale=1 (2 trees side by side => each 8x8u)
///   Tileset.png      : 160x96px  -> sliced 16px tiles, each tile = 1x1 unit
///
/// CAMERA: orthographicSize=9 => visible area = 32x18 units (16:9)
///   Y range: -9 to +9
///   X range: -16 to +16
///
/// BACKGROUND POSITIONING STRATEGY:
///   All 512x320 backgrounds are 32x20u. At scale=1.0 they exactly cover the 32u-wide screen
///   and 20u tall vs 18u visible = 2u margin. Perfect.
///
///   Each BG image has its content at specific vertical regions:
///   - CloudsBack:  Sky/clouds fill top ~60%, lower 40% is empty. Center at Y=+1 so sky fills top.
///   - BGBack:      Mountains are in bottom ~30% of image. Place center at Y=-2 so mountains
///                  appear at bottom of screen.
///   - BGFront:     Mountains+grass are in bottom ~40%. Center at Y=-3.
///   - CloudsFront: Wispy clouds in bottom ~50%. Center at Y=-4 so clouds appear near ground level.
///
/// CHARACTER HEIGHT: approximately 1.7 units tall (based on 160x96px Tileset, 16px=1u)
/// GROUND FLOOR: top surface at Y=-4 (camera bottom at -9, leaving room for ground body below)
/// </summary>
public static class PlatformerEnvironmentBuilder
{
    // Sorting orders — lower = further back
    const int SORT_SKY       = -50;
    const int SORT_MNT_FAR   = -40;
    const int SORT_MNT_NEAR  = -35;
    const int SORT_CLOUDS_F  = -20;
    const int SORT_PLATFORM  = 0;
    const int SORT_TREE      = 2;

    // Scene layout
    const float GROUND_TOP = -4f;   // top surface Y of the ground
    const float TILE_W     = 1f;    // each 16px tile = 1 unit (PPU=16)
    const float TILE_H     = 1f;

    [MenuItem("Tools/Build Platformer Environment")]
    public static void Build()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

        // Clean previous
        var old = GameObject.Find("Platformer_Environment");
        if (old != null) Object.DestroyImmediate(old);

        var root = new GameObject("Platformer_Environment");

        // ----------------------------------------------------------------
        // CAMERA SETUP
        // orthographicSize=9 → 18u tall, 32u wide at 16:9
        // Position at (0, 0, -10) so ground at Y=-4 is in lower third of view
        // ----------------------------------------------------------------
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.44f, 0.82f, 0.92f); // sky blue fallback
            cam.orthographicSize = 9f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }

        BuildBackground(root);
        BuildForeground(root);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Builder] Done. Scene saved.");
    }

    // =====================================================================
    //  BACKGROUND LAYERS
    // =====================================================================
    static void BuildBackground(GameObject root)
    {
        var bg = new GameObject("Background");
        bg.transform.SetParent(root.transform);

        // ----------------------------------------------------------------
        // All 512x320 backgrounds = 32x20 units at scale=1.
        // Camera: Y=-9 to Y=+9 (orthoSize=9).
        // 
        // Image pivot is center. At pos.Y=P, image spans P-10 to P+10 vertically.
        // We want each background content to appear in the correct screen region.
        // ----------------------------------------------------------------

        // 1. SKY / CLOUDS BACK — the main sky color with clouds at TOP of image
        //    Center at Y=0 → spans Y=-10 to Y=+10 → fully covers screen -9 to +9 ✓
        PlaceBg(bg, "BG_01_Sky",
            "Assets/Art/freecute_tileset/Background/CloudsBack.png",
            pos:   new Vector3(0f, 0f, 0f),
            scale: Vector3.one,
            sort:  SORT_SKY);

        // 2. FAR MOUNTAINS — lavender silhouette, content in BOTTOM ~30% of image (6u from bottom)
        //    To position mountain peaks at roughly Y=-1 to Y=-3 (mid-screen):
        //    content bottom = pos.Y - 10, content top ≈ pos.Y - 10 + 6 = pos.Y - 4
        //    Want content top at Y=-2 → pos.Y = -2 + 4 = +2
        //    Image spans Y=-8 to Y=+12 → mountains visible at Y=-2 to Y=-8 in screen ✓
        PlaceBg(bg, "BG_02_MountainsFar",
            "Assets/Art/freecute_tileset/Background/BGBack.png",
            pos:   new Vector3(0f, 2f, 0f),
            scale: Vector3.one,
            sort:  SORT_MNT_FAR);

        // 3. NEAR MOUNTAINS — gray mountains + green meadow, content in bottom ~45% (9u)
        //    Want meadow top at Y=-3 → content top at pos.Y - 10 + 9 = pos.Y - 1 = -3 → pos.Y = -2
        //    Image spans Y=-12 to Y=+8. Mountains at Y=-3 to Y=-8 in screen. ✓
        PlaceBg(bg, "BG_03_MountainsNear",
            "Assets/Art/freecute_tileset/Background/BGFront.png",
            pos:   new Vector3(0f, -2f, 0f),
            scale: Vector3.one,
            sort:  SORT_MNT_NEAR);

        // 4. FRONT CLOUDS — wispy lighter clouds in BOTTOM ~50% of image (10u)
        //    We want clouds to appear in the SKY AREA only (Y=0 to Y=+5), NOT below ground.
        //    Content bottom at pos.Y - 10. Content top at pos.Y - 10 + 10 = pos.Y.
        //    Want cloud top at Y=+5 → pos.Y = +5.
        //    Image spans Y=-5 to Y=+15. Cloud mass at Y=-5 to Y=+5. Visible: Y=-5 to Y=+5 (all sky). ✓
        PlaceBg(bg, "BG_04_CloudsFront",
            "Assets/Art/freecute_tileset/Background/CloudsFront.png",
            pos:   new Vector3(0f, 5f, 0f),
            scale: Vector3.one,
            sort:  SORT_CLOUDS_F);
    }

    // =====================================================================
    //  FOREGROUND: GROUND + PLATFORMS + TREES
    // =====================================================================
    static void BuildForeground(GameObject root)
    {
        var fg = new GameObject("Foreground");
        fg.transform.SetParent(root.transform);

        // Load tile sprites (sliced 16px tiles, each = 1x1 unit)
        Sprite tileL = LoadNamedSprite("Assets/Art/freecute_tileset/Foreground/Tileset.png", "tile_0_0");
        Sprite tileC = LoadNamedSprite("Assets/Art/freecute_tileset/Foreground/Tileset.png", "tile_1_0");
        Sprite tileR = LoadNamedSprite("Assets/Art/freecute_tileset/Foreground/Tileset.png", "tile_2_0");

        // Fallback if tiles not yet sliced
        if (tileC == null)
        {
            tileC = LoadSprite("Assets/Art/freecute_tileset/Foreground/Tileset.png");
            tileL = tileC;
            tileR = tileC;
            Debug.LogWarning("[Builder] Using unsliced Tileset. Run 'Tools/Slice Tileset PNG' first.");
        }

        Sprite treeSpr = LoadSprite("Assets/Art/freecute_tileset/Foreground/Trees.png");

        // ----------------------------------------------------------------
        // GROUND FLOOR  — spans full camera width + extra
        // Tiles are 1x1 unit. Ground visible from x=-16 to x=+16.
        // ----------------------------------------------------------------
        BuildPlatform(fg, "Ground",
            tileL, tileC, tileR,
            startX: -17f, endX: 17f, topY: GROUND_TOP,
            solid: true);

        // ----------------------------------------------------------------
        // FLOATING PLATFORMS  — classic platformer layout
        // Spacing designed so the player (≈1.7u tall) can jump between them.
        // Jump height typically 3-4 units in a classic platformer.
        //
        // Platform A: Left cliff          x=[-13,-7]  y=GROUND+4.5
        // Platform B: Center-left step    x=[-5,-2]   y=GROUND+3
        // Platform C: Center high         x=[0,4]     y=GROUND+6
        // Platform D: Right medium        x=[6,12]    y=GROUND+4
        // Platform E: Top tiny            x=[-2,2]    y=GROUND+8.5
        // ----------------------------------------------------------------
        BuildPlatform(fg, "Plat_A_LeftCliff",
            tileL, tileC, tileR,
            startX: -13f, endX: -7f, topY: GROUND_TOP + 4.5f,
            solid: false);

        BuildPlatform(fg, "Plat_B_CenterLeft",
            tileL, tileC, tileR,
            startX: -5f, endX: -2f, topY: GROUND_TOP + 3f,
            solid: false);

        BuildPlatform(fg, "Plat_C_CenterHigh",
            tileL, tileC, tileR,
            startX: 0f, endX: 4f, topY: GROUND_TOP + 6f,
            solid: false);

        BuildPlatform(fg, "Plat_D_Right",
            tileL, tileC, tileR,
            startX: 6f, endX: 12f, topY: GROUND_TOP + 4f,
            solid: false);

        BuildPlatform(fg, "Plat_E_Top",
            tileL, tileC, tileR,
            startX: -2f, endX: 2f, topY: GROUND_TOP + 8.5f,
            solid: false);

        // ----------------------------------------------------------------
        // TREES — Trees.png is 256x128 = 16x8 units (two trees side-by-side).
        // At scale=0.25 each tree occupies 2x1 units — too small.
        // At scale=0.375 → 3x1.5u — still small.
        // A nice tree for a 1.7u character should be ~3-4u tall.
        // Trees.png = 8u per tree at scale=1. Scale=0.4 → 3.2u tall. Good.
        // ----------------------------------------------------------------
        if (treeSpr != null)
        {
            // Trees.png bounds: 16u wide x 8u tall (at scale=1)
            float trH = treeSpr.bounds.size.y; // 8 units at scale=1
            float treeScale = 0.4f;            // → tree ~3.2u tall, visually appropriate

            var trees = new GameObject("Trees");
            trees.transform.SetParent(fg.transform);

            // On ground (left and right sides)
            PlaceTree(trees, treeSpr, -15f, GROUND_TOP, trH, treeScale, "Tree_G_L1");
            PlaceTree(trees, treeSpr,  -9f, GROUND_TOP, trH, treeScale, "Tree_G_L2");
            PlaceTree(trees, treeSpr,  13f, GROUND_TOP, trH, treeScale, "Tree_G_R1");
            PlaceTree(trees, treeSpr,  16f, GROUND_TOP, trH, treeScale, "Tree_G_R2");

            // On Platform A (left cliff) — one tree
            PlaceTree(trees, treeSpr, -11f, GROUND_TOP + 4.5f, trH, treeScale, "Tree_PlatA");

            // On Platform D (right)
            PlaceTree(trees, treeSpr,  10f, GROUND_TOP + 4f, trH, treeScale, "Tree_PlatD");
        }
    }

    // =====================================================================
    //  PLATFORM BUILDER
    // =====================================================================
    /// <summary>
    /// Creates a platform by tiling left/center/right sprites from startX to endX.
    /// Each tile is TILE_W x TILE_H = 1x1 unit.
    /// topY = world Y of the top surface.
    /// solid = BoxCollider2D (ground); !solid = PlatformEffector2D (passable from below).
    /// </summary>
    static void BuildPlatform(GameObject parent, string name,
        Sprite tL, Sprite tC, Sprite tR,
        float startX, float endX, float topY, bool solid)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);

        float totalW = endX - startX;
        int count    = Mathf.Max(1, Mathf.RoundToInt(totalW / TILE_W));
        float cy     = topY - TILE_H * 0.5f;  // center Y of tile row

        for (int i = 0; i < count; i++)
        {
            float x   = startX + i * TILE_W + TILE_W * 0.5f;
            Sprite spr = (count == 1) ? tC
                       : (i == 0)     ? (tL ?? tC)
                       : (i == count-1) ? (tR ?? tC)
                       : tC;

            var tile = new GameObject($"{name}_T{i}");
            tile.transform.SetParent(go.transform);
            tile.transform.position = new Vector3(x, cy, 0f);
            var sr = tile.AddComponent<SpriteRenderer>();
            sr.sprite       = spr;
            sr.sortingOrder = SORT_PLATFORM;
        }

        // One collider per platform (better performance than per-tile)
        var colGO = new GameObject(name + "_Col");
        colGO.transform.SetParent(go.transform);
        colGO.transform.position = new Vector3((startX + endX) * 0.5f, cy, 0f);
        var bc = colGO.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(totalW, TILE_H);

        if (!solid)
        {
            var pe = colGO.AddComponent<PlatformEffector2D>();
            pe.surfaceArc      = 170f;
            bc.usedByEffector  = true;
        }
    }

    // =====================================================================
    //  TREE PLACER
    // =====================================================================
    static void PlaceTree(GameObject parent, Sprite spr,
        float x, float surfaceTopY, float sprNaturalH, float scale, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        // Bottom of tree sits on surface. Tree pivot is center, so:
        // centerY = surfaceTopY + (naturalH * scale / 2)
        go.transform.position   = new Vector3(x, surfaceTopY + sprNaturalH * scale * 0.5f, 0f);
        go.transform.localScale = new Vector3(scale, scale, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = spr;
        sr.sortingOrder = SORT_TREE;
    }

    // =====================================================================
    //  BACKGROUND SPRITE PLACER
    // =====================================================================
    static void PlaceBg(GameObject parent, string name, string path,
        Vector3 pos, Vector3 scale, int sort)
    {
        Sprite spr = LoadSprite(path);
        if (spr == null) { Debug.LogWarning("[Builder] Missing: " + path); return; }
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.position   = pos;
        go.transform.localScale = scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = spr;
        sr.sortingOrder = sort;
    }

    // =====================================================================
    //  ASSET LOADERS
    // =====================================================================
    static Sprite LoadNamedSprite(string path, string spriteName)
    {
        foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(path))
            if (o is Sprite s && s.name == spriteName) return s;
        return null;
    }

    static Sprite LoadSprite(string path)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s != null) return s;
        foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(path))
            if (o is Sprite sp) return sp;
        return null;
    }
}
