using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Slider healthSlider;
    [Header("Damage Flash")]
    [Tooltip("Drag the DamageFlash UI Image here")]
    public Image damageImage;
    [Tooltip("The color the screen turns when hit (Red)")]
    public Color flashColor = new Color(1f, 0f, 0f, 0.8f);
    [Tooltip("How fast the red fades away")]
    public float flashSpeed = 5f;

    private int lastKnownHealth;
    // Start is called before the first frame update
    void Start()
    {
        if (damageImage != null)
        {
            damageImage.color = Color.clear;
        }

        lastKnownHealth = PlayerHealth.currentHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (healthText != null)
            healthText.text = "Health:" + PlayerHealth.currentHealth.ToString();
        if (PlayerHealth.currentHealth < lastKnownHealth)
        {
            if (damageImage != null)
            {
                damageImage.color = flashColor;
            }
        }

        lastKnownHealth = PlayerHealth.currentHealth;

        if (damageImage != null && damageImage.color.a > 0)
        {
            damageImage.color = Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
        }
    }
    

    
}
