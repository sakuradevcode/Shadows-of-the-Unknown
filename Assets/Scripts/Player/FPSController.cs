using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class FPSController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;

    [Header("Cámara y Linterna")]
    public float mouseSensitivity = 0.2f;
    [Tooltip("El objeto vacío a la altura de los ojos")]
    public Transform cameraHead; 
    public float minPitch = -80f; 
    public float maxPitch = 80f;  

    [Header("Tiempos de Sincronización (Segundos)")]
    [Tooltip("Cuánto tarda la mano en tocar el botón de prender/apagar")]
    public float timeToToggleLight = 0.5f; 
    [Tooltip("En qué momento de la recarga se apaga la luz (Ej: cuando saca la pila)")]
    public float timeToLightOffOnReload = 0.3f; 
    [Tooltip("Cuánto dura la animación de recarga completa antes de volver a prenderla")]
    public float timeToFinishReload = 2.0f;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference lookAction;
    public InputActionReference toggleLightAction; 
    public InputActionReference reloadAction;      
    public InputActionReference interactAction;    

    [Header("Animaciones y Linterna")]
    public Animator armsAnimator; 
    public Light flashlight; 
    
    private bool isLightOn = true; 
    private bool isReloading = false;
    
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

        // 1. LOOK AROUND
        transform.Rotate(Vector3.up * look.x * mouseSensitivity);

        // 2. LOOK UP/DOWN
        pitch -= look.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch); 
        
        if (cameraHead != null)
        {
            cameraHead.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
        
        // --- TURN ON / OFF FLASHLIGHT ---
        if (toggleLightAction != null && toggleLightAction.action.WasPerformedThisFrame() && !isReloading)
        {
            isLightOn = !isLightOn;

            if (armsAnimator != null)
            {
                if (isLightOn) armsAnimator.SetTrigger("turnOn");
                else armsAnimator.SetTrigger("turnOff");
            }
            
            StartCoroutine(ToggleLightSync());
        }
        
        // --- RELOAD ---
        if (reloadAction != null && reloadAction.action.WasPerformedThisFrame() && !isReloading)
        {
            StartCoroutine(ReloadSync());
        }
        
        // --- INTERACT ---
        if (interactAction != null && interactAction.action.WasPerformedThisFrame() && !isReloading)
        {
            if (armsAnimator != null) armsAnimator.SetTrigger("interact");
        }
    }

    void FixedUpdate()
    {   
        if (Cursor.visible) 
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); 
            if (armsAnimator != null) armsAnimator.SetBool("isWalking", false);
            return;
        }

        if (moveAction == null || moveAction.action == null) return;
        
        Vector2 move = moveAction.action.ReadValue<Vector2>();
        Vector3 moveDirection = (transform.forward * move.y + transform.right * move.x).normalized;
        Vector3 targetVelocity = moveDirection * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y; 

        rb.linearVelocity = targetVelocity;
        
        if (armsAnimator != null)
        {
            bool isMoving = move.sqrMagnitude > 0.01f;
            armsAnimator.SetBool("isWalking", isMoving);
        }
    }
    
    private IEnumerator ToggleLightSync()
    {
        yield return new WaitForSeconds(timeToToggleLight);
        
        if (flashlight != null) flashlight.enabled = isLightOn;
    }

    private IEnumerator ReloadSync()
    {
        isReloading = true;
        
        bool wasLightOnBeforeReload = isLightOn; 
        
        if (armsAnimator != null) armsAnimator.SetTrigger("reload");
        
        yield return new WaitForSeconds(timeToLightOffOnReload);
        
        if (flashlight != null) flashlight.enabled = false;
        isLightOn = false;
        
        yield return new WaitForSeconds(timeToFinishReload - timeToLightOffOnReload);
        
        isLightOn = wasLightOnBeforeReload;
        if (flashlight != null) flashlight.enabled = isLightOn;
        
        isReloading = false;
    }

    private void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (lookAction != null) lookAction.action.Enable();
        if (toggleLightAction != null) toggleLightAction.action.Enable();
        if (reloadAction != null) reloadAction.action.Enable();
        if (interactAction != null) interactAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
        if (lookAction != null) lookAction.action.Disable();
        if (toggleLightAction != null) toggleLightAction.action.Disable();
        if (reloadAction != null) reloadAction.action.Disable();
        if (interactAction != null) interactAction.action.Disable();
    }
}