using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class SetupPlayerAnimator
{
    [MenuItem("Tools/Setup Player Animator")]
    public static void Setup()
    {
        string path = "Assets/Characters/Player/AC_Player.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller == null)
        {
            Debug.LogError("AnimatorController not found at " + path);
            return;
        }

        // Add float parameter "idleSpeedMultiplier" if it doesn't exist
        AddParameterIfNotExists(controller, "idleSpeedMultiplier", AnimatorControllerParameterType.Float, 0.5f);

        var rootStateMachine = controller.layers[0].stateMachine;

        // Find state "player_idle" recursively
        AnimatorState idleState = FindState(rootStateMachine, "player_idle");

        if (idleState == null)
        {
            Debug.LogError("State player_idle not found!");
            return;
        }

        // Configure speed multiplier
        idleState.speed = 1.0f;
        idleState.speedParameterActive = true;
        idleState.speedParameter = "idleSpeedMultiplier";

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("Animator Controller AC_Player configured successfully with idleSpeedMultiplier!");
    }

    private static void AddParameterIfNotExists(AnimatorController controller, string name, AnimatorControllerParameterType type, float defaultValue)
    {
        foreach (var param in controller.parameters)
        {
            if (param.name == name) return;
        }
        controller.AddParameter(new AnimatorControllerParameter() {
            name = name,
            type = type,
            defaultFloat = defaultValue
        });
    }

    private static AnimatorState FindState(AnimatorStateMachine stateMachine, string name)
    {
        foreach (var childState in stateMachine.states)
        {
            if (childState.state.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            {
                return childState.state;
            }
        }

        foreach (var childStateMachine in stateMachine.stateMachines)
        {
            AnimatorState state = FindState(childStateMachine.stateMachine, name);
            if (state != null) return state;
        }

        return null;
    }
}
