using UnityEngine;
using UnityEngine.InputSystem; // Necesario para detectar el Shift

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runMultiplier = 2f;

    [Header("Audio de Pasos")]
    public AudioSource footstepsSource; // Arrastra aquí tu AudioSource con el sonido de pasos en loop
    public float walkStepSpeed = 1f;    // Pitch normal
    public float runStepSpeed = 1.5f;   // Pitch acelerado para simular trote

    [Header("References")]
    public Animator armsAnimator;

    private Rigidbody rb;
    private PlayerInputHandler inputHandler;
    private PlayerStateController stateController;

    private const float MoveThreshold = 0.01f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputHandler = GetComponent<PlayerInputHandler>();
        stateController = GetComponent<PlayerStateController>();
    }

    private void FixedUpdate()
    {
        if (Cursor.visible)
        {
            StopMovement();
            return;
        }

        if (inputHandler == null || stateController == null || stateController.IsBusy)
        {
            StopMovement();
            return;
        }

        Vector2 moveInput = inputHandler.MoveInput;
        Vector3 moveDirection = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;

        // Detectar si estamos presionando Shift usando el nuevo Input System
        bool isRunning = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        bool isMoving = moveInput.sqrMagnitude > MoveThreshold;

        // Calcular velocidad final
        float currentSpeed = (isRunning && isMoving) ? walkSpeed * runMultiplier : walkSpeed;
        Vector3 targetVelocity = moveDirection * currentSpeed;
        targetVelocity.y = rb.linearVelocity.y; // mantener gravedad
        rb.linearVelocity = targetVelocity;

        // --- MANEJO DE ESTADOS Y AUDIO ---
        if (isMoving)
        {
            stateController.SetState(PlayerState.Walking); 
            
            if (armsAnimator != null) armsAnimator.SetBool("isWalking", true);

            // Audio de pasos
            if (footstepsSource != null)
            {
                if (!footstepsSource.isPlaying) footstepsSource.Play();
                footstepsSource.pitch = isRunning ? runStepSpeed : walkStepSpeed;
            }
        }
        else
        {
            stateController.SetState(PlayerState.Idle);
            if (armsAnimator != null) armsAnimator.SetBool("isWalking", false);

            // Detener audio
            if (footstepsSource != null && footstepsSource.isPlaying)
            {
                footstepsSource.Pause(); 
            }
        }
    }

    private void StopMovement()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

        if (armsAnimator != null) armsAnimator.SetBool("isWalking", false);
        if (stateController != null) stateController.SetState(PlayerState.Idle);
        
        if (footstepsSource != null && footstepsSource.isPlaying) footstepsSource.Pause();
    }
}