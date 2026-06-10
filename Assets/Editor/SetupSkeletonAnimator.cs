using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class SetupSkeletonAnimator
{
    [MenuItem("Tools/Setup Skeleton Animator")]
    public static void Setup()
    {
        string path = "Assets/Art/monsters_creature_fantasy/Skeleton/Animation/AC_Skeleton.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller == null)
        {
            Debug.LogError("AnimatorController not found at " + path);
            return;
        }

        // Add parameters if they don't exist
        AddParameterIfNotExists(controller, "isMoving", AnimatorControllerParameterType.Bool);
        AddParameterIfNotExists(controller, "attack", AnimatorControllerParameterType.Trigger);
        AddParameterIfNotExists(controller, "takeHit", AnimatorControllerParameterType.Trigger);

        var rootStateMachine = controller.layers[0].stateMachine;

        // Find states
        AnimatorState idleState = FindState(rootStateMachine, "skeleton_idle");
        AnimatorState walkState = FindState(rootStateMachine, "skeleton_walk");
        AnimatorState attackState = FindState(rootStateMachine, "skeleton_attack");
        AnimatorState takeHitState = FindState(rootStateMachine, "skeleton_take_hit");
        AnimatorState deathState = FindState(rootStateMachine, "skeleton_death");

        if (idleState == null || walkState == null || attackState == null || takeHitState == null || deathState == null)
        {
            Debug.LogError("One of the states is missing! idle:" + (idleState != null) + ", walk:" + (walkState != null) + ", attack:" + (attackState != null) + ", takeHit:" + (takeHitState != null) + ", death:" + (deathState != null));
            return;
        }

        // Clear existing transitions on these states to rebuild them cleanly
        ClearTransitions(idleState);
        ClearTransitions(walkState);
        ClearTransitions(attackState);
        ClearTransitions(takeHitState);

        // Transition: Idle -> Walk
        var idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(AnimatorConditionMode.If, 0, "isMoving");
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0f;

        // Transition: Walk -> Idle
        var walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isMoving");
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0f;

        // Reset AnyState transitions
        rootStateMachine.anyStateTransitions = new AnimatorStateTransition[0];

        // AnyState -> Death when isDeath is true
        var anyToDeath = rootStateMachine.AddAnyStateTransition(deathState);
        anyToDeath.AddCondition(AnimatorConditionMode.If, 0, "isDeath");
        anyToDeath.hasExitTime = false;
        anyToDeath.duration = 0f;

        // AnyState -> Attack on attack trigger
        var anyToAttack = rootStateMachine.AddAnyStateTransition(attackState);
        anyToAttack.AddCondition(AnimatorConditionMode.If, 0, "attack");
        anyToAttack.hasExitTime = false;
        anyToAttack.duration = 0f;

        // AnyState -> TakeHit on takeHit trigger
        var anyToTakeHit = rootStateMachine.AddAnyStateTransition(takeHitState);
        anyToTakeHit.AddCondition(AnimatorConditionMode.If, 0, "takeHit");
        anyToTakeHit.hasExitTime = false;
        anyToTakeHit.duration = 0f;

        // Transitions out of Attack / TakeHit back to Idle
        var attackToIdle = attackState.AddTransition(idleState);
        attackToIdle.hasExitTime = true;
        attackToIdle.exitTime = 1f;
        attackToIdle.duration = 0f;

        var takeHitToIdle = takeHitState.AddTransition(idleState);
        takeHitToIdle.hasExitTime = true;
        takeHitToIdle.exitTime = 1f;
        takeHitToIdle.duration = 0f;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("Animator Controller AC_Skeleton configured successfully!");
    }

    private static void AddParameterIfNotExists(AnimatorController controller, string name, AnimatorControllerParameterType type)
    {
        foreach (var param in controller.parameters)
        {
            if (param.name == name) return;
        }
        controller.AddParameter(name, type);
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
        return null;
    }

    private static void ClearTransitions(AnimatorState state)
    {
        state.transitions = new AnimatorStateTransition[0];
    }

    [MenuItem("Tools/Setup Skeleton Prefab")]
    public static void SetupPrefab()
    {
        string prefabPath = "Assets/Art/monsters_creature_fantasy/Skeleton/Skeleton.prefab";
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        if (prefabRoot == null)
        {
            Debug.LogError("Prefab not found at " + prefabPath);
            return;
        }

        // Add Rigidbody2D
        Rigidbody2D rb = prefabRoot.GetComponent<Rigidbody2D>();
        if (rb == null) rb = prefabRoot.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Add CapsuleCollider2D
        CapsuleCollider2D col = prefabRoot.GetComponent<CapsuleCollider2D>();
        if (col == null) col = prefabRoot.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.62f, 1.72f);
        col.offset = new Vector2(-0.01f, -0.23f);
        col.direction = CapsuleDirection2D.Vertical;

        // Add TouchingDirections
        TouchingDirections td = prefabRoot.GetComponent<TouchingDirections>();
        if (td == null) td = prefabRoot.AddComponent<TouchingDirections>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.useLayerMask = false;
        td.castFilter = filter;

        // Add EnemyController
        EnemyController ec = prefabRoot.GetComponent<EnemyController>();
        if (ec == null) ec = prefabRoot.AddComponent<EnemyController>();
        ec.walkSpeed = 2f;
        ec.chaseSpeed = 2f;
        ec.idleTime = 1.5f;
        ec.maxHealth = 30f;
        ec.detectRange = 5f;
        ec.attackRange = 1.2f;
        ec.attackCooldown = 1.5f;
        
        ec.groundLayer = 1; // Default layer
        ec.ledgeCheckDistance = 1f;
        ec.wallCheckDistance = 0.5f;

        // Create transforms for Ledge and Wall checks
        Transform ledgeCheck = prefabRoot.transform.Find("LedgeCheck");
        if (ledgeCheck == null)
        {
            GameObject checkObj = new GameObject("LedgeCheck");
            checkObj.transform.parent = prefabRoot.transform;
            checkObj.transform.localPosition = new Vector3(0.5f, -1.0f, 0f);
            ledgeCheck = checkObj.transform;
        }
        ec.ledgeCheckPoint = ledgeCheck;

        Transform wallCheck = prefabRoot.transform.Find("WallCheck");
        if (wallCheck == null)
        {
            GameObject checkObj = new GameObject("WallCheck");
            checkObj.transform.parent = prefabRoot.transform;
            checkObj.transform.localPosition = new Vector3(0.5f, 0f, 0f);
            wallCheck = checkObj.transform;
        }
        ec.wallCheckPoint = wallCheck;

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);
        Debug.Log("Skeleton Prefab configured successfully!");
    }
}
