using UnityEngine;

/// <summary>
/// Default positions used by the editor baker (Escape Protocol / Bake Level Content).
/// After baking, edit objects under the LevelContent object in each scene.
/// </summary>
public static class LevelContentLayouts
{
    public struct EnemySpawn
    {
        public Vector2 position;
        public bool heavy;
    }

    public struct CollectibleSpawn
    {
        public Vector2 position;
        public int points;
    }

    public struct LaserSpawn
    {
        public Vector2 position;
        public bool horizontal;
        public float length;
        public float interval;
        public float offset;
    }

    public struct LevelLayout
    {
        public EnemySpawn[] enemies;
        public CollectibleSpawn[] collectibles;
        public LaserSpawn[] lasers;
    }

    public static bool TryGet(string sceneName, out LevelLayout layout)
    {
        switch (sceneName)
        {
            case "Level1":
                layout = Level1;
                return true;
            case "Level2":
                layout = Level2;
                return true;
            case "Level3":
                layout = Level3;
                return true;
            case "BeginningDungeon":
                layout = BeginningDungeon;
                return true;
            default:
                layout = default;
                return false;
        }
    }

    public static readonly LevelLayout Level1 = new LevelLayout
    {
        enemies = new[]
        {
            new EnemySpawn { position = new Vector2(4f, -2f) },
            new EnemySpawn { position = new Vector2(8f, -6f) },
            new EnemySpawn { position = new Vector2(11f, -10f) },
            new EnemySpawn { position = new Vector2(2f, -8f) },
            new EnemySpawn { position = new Vector2(6f, -12f) },
            new EnemySpawn { position = new Vector2(5f, -4f), heavy = true },
            new EnemySpawn { position = new Vector2(10f, -14f), heavy = true },
        },
        collectibles = new[]
        {
            new CollectibleSpawn { position = new Vector2(2f, 0.5f), points = 75 },
            new CollectibleSpawn { position = new Vector2(-1f, -5f), points = 100 },
            new CollectibleSpawn { position = new Vector2(7f, -9f), points = 75 },
            new CollectibleSpawn { position = new Vector2(12f, -6f), points = 125 },
            new CollectibleSpawn { position = new Vector2(3f, -11f), points = 50 },
        },
        lasers = new[]
        {
            new LaserSpawn { position = new Vector2(3f, -3.5f), horizontal = true, length = 4.5f, interval = 1.5f },
            new LaserSpawn { position = new Vector2(7f, -7.5f), horizontal = true, length = 4f, interval = 1.7f, offset = 0.4f },
            new LaserSpawn { position = new Vector2(1f, -9.5f), horizontal = true, length = 3.5f, interval = 1.6f, offset = 0.8f },
        },
    };

    public static readonly LevelLayout Level2 = new LevelLayout
    {
        enemies = new[]
        {
            new EnemySpawn { position = new Vector2(6f, -4f) },
            new EnemySpawn { position = new Vector2(12f, -8f) },
            new EnemySpawn { position = new Vector2(16f, -14f) },
            new EnemySpawn { position = new Vector2(2f, -10f) },
            new EnemySpawn { position = new Vector2(9f, -18f) },
            new EnemySpawn { position = new Vector2(4f, -12f), heavy = true },
            new EnemySpawn { position = new Vector2(14f, -6f), heavy = true },
        },
        collectibles = new[]
        {
            new CollectibleSpawn { position = new Vector2(3f, -2f), points = 75 },
            new CollectibleSpawn { position = new Vector2(8f, -6f), points = 100 },
            new CollectibleSpawn { position = new Vector2(13f, -11f), points = 125 },
            new CollectibleSpawn { position = new Vector2(18f, -16f), points = 100 },
            new CollectibleSpawn { position = new Vector2(0f, -14f), points = 75 },
        },
        lasers = new[]
        {
            new LaserSpawn { position = new Vector2(5f, -5f), horizontal = true, length = 5f, interval = 1.5f },
            new LaserSpawn { position = new Vector2(10f, -9f), horizontal = true, length = 4.5f, interval = 1.6f, offset = 0.5f },
            new LaserSpawn { position = new Vector2(15f, -13f), horizontal = true, length = 4f, interval = 1.8f, offset = 0.9f },
            new LaserSpawn { position = new Vector2(7f, -15f), horizontal = true, length = 4f, interval = 1.5f, offset = 1.2f },
        },
    };

    public static readonly LevelLayout Level3 = new LevelLayout
    {
        enemies = new[]
        {
            new EnemySpawn { position = new Vector2(4f, -3f) },
            new EnemySpawn { position = new Vector2(9f, -7f) },
            new EnemySpawn { position = new Vector2(14f, -11f) },
            new EnemySpawn { position = new Vector2(1f, -9f) },
            new EnemySpawn { position = new Vector2(11f, -15f) },
            new EnemySpawn { position = new Vector2(6f, -5f), heavy = true },
            new EnemySpawn { position = new Vector2(12f, -13f), heavy = true },
            new EnemySpawn { position = new Vector2(3f, -14f), heavy = true },
        },
        collectibles = new[]
        {
            new CollectibleSpawn { position = new Vector2(2f, -1f), points = 100 },
            new CollectibleSpawn { position = new Vector2(7f, -5f), points = 75 },
            new CollectibleSpawn { position = new Vector2(12f, -9f), points = 125 },
            new CollectibleSpawn { position = new Vector2(5f, -12f), points = 75 },
            new CollectibleSpawn { position = new Vector2(15f, -7f), points = 100 },
            new CollectibleSpawn { position = new Vector2(8f, -16f), points = 150 },
        },
        lasers = new[]
        {
            new LaserSpawn { position = new Vector2(4f, -4.5f), horizontal = true, length = 5f, interval = 1.5f },
            new LaserSpawn { position = new Vector2(9f, -8.5f), horizontal = true, length = 4.5f, interval = 1.6f, offset = 0.5f },
            new LaserSpawn { position = new Vector2(13f, -12.5f), horizontal = true, length = 4f, interval = 1.7f, offset = 1f },
            new LaserSpawn { position = new Vector2(6f, -10.5f), horizontal = true, length = 3.5f, interval = 1.4f, offset = 1.3f },
        },
    };

    public static readonly LevelLayout BeginningDungeon = new LevelLayout
    {
        enemies = new[]
        {
            new EnemySpawn { position = new Vector2(3f, -2f) },
            new EnemySpawn { position = new Vector2(-2f, -5f) },
            new EnemySpawn { position = new Vector2(1f, -4f), heavy = true },
        },
        collectibles = new[]
        {
            new CollectibleSpawn { position = new Vector2(0f, -1f), points = 50 },
            new CollectibleSpawn { position = new Vector2(4f, -6f), points = 75 },
        },
        lasers = new[]
        {
            new LaserSpawn { position = new Vector2(2f, -3.5f), horizontal = true, length = 3.5f, interval = 1.5f },
        },
    };
}
