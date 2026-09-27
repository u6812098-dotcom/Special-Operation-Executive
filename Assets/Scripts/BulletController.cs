using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class BulletController : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 2f;

    // Set automatically by Weapons.Shoot() right after this bullet spawns.
    [HideInInspector]
    public bool firedByEnemy = false;

    private Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.up * speed;
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Ignore other triggers(like camera bounds, enemy detection zones)
        if (hitInfo.isTrigger) return;
        if (hitInfo.gameObject.layer == LayerMask.NameToLayer("Furniture")) return;

        // NEW: Ignore the physical body of the entity that shot this bullet!
        if (firedByEnemy && hitInfo.CompareTag("Enemy")) return;
        if (!firedByEnemy && hitInfo.CompareTag("Player")) return;

        if (firedByEnemy)
        {
            // Enemy bullets only hurt the player, never other enemies.
            PlayerHealth player = hitInfo.GetComponentInParent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(1);
            }
        }
        else
        {
            // Player bullets only hurt enemies.
            EnemyAI enemy = hitInfo.GetComponentInParent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(1);
            }
        }

        // Destroy the bullet when it hits a valid target or wall
        Destroy(gameObject);
    }
}