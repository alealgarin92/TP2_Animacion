using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class SetupMainMenu : MonoBehaviour
{
    [MenuItem("Tools/Setup Main Menu")]
    public static void RunSetup()
    {
        // 1. Create a new scene and save it
        var activeScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
            UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, 
            UnityEditor.SceneManagement.NewSceneMode.Single);

        // 2. Create Camera
        GameObject camGo = new GameObject("Main Camera");
        Camera cam = camGo.AddComponent<Camera>();
        camGo.tag = "MainCamera";
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;

        // 3. Create Canvas
        GameObject canvasObj = new GameObject("UI Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Ensure EventSystem exists
        GameObject esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        
        System.Type newModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (newModuleType != null)
        {
            esObj.AddComponent(newModuleType);
        }
        else
        {
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 4. Create Background Image
        GameObject bgGo = new GameObject("BackgroundImage", typeof(RectTransform), typeof(Image));
        bgGo.transform.SetParent(canvas.transform, false);
        Image bgImage = bgGo.GetComponent<Image>();
        bgImage.color = Color.white;
        
        RectTransform bgRect = bgGo.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta = Vector2.zero;

        // 5. Create Main Menu Script Component on Canvas
        MainMenu mainMenu = canvasObj.AddComponent<MainMenu>();
        mainMenu.backgroundImage = bgImage;

        // Set up custom hardware cursor using a mini sword
        string cursorPath = "Assets/Art/rpg_icon_pack/icons/16x16/sword_01a.png";
        TextureImporter cursorImporter = AssetImporter.GetAtPath(cursorPath) as TextureImporter;
        if (cursorImporter != null)
        {
            cursorImporter.textureType = TextureImporterType.Cursor;
            cursorImporter.SaveAndReimport();
        }
        Texture2D cursorTex = AssetDatabase.LoadAssetAtPath<Texture2D>(cursorPath);
        mainMenu.cursorTexture = cursorTex;

        // Load background sprites to assign to script
        List<Sprite> bgSprites = new List<Sprite>();
        string[] bgPaths = new string[] {
            "Assets/Art/freecute_tileset/Background/menu_background.png",
            "Assets/Art/freecute_tileset/Background/BGBack.png",
            "Assets/Art/freecute_tileset/Background/BGFront.png",
            "Assets/Art/freecute_tileset/Background/Layer_0011_0.png",
            "Assets/Art/freecute_tileset/Background/Layer_0010_1.png",
            "Assets/Art/freecute_tileset/Background/Layer_0009_2.png"
        };
        foreach (var path in bgPaths)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null)
            {
                bgSprites.Add(s);
            }
        }
        mainMenu.backgrounds = bgSprites.ToArray();
        if (bgSprites.Count > 0)
        {
            bgImage.sprite = bgSprites[0];
        }

        // 6. Create Main Panel (Menu Principal)
        GameObject mainPanelGo = new GameObject("MainPanel", typeof(RectTransform));
        mainPanelGo.transform.SetParent(canvas.transform, false);
        RectTransform mainPanelRect = mainPanelGo.GetComponent<RectTransform>();
        mainPanelRect.anchorMin = Vector2.zero;
        mainPanelRect.anchorMax = Vector2.one;
        mainPanelRect.pivot = new Vector2(0.5f, 0.5f);
        mainPanelRect.anchoredPosition = Vector2.zero;
        mainPanelRect.sizeDelta = Vector2.zero;
        mainMenu.mainPanel = mainPanelGo;

        // 7. Create Decorative Border
        GameObject borderGo = new GameObject("DecorativeBorder", typeof(RectTransform), typeof(Image));
        borderGo.transform.SetParent(mainPanelGo.transform, false);
        Image borderImg = borderGo.GetComponent<Image>();
        borderImg.color = new Color(0.12f, 0.12f, 0.16f, 0.4f); // Transparent dark border overlay
        RectTransform borderRect = borderGo.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.pivot = new Vector2(0.5f, 0.5f);
        borderRect.anchoredPosition = Vector2.zero;
        borderRect.sizeDelta = new Vector2(-40, -40); // 20-pixel border margin

        // 8. Create Title Text (using standard UI Text for reliability)
        GameObject titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(Text));
        titleGo.transform.SetParent(mainPanelGo.transform, false);
        Text titleText = titleGo.GetComponent<Text>();
        titleText.text = "GUERREROS\nDE LA CIMA";
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.fontSize = 55;
        Font titleFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/UI/Fonts/alagard.ttf");
        if (titleFont != null)
        {
            titleText.font = titleFont;
        }
        else
        {
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontStyle = FontStyle.Bold;
        }
        titleText.color = new Color(0.85f, 0.15f, 0.15f); // Reddish color like screenshot
        titleText.lineSpacing = 0.9f;

        // Add outline for premium gold border effect
        Outline titleOutline = titleGo.AddComponent<Outline>();
        titleOutline.effectColor = new Color(0.95f, 0.8f, 0.3f, 1f); // Golden outline
        titleOutline.effectDistance = new Vector2(3f, -3f);

        // Add shadow for extra depth
        Shadow titleShadow = titleGo.AddComponent<Shadow>();
        titleShadow.effectColor = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Dark shadow
        titleShadow.effectDistance = new Vector2(4f, -4f);

        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.75f);
        titleRect.anchorMax = new Vector2(0.5f, 0.75f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(500, 200);

        // 9. Create Buttons Container
        GameObject btnContainerGo = new GameObject("ButtonsContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
        btnContainerGo.transform.SetParent(mainPanelGo.transform, false);
        VerticalLayoutGroup vLayout = btnContainerGo.GetComponent<VerticalLayoutGroup>();
        vLayout.spacing = 15f;
        vLayout.childAlignment = TextAnchor.MiddleCenter;
        vLayout.childForceExpandWidth = false;
        vLayout.childForceExpandHeight = false;
        vLayout.childControlWidth = false;
        vLayout.childControlHeight = false;

        RectTransform containerRect = btnContainerGo.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.35f);
        containerRect.anchorMax = new Vector2(0.5f, 0.35f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(300, 250);

        // Style helper for buttons
        Color btnNormalColor = new Color(0.24f, 0.28f, 0.32f, 1f); // Greyish-blue slate color like screenshot

        // A. JUGAR Button
        GameObject playBtnGo = CreateMenuButton("JUGAR", btnContainerGo.transform, btnNormalColor);
        Button playBtn = playBtnGo.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(playBtn.onClick, new UnityEngine.Events.UnityAction(mainMenu.PlayGame));
        
        // B. OPCIONES Button
        GameObject optBtnGo = CreateMenuButton("OPCIONES", btnContainerGo.transform, btnNormalColor);
        Button optBtn = optBtnGo.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(optBtn.onClick, new UnityEngine.Events.UnityAction(mainMenu.OpenOptions));
        
        // C. SALIR Button
        GameObject quitBtnGo = CreateMenuButton("SALIR", btnContainerGo.transform, btnNormalColor);
        Button quitBtn = quitBtnGo.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(quitBtn.onClick, new UnityEngine.Events.UnityAction(mainMenu.QuitGame));

        // 10. Create Options Panel
        GameObject optPanelGo = new GameObject("OptionsPanel", typeof(RectTransform));
        optPanelGo.transform.SetParent(canvas.transform, false);
        RectTransform optPanelRect = optPanelGo.GetComponent<RectTransform>();
        optPanelRect.anchorMin = Vector2.zero;
        optPanelRect.anchorMax = Vector2.one;
        optPanelRect.pivot = new Vector2(0.5f, 0.5f);
        optPanelRect.anchoredPosition = Vector2.zero;
        optPanelRect.sizeDelta = Vector2.zero;
        mainMenu.optionsPanel = optPanelGo;

        // Dark transparent background overlay for options
        GameObject optBgGo = new GameObject("OptionsBackgroundOverlay", typeof(RectTransform), typeof(Image));
        optBgGo.transform.SetParent(optPanelGo.transform, false);
        Image optBgImg = optBgGo.GetComponent<Image>();
        optBgImg.color = new Color(0.08f, 0.08f, 0.12f, 0.85f);
        RectTransform optBgRect = optBgGo.GetComponent<RectTransform>();
        optBgRect.anchorMin = Vector2.zero;
        optBgRect.anchorMax = Vector2.one;
        optBgRect.pivot = new Vector2(0.5f, 0.5f);
        optBgRect.anchoredPosition = Vector2.zero;
        optBgRect.sizeDelta = Vector2.zero;

        // Options Title
        GameObject optTitleGo = new GameObject("OptionsTitle", typeof(RectTransform), typeof(Text));
        optTitleGo.transform.SetParent(optPanelGo.transform, false);
        Text optTitleText = optTitleGo.GetComponent<Text>();
        optTitleText.text = "CONFIGURACIÓN";
        optTitleText.alignment = TextAnchor.MiddleCenter;
        optTitleText.fontSize = 45;
        Font optTitleFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/UI/Fonts/alagard.ttf");
        if (optTitleFont != null)
        {
            optTitleText.font = optTitleFont;
        }
        else
        {
            optTitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            optTitleText.fontStyle = FontStyle.Bold;
        }
        optTitleText.color = Color.white;

        // Add shadow for extra legibility
        Shadow optTitleShadow = optTitleGo.AddComponent<Shadow>();
        optTitleShadow.effectColor = new Color(0.1f, 0.1f, 0.1f, 0.7f);
        optTitleShadow.effectDistance = new Vector2(2f, -2f);

        RectTransform optTitleRect = optTitleGo.GetComponent<RectTransform>();
        optTitleRect.anchorMin = new Vector2(0.5f, 0.8f);
        optTitleRect.anchorMax = new Vector2(0.5f, 0.8f);
        optTitleRect.pivot = new Vector2(0.5f, 0.5f);
        optTitleRect.anchoredPosition = Vector2.zero;
        optTitleRect.sizeDelta = new Vector2(400, 80);

        // Background Selection Section
        GameObject selectionGo = new GameObject("BackgroundSelection", typeof(RectTransform));
        selectionGo.transform.SetParent(optPanelGo.transform, false);
        RectTransform selectionRect = selectionGo.GetComponent<RectTransform>();
        selectionRect.anchorMin = new Vector2(0.5f, 0.5f);
        selectionRect.anchorMax = new Vector2(0.5f, 0.5f);
        selectionRect.pivot = new Vector2(0.5f, 0.5f);
        selectionRect.anchoredPosition = Vector2.zero;
        selectionRect.sizeDelta = new Vector2(450, 100);

        // Selection Label Text
        GameObject labelGo = new GameObject("SelectionLabel", typeof(RectTransform), typeof(Text));
        labelGo.transform.SetParent(selectionGo.transform, false);
        Text labelText = labelGo.GetComponent<Text>();
        labelText.text = "CAMBIAR FONDO";
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.fontSize = 28;
        Font labelFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/UI/Fonts/m5x7.ttf");
        if (labelFont != null)
        {
            labelText.font = labelFont;
        }
        else
        {
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontStyle = FontStyle.Bold;
        }
        labelText.color = Color.white;
        RectTransform labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0.7f);
        labelRect.anchorMax = new Vector2(0.5f, 0.7f);
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = Vector2.zero;
        labelRect.sizeDelta = new Vector2(300, 40);

        // Previous Button
        GameObject prevBtnGo = CreateMenuButton("< ANTERIOR", selectionGo.transform, btnNormalColor);
        Button prevBtn = prevBtnGo.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(prevBtn.onClick, new UnityEngine.Events.UnityAction(mainMenu.PreviousBackground));
        RectTransform prevBtnRect = prevBtnGo.GetComponent<RectTransform>();
        prevBtnRect.anchorMin = new Vector2(0, 0.2f);
        prevBtnRect.anchorMax = new Vector2(0, 0.2f);
        prevBtnRect.pivot = new Vector2(0, 0.5f);
        prevBtnRect.anchoredPosition = new Vector2(20, 0);

        // Next Button
        GameObject nextBtnGo = CreateMenuButton("SIGUIENTE >", selectionGo.transform, btnNormalColor);
        Button nextBtn = nextBtnGo.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(nextBtn.onClick, new UnityEngine.Events.UnityAction(mainMenu.NextBackground));
        RectTransform nextBtnRect = nextBtnGo.GetComponent<RectTransform>();
        nextBtnRect.anchorMin = new Vector2(1, 0.2f);
        nextBtnRect.anchorMax = new Vector2(1, 0.2f);
        nextBtnRect.pivot = new Vector2(1, 0.5f);
        nextBtnRect.anchoredPosition = new Vector2(-20, 0);

        // Back / Accept Button
        GameObject backBtnGo = CreateMenuButton("ACEPTAR", optPanelGo.transform, new Color(0.15f, 0.45f, 0.15f));
        Button backBtn = backBtnGo.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(backBtn.onClick, new UnityEngine.Events.UnityAction(mainMenu.CloseOptions));
        RectTransform backBtnRect = backBtnGo.GetComponent<RectTransform>();
        backBtnRect.anchorMin = new Vector2(0.5f, 0.25f);
        backBtnRect.anchorMax = new Vector2(0.5f, 0.25f);
        backBtnRect.pivot = new Vector2(0.5f, 0.5f);
        backBtnRect.anchoredPosition = Vector2.zero;

        // 11. Initial Activation States
        mainPanelGo.SetActive(true);
        optPanelGo.SetActive(false);

        // Save active scene
        string scenePath = "Assets/Scenes/MainMenu.unity";
        if (!Directory.Exists("Assets/Scenes"))
        {
            Directory.CreateDirectory("Assets/Scenes");
        }
        
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene, scenePath);
        Debug.Log("Successfully created MainMenu scene at " + scenePath);

        // 12. Update Build Settings
        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        
        // Remove old main menu scene from build settings if it exists
        buildScenes.RemoveAll(s => s.path.Contains("MainMenu.unity"));

        // Insert new MainMenu scene at index 0
        buildScenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));

        // Ensure "Escenario Prueba" exists in Build Settings
        string gameScenePath = "Assets/Scenes/Escenario Prueba.unity";
        if (File.Exists(gameScenePath))
        {
            if (!buildScenes.Exists(s => s.path == gameScenePath))
            {
                buildScenes.Add(new EditorBuildSettingsScene(gameScenePath, true));
            }
            else
            {
                // Move it to index 1 if it is at index 0
                int idx = buildScenes.FindIndex(s => s.path == gameScenePath);
                if (idx == 0 && buildScenes.Count > 1)
                {
                    var temp = buildScenes[0];
                    buildScenes[0] = buildScenes[1];
                    buildScenes[1] = temp;
                }
            }
        }

        EditorBuildSettings.scenes = buildScenes.ToArray();
        Debug.Log("Successfully updated Editor Build Settings with MainMenu scene at index 0!");
        
        // Clean dirty states
        EditorUtility.SetDirty(canvasObj);
    }

    private static GameObject CreateMenuButton(string text, Transform parent, Color baseColor)
    {
        // 1. Create Root Button GameObject
        GameObject btnGo = new GameObject(text + "Button", typeof(RectTransform), typeof(Button));
        btnGo.transform.SetParent(parent, false);

        RectTransform btnRect = btnGo.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(200, 50);

        // 2. Highlight Border Image (drawn behind the button)
        GameObject highlightGo = new GameObject("HighlightBorder", typeof(RectTransform), typeof(Image));
        highlightGo.transform.SetParent(btnGo.transform, false);
        Image highlightImg = highlightGo.GetComponent<Image>();
        highlightImg.color = new Color(0.95f, 0.8f, 0.3f, 1f); // Bright yellow/gold outline color
        RectTransform highlightRect = highlightGo.GetComponent<RectTransform>();
        highlightRect.anchorMin = Vector2.zero;
        highlightRect.anchorMax = Vector2.one;
        highlightRect.pivot = new Vector2(0.5f, 0.5f);
        highlightRect.anchoredPosition = Vector2.zero;
        highlightRect.sizeDelta = new Vector2(8, 8); // 4-pixel border expansion

        // 3. Dark Outer Border Image
        GameObject borderGo = new GameObject("OuterBorder", typeof(RectTransform), typeof(Image));
        borderGo.transform.SetParent(btnGo.transform, false);
        Image borderImg = borderGo.GetComponent<Image>();
        borderImg.color = new Color(0.12f, 0.14f, 0.16f, 1f); // Dark slate grey border
        RectTransform borderRect = borderGo.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.pivot = new Vector2(0.5f, 0.5f);
        borderRect.anchoredPosition = Vector2.zero;
        borderRect.sizeDelta = new Vector2(4, 4); // 2-pixel border expansion

        // 4. Fill/Background Image
        GameObject fillGo = new GameObject("FillBackground", typeof(RectTransform), typeof(Image));
        fillGo.transform.SetParent(btnGo.transform, false);
        Image fillImg = fillGo.GetComponent<Image>();
        fillImg.color = baseColor; // Slate blue color (#3d4750)
        RectTransform fillRect = fillGo.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.pivot = new Vector2(0.5f, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = Vector2.zero;

        // 5. Corner Accents/Brackets (to look like pixel art metal notches)
        Color accentColor = new Color(0.48f, 0.54f, 0.6f, 1f); // Lighter blue-grey accent
        CreateCornerAccent(fillGo.transform, new Vector2(0, 1), new Vector2(1, -1), accentColor); // Top-Left
        CreateCornerAccent(fillGo.transform, new Vector2(1, 1), new Vector2(-1, -1), accentColor); // Top-Right
        CreateCornerAccent(fillGo.transform, new Vector2(0, 0), new Vector2(1, 1), accentColor); // Bottom-Left
        CreateCornerAccent(fillGo.transform, new Vector2(1, 0), new Vector2(-1, 1), accentColor); // Bottom-Right

        // 6. Text child GameObject
        GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
        txtGo.transform.SetParent(fillGo.transform, false);

        Text txt = txtGo.GetComponent<Text>();
        txt.text = text;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = 26;
        Font btnFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/UI/Fonts/m5x7.ttf");
        if (btnFont != null)
        {
            txt.font = btnFont;
        }
        else
        {
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontStyle = FontStyle.Bold;
        }
        txt.color = Color.white;

        RectTransform txtRect = txtGo.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.pivot = new Vector2(0.5f, 0.5f);
        txtRect.anchoredPosition = Vector2.zero;
        txtRect.sizeDelta = Vector2.zero;

        // 7. Configure Button Component transitions
        Button btn = btnGo.GetComponent<Button>();
        btn.targetGraphic = fillImg;
        
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        colors.selectedColor = Color.white;
        btn.colors = colors;

        // 8. Add Hover Helper Component
        MenuButtonHover hoverHelper = btnGo.AddComponent<MenuButtonHover>();
        hoverHelper.highlightBorder = highlightImg;
        hoverHelper.buttonText = txt;

        return btnGo;
    }

    private static void CreateCornerAccent(Transform parent, Vector2 anchor, Vector2 direction, Color color)
    {
        // Creates a small pixel-art styled bracket corner detail (3 L-shaped pixels)
        GameObject accentGo = new GameObject("CornerAccent", typeof(RectTransform), typeof(Image));
        accentGo.transform.SetParent(parent, false);
        Image img = accentGo.GetComponent<Image>();
        img.color = color;

        RectTransform rect = accentGo.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.sizeDelta = new Vector2(6, 6);
        rect.anchoredPosition = direction * 3;
    }
}
