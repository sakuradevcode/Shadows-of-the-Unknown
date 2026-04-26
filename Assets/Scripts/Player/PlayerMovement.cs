using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

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

        if (inputHandler == null || stateController == null)
            return;

        if (stateController.IsBusy)
        {
            StopMovement();
            return;
        }

        Vector2 moveInput = inputHandler.MoveInput;

        Vector3 moveDirection =
            (transform.forward * moveInput.y +
             transform.right * moveInput.x).normalized;

        Vector3 targetVelocity = moveDirection * moveSpeed;

        // mantener gravedad
        targetVelocity.y = rb.linearVelocity.y;

        rb.linearVelocity = targetVelocity;

        bool isMoving = moveInput.sqrMagnitude > MoveThreshold;

        if (isMoving)
            stateController.SetState(PlayerState.Walking);
        else
            stateController.SetState(PlayerState.Idle);

        if (armsAnimator != null)
            armsAnimator.SetBool("isWalking", isMoving);
    }

    private void StopMovement()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

        if (armsAnimator != null)
            armsAnimator.SetBool("isWalking", false);

        if (stateController != null)
            stateController.SetState(PlayerState.Idle);
    }
}