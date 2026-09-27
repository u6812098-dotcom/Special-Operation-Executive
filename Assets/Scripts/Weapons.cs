using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapons : MonoBehaviour
{
    [Header("Weapon Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;

    [Tooltip("Check this on enemy weapons. Leave UNCHECKED on the Player's AR/Pistol. This is how bullets know which side hurts.")]
    public bool firedByEnemy = false;

    [Header("Ammo System")]
    public bool isInfiniteAmmo = false; // Enemies and pistols will use true
    public int currentAmmo = 30;

    [Header("Visuals")]
    public GameObject muzzleFlash;
    public int muzzleFlashFrames = 10;

    private float nextFireTime = 0f;
    public AudioSource audioSource;
    public AudioClip shootSound;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Weapon script started!");

        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(false);
            Debug.Log("Muzzle flash was successfully hidden by the script.");
        }
        else
        {
            Debug.LogWarning("The Muzzle Flash slot is empty in the Inspector!");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    public bool Shoot()
    {
        
        if (firePoint == null || bulletPrefab == null)
        {
            Debug.LogError("ERROR: FirePoint or BulletPrefab is NULL!");
            return true;
        }

        if (Time.time >= nextFireTime)
        {
            // Return false to tell the controller this gun is empty
            if (!isInfiniteAmmo && currentAmmo <= 0) return false;

            nextFireTime = Time.time + fireRate;
            if (!isInfiniteAmmo) currentAmmo--;

            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // Tell the bullet who fired it, so it knows what it's allowed to hurt.
            BulletController bc = bulletObj.GetComponent<BulletController>();
            if (bc != null)
            {
                bc.firedByEnemy = firedByEnemy;
            }

            if (audioSource != null && shootSound != null)
            {
                audioSource.PlayOneShot(shootSound);
            }

            if (muzzleFlash != null)
            {
                StopAllCoroutines();
                StartCoroutine(ShowMuzzleFlash());
            }
            return true;
        }

        return true;
        //return true;
    }
    private IEnumerator ShowMuzzleFlash()
    {
        muzzleFlash.SetActive(true);

        // Stays on for exactly 0.05 seconds, regardless of your framerate
        yield return new WaitForSeconds(0.05f);

        muzzleFlash.SetActive(false);
    }
}