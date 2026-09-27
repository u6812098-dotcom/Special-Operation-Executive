using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ammoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoText;

    [Tooltip("Drag the GameObject holding the Player's Weapon script here")]
    [SerializeField] private Weapons playerWeapon;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (ammoText != null && playerWeapon != null)
        {
            // Check if the weapon has infinite ammo (like a base pistol)
            if (playerWeapon.isInfiniteAmmo)
            {
                ammoText.text = "Ammo: ∞";
            }
            else
            {
                ammoText.text = "Ammo: " + playerWeapon.currentAmmo.ToString();
            }
        }
    }
}
