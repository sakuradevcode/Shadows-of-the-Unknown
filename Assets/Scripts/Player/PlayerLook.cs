using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [Header("Look Settings")]
    public float mouseSensitivity = 0.2f;

    [Tooltip("Objeto vacío a la altura de los ojos")]
    public Transform cameraHead;

    public float minPitch = -80f;
    public float maxPitch = 80f;

    private PlayerInputHandler inputHandler;
    private PlayerStateController stateController;

    private float pitch;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        stateController = GetComponent<PlayerStateController>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Cursor.visible)
            return;

        if (inputHandler == null)
            return;

        HandleLook();
    }

    private void HandleLook()
    {
        Vector2 lookInput = inputHandler.LookInput;

        // Rotación horizontal (Yaw)
        transform.Rotate(
            Vector3.up * lookInput.x * mouseSensitivity
        );

        // Rotación vertical (Pitch)
        pitch -= lookInput.y * mouseSensitivity;
        pitch = Mathf.Clamp(
            pitch,
            minPitch,
            maxPitch
        );

        if (cameraHead != null)
        {
            cameraHead.localRotation =
                Quaternion.Euler(pitch, 0f, 0f);
        }
    }
}