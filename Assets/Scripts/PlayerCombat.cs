using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Arsenal")]
    public Weapons assaultRifle;
    public Weapons pistol;
    private Weapons activeWeapon;

    [Header("Visual States")]
    public SpriteRenderer spriteRenderer;
    public Sprite arSprite;     // Assign AR state from Player.png
    public Sprite pistolSprite; // Assign Pistol state from Player.png

    public TextMeshProUGUI ammoText;
    // Start is called before the first frame update
    void Start()
    {
        // Start game with the AR equipped
        EquipWeapon(assaultRifle, arSprite);
    }

    // Update is called once per frame
    void Update()
    {
        if (ammoText != null && activeWeapon != null)
        {
            if (activeWeapon.isInfiniteAmmo)
            {
                ammoText.text = "Ammo : Infinite";
            }
            else
            {
                // Reads currentAmmo from Weapons.cs[cite: 4]
                ammoText.text = "Ammo : " + activeWeapon.currentAmmo; 
            }
        }
        if (Input.GetMouseButton(0) && activeWeapon != null)
        {
            bool hasAmmo = activeWeapon.Shoot();

            // Transition from AR to Pistol when ammo depletes
            if (!hasAmmo && activeWeapon == assaultRifle)
            {
                EquipWeapon(pistol, pistolSprite);
            }
        }
    }
    private void EquipWeapon(Weapons gun, Sprite newSprite)
    {
        activeWeapon = gun;

        // Update the player's visual state
        if (spriteRenderer != null && newSprite != null)
        {
            spriteRenderer.sprite = newSprite;
        }
    }
}
