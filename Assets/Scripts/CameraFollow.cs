using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    // Z is set to -10 by default so the camera sits behind the 2D scene
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Camera Feeling")]
    [Range(0f, 1f)]
    public float smoothTime = 0.15f; // Lower is snappier, higher is "floatier"

    private Vector3 velocity = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate()
    {
        // Don't do anything if the target hasn't been assigned
        if (target == null) return;

        // Calculate where the camera should be
        Vector3 targetPosition = target.position + offset;

        // Smoothly glide the camera to the target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
