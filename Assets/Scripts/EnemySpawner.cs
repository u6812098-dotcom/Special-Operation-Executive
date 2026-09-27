using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("What to spawn")]
    [Tooltip("Drag one or more enemy prefabs here. A random one is picked for each enemy spawned.")]
    public GameObject[] enemyPrefabs;

    [Header("Player Reference")]
    [Tooltip("Leave empty to auto-find the GameObject tagged 'Player'.")]
    public Transform player;

    [Header("Where to spawn")]
    [Tooltip("Bottom-left corner of the area enemies can spawn in.")]
    public Vector2 spawnAreaMin;
    [Tooltip("Top-right corner of the area enemies can spawn in.")]
    public Vector2 spawnAreaMax;
    [Tooltip("Enemies won't spawn closer to the player than this.")]
    public float minDistanceFromPlayer = 6f;
    [Tooltip("Enemies won't spawn farther from the player than this.")]
    public float maxDistanceFromPlayer = 14f;
    [Tooltip("Same obstacle layers used by EnemyAI (walls), so enemies don't spawn inside a wall.")]
    public LayerMask obstacleMask;
    [Tooltip("The layer your floor/ground tiles are on. A spawn point must land ON this layer to count as valid, which keeps spawns inside the actual map instead of empty space outside it.")]
    public LayerMask groundMask;
    [Tooltip("Roughly match the enemy's body radius, used to check the spot is clear.")]
    public float checkRadius = 0.4f;
    [Tooltip("How many random spots to try before giving up on placing one enemy.")]
    public int maxPlacementAttempts = 20;

    [System.Serializable]
    public class SpawnTier
    {
        [Tooltip("This tier becomes active once the survival timer passes this many seconds.")]
        public float startTimeSeconds;
        [Tooltip("How many enemies spawn at once, each time this tier's interval elapses.")]
        public int enemiesPerBurst;
        [Tooltip("Seconds between each burst while this tier is active.")]
        public float interval;
    }

    [Header("Difficulty Ramp")]
    [Tooltip("Ordered from easiest to hardest. The spawner uses the last tier whose start time has passed.")]
    public SpawnTier[] tiers = new SpawnTier[]
    {
        new SpawnTier { startTimeSeconds = 0f,   enemiesPerBurst = 1, interval = 2f  },  // 0:00 - 1 enemy every 2s
        new SpawnTier { startTimeSeconds = 60f,  enemiesPerBurst = 2, interval = 3f  },  // 1:00 - 2 enemies every 3s
        new SpawnTier { startTimeSeconds = 120f, enemiesPerBurst = 3, interval = 6f  },  // 2:00 - 3 enemies every 6s
        new SpawnTier { startTimeSeconds = 180f, enemiesPerBurst = 4, interval = 10f },  // 3:00 - 4 enemies every 10s (cap)
    };

    private float survivalTimer = 0f;
    private float burstTimer = 0f;

    void Start()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }
    }

    void Update()
    {
        // Stop spawning entirely once the player has died.
        if (PlayerHealth.IsGameOver) return;

        survivalTimer += Time.deltaTime;

        SpawnTier currentTier = GetCurrentTier();
        if (currentTier == null || enemyPrefabs.Length == 0 || player == null) return;

        burstTimer += Time.deltaTime;
        if (burstTimer >= currentTier.interval)
        {
            burstTimer = 0f;
            SpawnBurst(currentTier.enemiesPerBurst);
        }
    }

    private SpawnTier GetCurrentTier()
    {
        SpawnTier result = null;
        foreach (SpawnTier tier in tiers)
        {
            if (survivalTimer >= tier.startTimeSeconds)
            {
                // Tiers are checked in order, so the last one that qualifies is the current one.
                result = tier;
            }
        }
        return result;
    }

    private void SpawnBurst(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (!TryGetRandomSpawnPosition(out Vector2 spawnPos)) continue;

            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Instantiate(prefab, spawnPos, Quaternion.identity);
        }
    }

    private bool TryGetRandomSpawnPosition(out Vector2 result)
    {
        for (int i = 0; i < maxPlacementAttempts; i++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );

            float distToPlayer = Vector2.Distance(candidate, player.position);
            if (distToPlayer < minDistanceFromPlayer || distToPlayer > maxDistanceFromPlayer) continue;

            // Must NOT be inside a wall...
            bool blocked = Physics2D.OverlapCircle(candidate, checkRadius, obstacleMask);
            // ...and MUST be sitting on actual ground (this is what keeps
            // spawns inside the map instead of in empty space outside it).
            bool onGround = Physics2D.OverlapCircle(candidate, checkRadius, groundMask);

            if (!blocked && onGround)
            {
                result = candidate;
                return true;
            }
        }

        // Couldn't find a valid spot this attempt; caller just skips this one enemy.
        result = Vector2.zero;
        return false;
    }

    // Lets you see the spawn area, and the player's safe-zone ring, in the Scene view.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((spawnAreaMin.x + spawnAreaMax.x) / 2f, (spawnAreaMin.y + spawnAreaMax.y) / 2f, 0f);
        Vector3 size = new Vector3(spawnAreaMax.x - spawnAreaMin.x, spawnAreaMax.y - spawnAreaMin.y, 0f);
        Gizmos.DrawWireCube(center, size);

        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, minDistanceFromPlayer);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, maxDistanceFromPlayer);
        }
    }
}