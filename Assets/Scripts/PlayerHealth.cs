using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
//using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 20;
    public static int currentHealth;

    [Header("Death Visuals")]
    [Tooltip("Drag the Player's SpriteRenderer here.")]
    public SpriteRenderer spriteRenderer;
    [Tooltip("Assign the lying-down death sprite from Player.png here.")]
    public Sprite deathSprite;

    [Header("UI Settings")]
    [Tooltip("Drag your Game Over UI Panel or Text GameObject here.")]
    public GameObject gameOverMessage;

    private bool isDead = false;

    [Header("Passive Regeneration")]
    [Tooltip("How much health is restored per tick.")]
    public int regenAmount = 1;
    [Tooltip("Seconds to wait before restoring health.")]
    public float regenInterval = 2f;

    private float regenTimer = 0f;
    // True once the player has died. EnemyAI and EnemySpawner check this
    // so they stop acting the moment the game ends.
    public static bool IsGameOver = false;

    public AudioSource audioSource;
    public AudioClip deadSound;

    public TextMeshProUGUI healthText;

    // Start is called before the first frame update
    void Start()
    {
        // Reset in case this is a fresh play session with domain reload disabled.
        IsGameOver = false;

        currentHealth = maxHealth;

        // Auto-get SpriteRenderer if not manually assigned
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (gameOverMessage != null)
        {
            gameOverMessage.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead || currentHealth >= maxHealth) return;

        regenTimer += Time.deltaTime;

        // When the timer reaches the interval, heal the player
        if (regenTimer >= regenInterval)
        {
            currentHealth += regenAmount;
            regenTimer = 0f; // Reset timer for the next tick

            // Cap health at maximum
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }

            //Debug.Log("Passively Healed! Current Health: " + currentHealth);
        }
    }
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        //Debug.Log("Player Hit! Current Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        isDead = true;
        IsGameOver = true;

        if (audioSource != null && deadSound != null)
        {
            audioSource.PlayOneShot(deadSound);
        }

        // Swap to the lying down death pose
        if (spriteRenderer != null && deathSprite != null)
        {
            spriteRenderer.sprite = deathSprite;
        }

        // Disable control and combat scripts
        PlayerController movement = GetComponent<PlayerController>();
        if (movement != null) movement.enabled = false;

        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null) combat.enabled = false;

        // Stop physics movement
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;

        // Display Game Ending Message
        if (gameOverMessage != null)
        {
            //gameOverMessage.SetActive(true);
        }

        //Debug.Log("Player has died. Game Over!");
        //SceneManager.LoadScene("Game Over");
        Invoke(nameof(EndGame), 1.5f);
    }
    private void EndGame() => SceneManager.LoadScene("Game Over");
}