using UnityEngine;

// Attach this to your ammo pickup prefab. Its collider must be set to
// "Is Trigger" so OnTriggerEnter2D fires when the player walks over it.
public class AmmoPickup : MonoBehaviour
{
    [Tooltip("How much ammo this pickup gives the player's assault rifle.")]
    public int ammoAmount = 15;
    [Header("Audio")]
    public AudioClip pickupSound;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Look on the player object itself, or its parent, in case the
        // collider that touched us is on a child object.
        PlayerCombat player = other.GetComponent<PlayerCombat>();
        if (player == null)
        {
            player = other.GetComponentInParent<PlayerCombat>();
        }
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, Camera.main.transform.position, 4f);
        }

        if (player != null && player.assaultRifle != null)
        {
            player.assaultRifle.currentAmmo += ammoAmount;
            Destroy(gameObject);
        }
    }
}
