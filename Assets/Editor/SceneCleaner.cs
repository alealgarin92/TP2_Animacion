using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class SceneCleaner
{
    [MenuItem("Tools/Clean Old Environment Objects")]
    public static void Clean()
    {
        // Remove leftover objects from previous attempts
        string[] oldNames = { "Background", "Environment" };
        
        foreach (string n in oldNames)
        {
            GameObject go = GameObject.Find(n);
            if (go != null)
            {
                // Make sure it's not the one inside Platformer_Environment
                if (go.transform.parent == null)
                {
                    Object.DestroyImmediate(go);
                    Debug.Log("[SceneCleaner] Removed root object: " + n);
                }
            }
        }

        // Also check for stray LevelBuilder artifacts
        var stray = GameObject.Find("LevelBuilder_Environment");
        if (stray != null) Object.DestroyImmediate(stray);

        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        Debug.Log("[SceneCleaner] Done.");
    }
}
