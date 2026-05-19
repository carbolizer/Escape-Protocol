#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class LevelContentSceneBaker
{
    private const string RootName = "LevelContent";
    private const string EnemyPath = "Assets/Prefabs/Enemy.prefab";
    private const string HeavyEnemyPath = "Assets/Prefabs/HeavyEnemy.prefab";
    private const string CollectiblePath = "Assets/Prefabs/ScoreCollectible.prefab";
    private const string LaserPath = "Assets/Prefabs/LaserDoor.prefab";
    private const string CollectibleTilePath = "Assets/Tiles/Dungeon_Tileset_86.asset";

    private static readonly string[] GameplayScenes =
        { "Level1", "Level2", "Level3", "BeginningDungeon" };

    [MenuItem("Escape Protocol/Bake Level Content Into Open Scene")]
    public static void BakeOpenScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!LevelContentLayouts.TryGet(scene.name, out LevelContentLayouts.LevelLayout layout))
        {
            EditorUtility.DisplayDialog(
                "Bake Level Content",
                $"Scene \"{scene.name}\" has no default layout. Open Level1, Level2, Level3, or BeginningDungeon.",
                "OK");
            return;
        }

        BakeScene(scene, layout);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"Baked level content into {scene.name}. Edit objects under \"{RootName}\" in the Hierarchy.");
    }

    [MenuItem("Escape Protocol/Bake Level Content Into All Gameplay Scenes")]
    public static void BakeAllGameplayScenes()
    {
        string activePath = SceneManager.GetActiveScene().path;
        int baked = 0;

        foreach (string sceneName in GameplayScenes)
        {
            string path = $"Assets/Scenes/{sceneName}.unity";
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            if (!LevelContentLayouts.TryGet(scene.name, out LevelContentLayouts.LevelLayout layout))
                continue;

            BakeScene(scene, layout);
            EditorSceneManager.SaveScene(scene);
            baked++;
        }

        if (!string.IsNullOrEmpty(activePath))
            EditorSceneManager.OpenScene(activePath, OpenSceneMode.Single);

        Debug.Log($"Baked level content into {baked} gameplay scene(s).");
    }

    [MenuItem("Escape Protocol/Remove Runtime Spawned Level Content")]
    public static void RemoveRuntimeSpawnedRoot()
    {
        GameObject spawned = GameObject.Find("SpawnedLevelContent");
        if (spawned == null)
        {
            Debug.Log("No SpawnedLevelContent object in the open scene.");
            return;
        }

        Undo.DestroyObjectImmediate(spawned);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private static void BakeScene(Scene scene, LevelContentLayouts.LevelLayout layout)
    {
        EnsurePrefabsExist();

        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPath);
        GameObject heavyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HeavyEnemyPath);
        GameObject collectiblePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CollectiblePath);
        GameObject laserPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LaserPath);

        if (enemyPrefab == null || collectiblePrefab == null || laserPrefab == null)
        {
            Debug.LogError("Level content prefabs are missing. Reimport the project or run the baker again.");
            return;
        }

        if (heavyPrefab == null)
            heavyPrefab = enemyPrefab;

        Transform root = GetOrCreateRoot();

        ClearChildren(root);

        int enemyIndex = 0;
        foreach (LevelContentLayouts.EnemySpawn spawn in layout.enemies)
        {
            GameObject prefab = spawn.heavy ? heavyPrefab : enemyPrefab;
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root);
            instance.transform.position = spawn.position;
            instance.name = spawn.heavy ? $"HeavyEnemy_{enemyIndex++}" : $"Enemy_{enemyIndex++}";
        }

        int collectibleIndex = 0;
        foreach (LevelContentLayouts.CollectibleSpawn spawn in layout.collectibles)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(collectiblePrefab, root);
            instance.transform.position = spawn.position;
            instance.name = $"ScoreCollectible_{collectibleIndex++}";
            EnsureCollectibleVisual(instance);

            ScoreCollectible pickup = instance.GetComponent<ScoreCollectible>();
            if (pickup != null)
                pickup.pointValue = spawn.points;
        }

        int laserIndex = 0;
        foreach (LevelContentLayouts.LaserSpawn spawn in layout.lasers)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(laserPrefab, root);
            instance.transform.position = spawn.position;
            instance.name = spawn.horizontal ? $"LaserDoor_H_{laserIndex++}" : $"LaserDoor_V_{laserIndex++}";

            LaserDoor laser = instance.GetComponent<LaserDoor>();
            if (laser == null) continue;

            laser.horizontal = spawn.horizontal;
            laser.beamLength = spawn.length;
            laser.beamThickness = 0.35f;
            laser.toggleInterval = spawn.interval;
            laser.startOffset = spawn.offset;
        }
    }

    private static Transform GetOrCreateRoot()
    {
        GameObject existing = GameObject.Find(RootName);
        if (existing != null)
            return existing.transform;

        GameObject root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Create LevelContent root");
        return root.transform;
    }

    private static void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(root.GetChild(i).gameObject);
    }

    private static void EnsurePrefabsExist()
    {
        if (!AssetDatabase.LoadAssetAtPath<GameObject>(CollectiblePath))
            CreateCollectiblePrefab();

        if (!AssetDatabase.LoadAssetAtPath<GameObject>(LaserPath))
            CreateLaserPrefab();

        if (!AssetDatabase.LoadAssetAtPath<GameObject>(HeavyEnemyPath))
            CreateHeavyEnemyPrefab();
    }

    private static void CreateCollectiblePrefab()
    {
        GameObject go = new GameObject("ScoreCollectible");
        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;
        ScoreCollectible pickup = go.AddComponent<ScoreCollectible>();
        SerializedObject so = new SerializedObject(pickup);
        so.FindProperty("collectibleSpriteAsset").objectReferenceValue = LoadCollectibleSprite();
        so.ApplyModifiedPropertiesWithoutUndo();
        EnsureCollectibleVisual(go);
        PrefabUtility.SaveAsPrefabAsset(go, CollectiblePath);
        Object.DestroyImmediate(go);
    }

    private static void EnsureCollectibleVisual(GameObject collectible)
    {
        Transform existing = collectible.transform.Find("CollectibleVisual");
        GameObject visual = existing != null ? existing.gameObject : new GameObject("CollectibleVisual");
        if (existing == null)
            visual.transform.SetParent(collectible.transform, false);

        SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = visual.AddComponent<SpriteRenderer>();

        Sprite sprite = LoadCollectibleSprite();
        if (sprite != null)
            renderer.sprite = sprite;

        renderer.color = Color.white;
        renderer.sortingLayerName = "Default";
        renderer.sortingOrder = 20;
    }

    private static Sprite LoadCollectibleSprite()
    {
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(CollectibleTilePath);
        return tile != null ? tile.sprite : null;
    }

    private static void CreateLaserPrefab()
    {
        GameObject go = new GameObject("LaserDoor");
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        LaserDoor laser = go.AddComponent<LaserDoor>();
        laser.horizontal = true;
        laser.beamLength = 4f;
        laser.beamThickness = 0.35f;
        laser.toggleInterval = 1.5f;
        PrefabUtility.SaveAsPrefabAsset(go, LaserPath);
        Object.DestroyImmediate(go);
    }

    private static void CreateHeavyEnemyPrefab()
    {
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPath);
        if (source == null)
        {
            Debug.LogError($"Cannot create HeavyEnemy prefab: missing {EnemyPath}");
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
        instance.name = "HeavyEnemy";

        EnemyHealth health = instance.GetComponent<EnemyHealth>();
        if (health == null)
            health = instance.AddComponent<EnemyHealth>();
        health.isHeavy = true;
        health.maxHealth = 3;

        EnemyPoints points = instance.GetComponent<EnemyPoints>();
        if (points != null)
        {
            points.pointValue = 120;
            points.stealthKillValue = 280;
        }

        SpriteRenderer sprite = instance.GetComponent<SpriteRenderer>();
        if (sprite != null)
            sprite.color = new Color(0.95f, 0.35f, 0.45f, 1f);

        instance.transform.localScale *= 1.12f;

        PrefabUtility.SaveAsPrefabAsset(instance, HeavyEnemyPath);
        Object.DestroyImmediate(instance);
    }
}
#endif
