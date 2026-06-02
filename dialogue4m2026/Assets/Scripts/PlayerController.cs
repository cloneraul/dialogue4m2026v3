using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Force multiplier applied every FixedUpdate based on input.")]
    [SerializeField] private float moveSpeed = 10f;
    [Tooltip("Maximum horizontal speed. Set to 0 or negative to disable clamping.")]
    [SerializeField] private float maxSpeed = 8f;
    [Tooltip("When enabled, movement is relative to the assigned camera's forward/right.")]
    [SerializeField] private bool cameraRelativeMovement = true;
    [Tooltip("Optional camera transform. If null the script will try Camera.main at Awake().")]
    [SerializeField] private Transform cameraTransform = null;

    // internal storage for input
    private Vector2 moveInput = Vector2.zero;
    private Rigidbody rb;
    private PlayerInput playerInput;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        // Good defaults for a rolling ball
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void OnEnable()
    {
        playerInput.actions.FindAction("Move").performed += OnMove;
        playerInput.actions.FindAction("Move").canceled +=
            context => { moveInput = Vector2.zero; };
        playerInput.actions.FindAction("Interact").performed += OnInteract;
    }

    private void OnDisable()
    {
        playerInput.actions.FindAction("Move").performed -= OnMove;
        playerInput.actions.FindAction("Interact").performed -= OnInteract;
    }

    /// <summary>
    /// Callback expected to be wired from a PlayerInput (Behavior = Invoke Unity Events)
    /// or by using the old SendMessage style with the Input System. The action should
    /// be a Value type with Vector2 binding (e.g. WASD/left stick).
    /// </summary>
    /// <param name="value">InputValue provided by the Input System</param>
    public void OnMove(InputValue value)
    {
        //if (value == null) return;
        //moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    private void ApplyMovement()
    {
        if (rb == null) return;

        // Convert 2D input to world-space direction
        Vector3 desired = new Vector3(moveInput.x, 0f, moveInput.y);

        if (cameraRelativeMovement && cameraTransform != null)
        {
            // Project camera forward on the XZ plane and build a right vector
            Vector3 camForward = cameraTransform.forward;
            camForward.y = 0f;
            camForward.Normalize();
            Vector3 camRight = cameraTransform.right;
            camRight.y = 0f;
            camRight.Normalize();

            desired = camRight * moveInput.x + camForward * moveInput.y;
        }

        if (desired.sqrMagnitude > 1f)
            desired.Normalize();

        // Apply force. ForceMode.Force makes it frame-rate independent when used in FixedUpdate.
        Vector3 force = desired * moveSpeed;
        rb.AddForce(force, ForceMode.Force);

        // Optionally clamp horizontal speed while preserving vertical velocity (gravity/jumps)
        if (maxSpeed > 0f)
        {
            Vector3 horizontalVel = rb.linearVelocity;
            horizontalVel.y = 0f;
            float speed = horizontalVel.magnitude;
            if (speed > maxSpeed)
            {
                Vector3 limited = horizontalVel.normalized * maxSpeed;
                rb.linearVelocity = new Vector3(limited.x, rb.linearVelocity.y, limited.z);
            }
        }
    }

    // Public setters/getters for runtime tweaking
    public void SetMoveSpeed(float speed) => moveSpeed = speed;
    public void SetMaxSpeed(float max) => maxSpeed = max;
    public void SetCameraRelative(bool enabled) => cameraRelativeMovement = enabled;
    public void SetCameraTransform(Transform t) => cameraTransform = t;

    private void OnMove(InputAction.CallbackContext obj)
    {
        moveInput = obj.ReadValue<Vector2>();
    }
    
    private void OnInteract(InputAction.CallbackContext obj)
    { 
        InteractOM.Interact();
    }
}

