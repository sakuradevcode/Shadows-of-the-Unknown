using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class FPSController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;

    [Header("Cámara y Linterna")]
    public float mouseSensitivity = 0.2f;
    [Tooltip("El objeto vacío a la altura de los ojos")]
    public Transform cameraHead; 
    public float minPitch = -80f; // No dejar que mire totalmente hacia los pies
    public float maxPitch = 80f;  

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference lookAction;

    private Rigidbody rb;
    private float pitch; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {   
        if (Cursor.visible) return;
        
        if (lookAction == null || lookAction.action == null) return;
        
        Vector2 look = lookAction.action.ReadValue<Vector2>();

        // 1. Mirar a los lados (Rota el cuerpo entero invisible)
        transform.Rotate(Vector3.up * look.x * mouseSensitivity);

        // 2. Mirar arriba/abajo (Rota solo la Cabeza, moviendo la linterna y cámara)
        pitch -= look.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch); 
        
        if (cameraHead != null)
        {
            cameraHead.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    void FixedUpdate()
    {   
        // NUEVA LÍNEA: Si estamos en un menú, el personaje no camina
        if (Cursor.visible) 
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); // Frenar en seco
            return;
        }

        // ESCUDO: Si no hay input asignado...
        if (moveAction == null || moveAction.action == null) return;
        
        // 3. Caminar (Físicas)
        Vector2 move = moveAction.action.ReadValue<Vector2>();
        
        // Caminamos hacia donde mira el cuerpo invisible
        Vector3 moveDirection = (transform.forward * move.y + transform.right * move.x).normalized;
        
        Vector3 targetVelocity = moveDirection * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y; 

        rb.linearVelocity = targetVelocity;
    }

    private void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (lookAction != null) lookAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
        if (lookAction != null) lookAction.action.Disable();
    }
}