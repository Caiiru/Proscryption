using UnityEngine;
using UnityEngine.InputSystem;

public class EditorCameraController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float minSpeed = 1f;
    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private float scrollSpeedStep = 3f;
    [SerializeField] private float fastMultiplier = 3f;

    [Header("Smoothing")]
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float deceleration = 10f;

    [Header("Look")]
    [SerializeField] private float lookSensitivity = 0.2f;
    [SerializeField] private float maxLookAngle = 89f;
    [SerializeField] private bool invertY = false;

    [Header("Pan")]
    [SerializeField] private float panSpeed = 0.015f;

    private float yaw;
    private float pitch;
    private Vector3 currentVelocity = Vector3.zero;

    private Keyboard keyboard;
    private Mouse mouse;

    private void Start()
    {
        Vector3 euler = transform.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;

        if (pitch > 180f)
            pitch -= 360f;

        keyboard = Keyboard.current;
        mouse = Mouse.current;

        if (keyboard == null)
            Debug.LogWarning("No keyboard detected by Input System.");

        if (mouse == null)
            Debug.LogWarning("No mouse detected by Input System.");
    }

    private void Update()
    {
        if (keyboard == null) keyboard = Keyboard.current;
        if (mouse == null) mouse = Mouse.current;

        if (keyboard == null || mouse == null)
            return;

        DebugInputs();
        HandleSpeedScroll();
        HandleRotation();
        HandleMovement();
        HandlePan();
    }

    private void DebugInputs()
    {
        if (keyboard.wKey.wasPressedThisFrame) Debug.Log("W was pressed");
        if (keyboard.aKey.wasPressedThisFrame) Debug.Log("A was pressed");
        if (keyboard.sKey.wasPressedThisFrame) Debug.Log("S was pressed");
        if (keyboard.dKey.wasPressedThisFrame) Debug.Log("D was pressed");

        if (keyboard.qKey.wasPressedThisFrame) Debug.Log("Q was pressed");
        if (keyboard.eKey.wasPressedThisFrame) Debug.Log("E was pressed");

        if (keyboard.leftShiftKey.wasPressedThisFrame) Debug.Log("Left Shift was pressed");

        if (mouse.leftButton.wasPressedThisFrame) Debug.Log("Left Mouse was pressed");
        if (mouse.rightButton.wasPressedThisFrame) Debug.Log("Right Mouse was pressed");
        if (mouse.middleButton.wasPressedThisFrame) Debug.Log("Middle Mouse was pressed");

        Vector2 scroll = mouse.scroll.ReadValue();
        if (Mathf.Abs(scroll.y) > 0.01f)
        {
            Debug.Log("Mouse wheel moved: " + scroll.y);
        }
    }

    private void HandleSpeedScroll()
    {
        float scrollY = mouse.scroll.ReadValue().y;

        if (Mathf.Abs(scrollY) > 0.01f)
        {
            float direction = Mathf.Sign(scrollY);
            float step = scrollSpeedStep * Mathf.Max(1f, moveSpeed * 0.15f);

            moveSpeed += direction * step;
            moveSpeed = Mathf.Clamp(moveSpeed, minSpeed, maxSpeed);
        }
    }

    private void HandleRotation()
    {
        if (!mouse.rightButton.isPressed)
            return;

        Vector2 delta = mouse.delta.ReadValue();

        yaw += delta.x * lookSensitivity;
        pitch += (invertY ? delta.y : -delta.y) * lookSensitivity;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void HandleMovement()
    {
        float inputX = 0f;
        float inputY = 0f;
        float inputZ = 0f;

        if (keyboard.aKey.isPressed) inputX -= 1f;
        if (keyboard.dKey.isPressed) inputX += 1f;

        if (keyboard.qKey.isPressed) inputY -= 1f;
        if (keyboard.eKey.isPressed) inputY += 1f;

        if (keyboard.sKey.isPressed) inputZ -= 1f;
        if (keyboard.wKey.isPressed) inputZ += 1f;

        Vector3 targetDirection =
            (transform.right * inputX) +
            (transform.up * inputY) +
            (transform.forward * inputZ);

        if (targetDirection.sqrMagnitude > 1f)
            targetDirection.Normalize();

        float speed = moveSpeed;

        if (keyboard.leftShiftKey.isPressed)
            speed *= fastMultiplier;

        Vector3 targetVelocity = targetDirection * speed;
        float sharpness = targetDirection.sqrMagnitude > 0.0001f ? acceleration : deceleration;

        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, sharpness * Time.deltaTime);
        transform.position += currentVelocity * Time.deltaTime;
    }

    private void HandlePan()
    {
        if (!mouse.middleButton.isPressed)
            return;

        Vector2 delta = mouse.delta.ReadValue();

        Vector3 pan =
            (-transform.right * delta.x + -transform.up * delta.y) * moveSpeed * panSpeed * Time.deltaTime;

        transform.position += pan;
    }
}