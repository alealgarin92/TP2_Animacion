using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.UI;

public class SetupHealthUI : MonoBehaviour
{
    [MenuItem("Tools/Setup Health UI")]
    public static void RunSetup()
    {
        // 1. Slice heart.rotate.png
        string texturePath = "Assets/Art/Heart and health bars/animations/heart.rotate.png";
        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError("heart.rotate.png not found at " + texturePath);
            return;
        }

        // Force sprite settings
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point;
        importer.spritePixelsPerUnit = 32f;
        
        int width = 384;
        int spriteSize = 32;
        int cols = width / spriteSize;
        
        var metadata = new List<SpriteMetaData>();
        for (int i = 0; i < cols; i++)
        {
            SpriteMetaData meta = new SpriteMetaData();
            meta.rect = new Rect(i * spriteSize, 0, spriteSize, spriteSize);
            meta.name = "heart_rotate_" + i;
            meta.alignment = (int)SpriteAlignment.Center;
            meta.pivot = new Vector2(0.5f, 0.5f);
            metadata.Add(meta);
        }
        importer.spritesheet = metadata.ToArray();

        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
        
        Debug.Log("Successfully sliced heart.rotate.png into 12 frames!");

        // 2. Load the sliced sprites
        Object[] objects = AssetDatabase.LoadAllAssetsAtPath(texturePath);
        List<Sprite> sprites = new List<Sprite>();
        foreach (var obj in objects)
        {
            if (obj is Sprite s)
            {
                sprites.Add(s);
            }
        }

        // Sort by name to ensure sequential frames
        sprites.Sort((a, b) => string.Compare(a.name, b.name));

        if (sprites.Count == 0)
        {
            Debug.LogError("No sprites loaded from " + texturePath);
            return;
        }

        // 3. Create Animation Clip
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 12f;
        
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count];
        for (int i = 0; i < sprites.Count; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe();
            keyframes[i].time = i * (1f / 12f);
            keyframes[i].value = sprites[i];
        }

        EditorCurveBinding binding = new EditorCurveBinding();
        binding.type = typeof(Image);
        binding.path = "";
        binding.propertyName = "m_Sprite";

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        string animFolder = "Assets/Art/Heart and health bars/animations";
        if (!Directory.Exists(animFolder))
        {
            Directory.CreateDirectory(animFolder);
        }

        string animPath = animFolder + "/heart_rotate.anim";
        AssetDatabase.CreateAsset(clip, animPath);
        AssetDatabase.SaveAssets();
        Debug.Log("Successfully created AnimationClip at " + animPath);

        // 4. Create Animator Controller
        string controllerPath = animFolder + "/AC_Heart.controller";
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.layers[0].stateMachine.AddState("heart_rotate").motion = clip;
        Debug.Log("Successfully created AnimatorController at " + controllerPath);

        // 5. Create Prefab
        string prefabFolder = "Assets/Art/Heart and health bars/prefabs";
        if (!Directory.Exists(prefabFolder))
        {
            Directory.CreateDirectory(prefabFolder);
        }
        string prefabPath = prefabFolder + "/HeartUI.prefab";

        GameObject tempHeart = new GameObject("HeartUI", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Animator));
        Image img = tempHeart.GetComponent<Image>();
        img.sprite = sprites[0];
        img.rectTransform.sizeDelta = new Vector2(32, 32);
        
        Animator anim = tempHeart.GetComponent<Animator>();
        anim.runtimeAnimatorController = controller;

        GameObject heartPrefab = PrefabUtility.SaveAsPrefabAsset(tempHeart, prefabPath);
        DestroyImmediate(tempHeart);
        Debug.Log("Successfully created HeartUI prefab at " + prefabPath);

        // 6. Setup Canvas and Hierarchy in Active Scene
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("UI Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Ensure EventSystem exists
        GameObject esObj = GameObject.Find("EventSystem");
        if (esObj == null)
        {
            var es = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es != null) esObj = es.gameObject;
        }

        if (esObj == null)
        {
            esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        }

        System.Type newModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (newModuleType != null)
        {
            var oldModule = esObj.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (oldModule != null)
            {
                UnityEngine.Object.DestroyImmediate(oldModule);
            }
            if (esObj.GetComponent(newModuleType) == null)
            {
                esObj.AddComponent(newModuleType);
            }
        }
        else
        {
            if (esObj.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>() == null)
            {
                esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        // Create HealthPanel under Canvas
        Transform healthPanel = canvas.transform.Find("PlayerHealthUI");
        GameObject healthPanelGo;
        if (healthPanel == null)
        {
            healthPanelGo = new GameObject("PlayerHealthUI", typeof(RectTransform));
            healthPanelGo.transform.SetParent(canvas.transform, false);
            healthPanel = healthPanelGo.transform;
        }
        else
        {
            healthPanelGo = healthPanel.gameObject;
        }

        RectTransform panelRect = healthPanelGo.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(20, -20);
        panelRect.sizeDelta = new Vector2(200, 50);

        // Create HeartsContainer under HealthPanel
        Transform container = healthPanel.Find("HeartsContainer");
        GameObject containerGo;
        if (container == null)
        {
            containerGo = new GameObject("HeartsContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            containerGo.transform.SetParent(healthPanel, false);
            container = containerGo.transform;
        }
        else
        {
            containerGo = container.gameObject;
        }

        RectTransform containerRect = containerGo.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0.5f);
        containerRect.anchorMax = new Vector2(0, 0.5f);
        containerRect.pivot = new Vector2(0, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;

        HorizontalLayoutGroup layout = containerGo.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 5f;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = false;
        layout.childControlHeight = false;

        ContentSizeFitter fitter = containerGo.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Attach HealthUI script
        HealthUI healthUI = containerGo.GetComponent<HealthUI>();
        if (healthUI == null)
        {
            healthUI = containerGo.AddComponent<HealthUI>();
        }

        // Setup references
        healthUI.player = Object.FindFirstObjectByType<PlayerController>();
        healthUI.heartPrefab = heartPrefab;

        // 7. Setup Game Over UI under Canvas
        Transform gameOverPanel = canvas.transform.Find("GameOverPanel");
        GameObject gameOverPanelGo;
        if (gameOverPanel == null)
        {
            gameOverPanelGo = new GameObject("GameOverPanel", typeof(RectTransform), typeof(Image), typeof(GameOverUI));
            gameOverPanelGo.transform.SetParent(canvas.transform, false);
            gameOverPanel = gameOverPanelGo.transform;
        }
        else
        {
            gameOverPanelGo = gameOverPanel.gameObject;
        }

        // Configure Game Over Panel background image
        Image bgImage = gameOverPanelGo.GetComponent<Image>();
        bgImage.color = new Color(0.1f, 0.02f, 0.02f, 0.85f); // Beautiful dark red/black transparent overlay
        
        RectTransform gameOverRect = gameOverPanelGo.GetComponent<RectTransform>();
        gameOverRect.anchorMin = Vector2.zero;
        gameOverRect.anchorMax = Vector2.one;
        gameOverRect.pivot = new Vector2(0.5f, 0.5f);
        gameOverRect.anchoredPosition = Vector2.zero;
        gameOverRect.sizeDelta = Vector2.zero; // Full screen

        // 8. Create Game Over Text
        Transform gameOverText = gameOverPanel.Find("GameOverText");
        GameObject gameOverTextGo;
        if (gameOverText == null)
        {
            gameOverTextGo = new GameObject("GameOverText", typeof(RectTransform), typeof(Text));
            gameOverTextGo.transform.SetParent(gameOverPanel, false);
            gameOverText = gameOverTextGo.transform;
        }
        else
        {
            gameOverTextGo = gameOverText.gameObject;
        }

        Text txt = gameOverTextGo.GetComponent<Text>();
        txt.text = "GAME OVER";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = new Color(0.9f, 0.1f, 0.1f, 1f); // Vibrant red color
        txt.fontSize = 48;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // Compatible built-in font
        txt.fontStyle = FontStyle.Bold;

        RectTransform textRect = gameOverTextGo.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.6f);
        textRect.anchorMax = new Vector2(0.5f, 0.6f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(400, 100);

        // 9. Create Retry Button
        Transform retryButtonTrans = gameOverPanel.Find("RetryButton");
        GameObject retryButtonGo;
        if (retryButtonTrans == null)
        {
            retryButtonGo = new GameObject("RetryButton", typeof(RectTransform), typeof(Image), typeof(Button));
            retryButtonGo.transform.SetParent(gameOverPanel, false);
            retryButtonTrans = retryButtonGo.transform;

            GameObject btnTextGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            btnTextGo.transform.SetParent(retryButtonTrans, false);
            
            Text btnTxt = btnTextGo.GetComponent<Text>();
            btnTxt.text = "Reintentar";
            btnTxt.alignment = TextAnchor.MiddleCenter;
            btnTxt.color = Color.white;
            btnTxt.fontSize = 20;
            btnTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnTxt.fontStyle = FontStyle.Bold;

            RectTransform btnTextRect = btnTextGo.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.pivot = new Vector2(0.5f, 0.5f);
            btnTextRect.anchoredPosition = Vector2.zero;
            btnTextRect.sizeDelta = Vector2.zero;
        }
        else
        {
            retryButtonGo = retryButtonTrans.gameObject;
        }

        Image btnImg = retryButtonGo.GetComponent<Image>();
        btnImg.color = new Color(0.4f, 0.05f, 0.05f, 1f); // Dark red button background

        Button btn = retryButtonGo.GetComponent<Button>();
        
        RectTransform btnRect = retryButtonGo.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.4f);
        btnRect.anchorMax = new Vector2(0.5f, 0.4f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = Vector2.zero;
        btnRect.sizeDelta = new Vector2(160, 50);

        // 10. Hook up GameOverUI references
        GameOverUI gameOverUI = gameOverPanelGo.GetComponent<GameOverUI>();
        if (gameOverUI == null)
        {
            gameOverUI = gameOverPanelGo.AddComponent<GameOverUI>();
        }
        gameOverUI.player = Object.FindFirstObjectByType<PlayerController>();
        gameOverUI.retryButton = btn;

        // Hook up PlayerController reference to gameOverPanel
        if (gameOverUI.player != null)
        {
            gameOverUI.player.gameOverPanel = gameOverPanelGo;
            EditorUtility.SetDirty(gameOverUI.player);
        }

        // Initially deactivate the game over panel
        gameOverPanelGo.SetActive(false);

        EditorUtility.SetDirty(containerGo);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        
        Debug.Log("Successfully configured Canvas UI and Game Over Panel in scene!");
    }
}
