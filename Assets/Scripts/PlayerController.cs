using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Rotation Settings")]
    [Tooltip("Adjust if your sprite doesn't face the mouse correctly. 90 is common for sprites facing UP.")]
    public float rotationOffset = 180f;

    private Rigidbody2D rb;
    private Camera mainCamera;
    private Vector2 movementInput;
    private Vector2 mousePosition;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        // Force correct Rigidbody settings for 2D top-down physics
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    // Update is called once per frame
    void Update()
    {
        // 1. Capture Input in Update (snappy movement with GetAxisRaw)
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");

        // 2. Capture Mouse Position
        mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
    }
    void FixedUpdate()
    {
        // 3. Apply Movement 
        // Using rb.velocity makes sliding against wall colliders buttery smooth
        rb.velocity = movementInput.normalized * moveSpeed;

        // 4. Apply Rotation
        Vector2 lookDirection = mousePosition - rb.position;
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - rotationOffset;

        // Apply the rotation directly to the Rigidbody
        rb.rotation = angle;
    }
}
