using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Weapons weapon; // Drag this enemy's Weapons component here

    [Header("Health")]
    [Tooltip("Set this to 1 so any hit destroys the enemy.")]
    public int health = 1;

    [Header("Death")]
    [Tooltip("Sprite shown when the enemy dies (e.g. the lying-down frame from Enemy.png).")]
    public Sprite deathSprite;
    [Tooltip("Prefab with a SpriteRenderer showing a blood splash frame from Blood.png.")]
    public GameObject bloodSplashPrefab;
    [Tooltip("Offset from the enemy's position where blood spawns, e.g. (0, 0.3) to appear on the body instead of at the feet.")]
    public Vector2 bloodSpawnOffset = new Vector2(0f, 0.3f);
    [Tooltip("How long the dead body stays visible before it's removed.")]
    public float despawnDelay = 2f;

    [Header("Ammo Drop")]
    [Tooltip("Prefab with an AmmoPickup component, spawned on death sometimes.")]
    public GameObject ammoPickupPrefab;
    [Range(0f, 1f)]
    [Tooltip("Chance (0 = never, 1 = always) that this enemy drops ammo when it dies.")]
    public float ammoDropChance = 1f;

    private SpriteRenderer spriteRenderer;
    private Collider2D[] cols; // this enemy usually has TWO: the solid hitbox + the detection trigger
    private bool isDead = false;

    [Header("Aiming")]
    [Tooltip("Adjust if the enemy sprite doesn't face the player correctly. Match your Player's rotationOffset value.")]
    public float rotationOffset = 180f;
    public float rotationSpeed = 10f;

    [Header("Detection")]
    [Tooltip("Layers that block line-of-sight, e.g. Walls/Ground. Set this in the Inspector.")]
    public LayerMask obstacleMask;

    [Header("Movement")]
    public float moveSpeed = 3f;
    [Tooltip("The enemy stops closing distance once this close to the player, and holds position to shoot.")]
    public float preferredRange = 4f;
    [Tooltip("Small buffer so the enemy doesn't jitter in and out of preferredRange.")]
    public float rangeBuffer = 0.3f;

    [Header("Separation (prevents enemies stacking)")]
    [Tooltip("Layer(s) your enemies are on. Used to find nearby enemies to push apart from.")]
    public LayerMask enemyMask;
    [Tooltip("How close another enemy needs to be before we start pushing away from it.")]
    public float separationRadius = 1.25f;
    [Tooltip("How strongly enemies push apart relative to the seek-the-player movement. 1 = equal weight.")]
    public float separationStrength = 1.5f;

    [Header("Collision Safety")]
    [Tooltip("Radius used when checking ahead for walls before moving, so separation pushes can't shove an enemy through geometry. Roughly match your collider radius.")]
    public float bodyRadius = 0.4f;
    [Tooltip("Small skin width kept between the enemy and a wall it's clamped against.")]
    public float wallSkin = 0.05f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip deadSound;
    public AudioClip hitSound;

    private Transform target;
    private Rigidbody2D rb;
    private bool playerInRange = false;   // inside the detection radius trigger
    private bool canSeePlayer = false;    // inside radius AND no wall in between

    // Reused buffer so FixedUpdate doesn't allocate every frame.
    private static readonly Collider2D[] neighborBuffer = new Collider2D[16];
    [Header("Random Movement")]
    [Tooltip("How strongly the enemy wanders. 0 = no random movement.")]
    public float wanderStrength = 0.8f;
    [Tooltip("How often (in seconds) the enemy picks a new random direction.")]
    public float wanderInterval = 1.5f;

    private Vector2 wanderDirection;
    private float nextWanderTime;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        cols = GetComponents<Collider2D>();
    }

    void Update()
    {
        // Stop reacting entirely once the enemy is dead OR the player is.
        if (isDead || PlayerHealth.IsGameOver) return;

        // Re-check line of sight every frame, not just on trigger enter/exit,
        // so the enemy loses the player the moment a wall gets between them.
        if (playerInRange && target != null)
        {
            canSeePlayer = HasLineOfSight();
        }
        else
        {
            canSeePlayer = false;
        }
        if (canSeePlayer)
        {
            // Aim toward the player and shoot
            Vector2 direction = target.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - rotationOffset;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), rotationSpeed * Time.deltaTime);

            weapon.Shoot();
        }
        else if (wanderDirection.sqrMagnitude > 0.1f)
        {
            // If they can't see the player, look in the direction they are randomly wandering
            float angle = Mathf.Atan2(wanderDirection.y, wanderDirection.x) * Mathf.Rad2Deg - rotationOffset;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), rotationSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
       
        if (isDead || PlayerHealth.IsGameOver) return;

        // 1. Pick a new random direction every few seconds
        if (Time.time >= nextWanderTime)
        {
            wanderDirection = Random.insideUnitCircle.normalized;
            // Add a little randomness to the timer so enemies don't all change direction at the exact same frame
            nextWanderTime = Time.time + Random.Range(wanderInterval * 0.8f, wanderInterval * 1.2f);
        }

        Vector2 seek = Vector2.zero;
        bool wantsToSeek = false;

        if (canSeePlayer && rb != null && target != null)
        {
            Vector2 toPlayer = (Vector2)target.position - rb.position;

            if (toPlayer.magnitude > preferredRange + rangeBuffer)
            {
                seek = toPlayer.normalized;
                wantsToSeek = true;
            }
        }

        Vector2 separation = GetSeparationVector();
        bool wantsToSeparate = separation.sqrMagnitude > 0.0001f;

        // 2. Blend Seeking, Separation, and Wandering together
        Vector2 moveDir = seek
                        + separation.normalized * separationStrength * (wantsToSeparate ? 1f : 0f)
                        + wanderDirection * wanderStrength;

        if (moveDir.sqrMagnitude < 0.0001f) return;
        moveDir.Normalize();

        float step = moveSpeed * Time.fixedDeltaTime;
        Vector2 safeMove = ClampMoveAgainstWalls(rb.position, moveDir, step);

        rb.MovePosition(rb.position + safeMove);
    }

    /// <summary>
    /// Sums push-away vectors from nearby enemies so this enemy doesn't
    /// stack on top of others converging on the same target.
    /// </summary>
    private Vector2 GetSeparationVector()
    {
        Vector2 push = Vector2.zero;

        int count = Physics2D.OverlapCircleNonAlloc(rb.position, separationRadius, neighborBuffer, enemyMask);
        for (int i = 0; i < count; i++)
        {
            Collider2D other = neighborBuffer[i];
            if (other == null || other.attachedRigidbody == rb) continue; // skip self

            Vector2 away = rb.position - (Vector2)other.transform.position;
            float dist = away.magnitude;
            if (dist < 0.0001f)
            {
                // Exactly overlapping (spawned on top of each other) — nudge
                // in a stable but arbitrary direction so it's not zero.
                away = new Vector2(Mathf.Epsilon, 0f);
                dist = Mathf.Epsilon;
            }

            // Closer enemies push harder (inverse distance weighting).
            push += away.normalized * (separationRadius - dist) / separationRadius;
        }

        return push;
    }

    /// <summary>
    /// Casts a circle ahead of the intended move so a wall always stops the
    /// enemy, even when separation and seek are combined into one big step
    /// that would otherwise tunnel past thin colliders.
    /// </summary>
    private Vector2 ClampMoveAgainstWalls(Vector2 currentPos, Vector2 moveDir, float step)
    {
        RaycastHit2D hit = Physics2D.CircleCast(currentPos, bodyRadius, moveDir, step, obstacleMask);
        if (hit.collider == null)
        {
            return moveDir * step;
        }

        // Stop just short of the wall instead of passing through it.
        float safeDistance = Mathf.Max(0f, hit.distance - wallSkin);
        return moveDir * safeDistance;
    }

    /// <summary>
    /// Call this from your bullet script when it hits this enemy,
    /// e.g. GetComponent&lt;EnemyAI&gt;().TakeDamage(1);
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        SpawnBloodSplash();

        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    private void SpawnBloodSplash()
    {
        if (bloodSplashPrefab == null) return;

        Vector3 spawnPos = transform.position + (Vector3)bloodSpawnOffset;
        Instantiate(bloodSplashPrefab, spawnPos, Quaternion.identity);
    }

    private void Die()
    {
        isDead = true;
        if (audioSource != null && deadSound != null)
        {
            audioSource.PlayOneShot(deadSound);
            audioSource.PlayOneShot(hitSound);
        }
        ScoreManager.currentScore += 1;

        // Stop all movement immediately.
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        // Turn off EVERY collider on this object (the solid body hitbox
        // AND the detection trigger) so the corpse can't be pushed by the
        // player, can't block bullets, and won't trigger separation checks
        // from other enemies.
        if (cols != null)
        {
            foreach (Collider2D c in cols)
            {
                c.enabled = false;
            }
        }

        // Swap to the death pose.
        if (spriteRenderer != null && deathSprite != null)
        {
            spriteRenderer.sprite = deathSprite;
        }

        // Random chance to drop ammo for the player to pick up.
        if (ammoPickupPrefab != null && Random.value <= ammoDropChance)
        {
            Instantiate(ammoPickupPrefab, transform.position, Quaternion.identity);
        }
        this.enabled = false;

        // Remove the body after a short delay so it doesn't disappear instantly.
        Destroy(gameObject, despawnDelay);
    }

    private bool HasLineOfSight()
    {
        // Linecast returns true if it HITS something on obstacleMask between the two points.
        // So "can see" means the linecast does NOT hit a wall.
        RaycastHit2D hit = Physics2D.Linecast(transform.position, target.position, obstacleMask);
        return hit.collider == null;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            target = other.transform;
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            canSeePlayer = false;
            target = null;
        }
    }

    // Optional: visualize the line-of-sight check in the Scene view while selected
    void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Gizmos.color = canSeePlayer ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, target.position);
    }
}