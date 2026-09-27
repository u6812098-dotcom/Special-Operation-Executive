using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BloodSplash : MonoBehaviour
{
    [Tooltip("The sliced blood frames from Blood.png, in play order.")]
    public Sprite[] frames;

    [Tooltip("How long each frame stays on screen.")]
    public float frameTime = 0.05f;

    [Tooltip("How long the final frame lingers before the splash disappears.")]
    public float holdTime = 1.5f;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // Draw above the floor/ground tiles instead of underneath them.
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 5;

        if (frames != null && frames.Length > 0)
        {
            StartCoroutine(Animate());
        }
        else
        {
            Destroy(gameObject, holdTime);
        }
    }

    private System.Collections.IEnumerator Animate()
    {
        foreach (Sprite frame in frames)
        {
            sr.sprite = frame;
            yield return new WaitForSeconds(frameTime);
        }

        yield return new WaitForSeconds(holdTime);
        Destroy(gameObject);
    }
}
