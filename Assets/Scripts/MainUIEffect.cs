using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUIEffect : MonoBehaviour
{
    [Tooltip("Drag your UI Image or TextMeshPro text here.")]
    public Graphic uiElement;

    [Tooltip("How fast the colors change (in seconds).")]
    public float flashSpeed = 0.1f;

    [Tooltip("The colors it will cycle through.")]
    public Color[] discoColors = { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan };
    // Start is called before the first frame update
    void Start()
    {
        // If you forgot to assign it, try to grab the component on this object
        if (uiElement == null)
        {
            uiElement = GetComponent<Graphic>();
        }

        if (uiElement != null)
        {
            StartCoroutine(DiscoRoutine());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private IEnumerator DiscoRoutine()
    {
        float originalAlpha = uiElement.color.a;

        while (true)
        {
            // Pick a random color from your list
            Color randomColor = discoColors[Random.Range(0, discoColors.Length)];

            // Force the random color to use the original alpha
            randomColor.a = originalAlpha;

            // Apply it to the UI
            uiElement.color = randomColor;

            yield return new WaitForSeconds(flashSpeed);
        }
    }
}
