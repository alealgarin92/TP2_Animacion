using UnityEngine;
using UnityEditor;

public static class EnvDiagnostic
{
    [MenuItem("Tools/Diagnose Environment")]
    public static void Run()
    {
        GameObject root = GameObject.Find("Platformer_Environment");
        if (root == null) { Debug.LogError("[Diag] Platformer_Environment not found!"); return; }

        // Count SpriteRenderers
        SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        int nullSprites = 0;
        int validSprites = 0;

        foreach (var sr in renderers)
        {
            if (sr.sprite == null)
            {
                nullSprites++;
                Debug.LogWarning("[Diag] NULL sprite on: " + sr.gameObject.name + " (path: " + GetPath(sr.transform) + ")");
            }
            else
            {
                validSprites++;
            }
        }

        // Count BoxCollider2Ds
        BoxCollider2D[] colliders = root.GetComponentsInChildren<BoxCollider2D>(true);

        Debug.Log($"[Diag] SUMMARY: Valid sprites={validSprites}, Null sprites={nullSprites}, Colliders={colliders.Length}");

        // Print first few valid sprite names
        int shown = 0;
        foreach (var sr in renderers)
        {
            if (sr.sprite != null && shown < 5)
            {
                Debug.Log($"[Diag] Sample: {sr.gameObject.name} => sprite '{sr.sprite.name}' bounds={sr.sprite.bounds.size}");
                shown++;
            }
        }
    }

    static string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
        return path;
    }
}
