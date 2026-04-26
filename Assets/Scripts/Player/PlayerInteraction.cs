// PlayerInteraction.cs

using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    public Animator armsAnimator;

    private PlayerInputHandler inputHandler;
    private PlayerStateController stateController;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        stateController = GetComponent<PlayerStateController>();
    }

    private void Update()
    {
        if (Cursor.visible)
            return;

        if (inputHandler == null || stateController == null)
            return;

        HandleInteraction();
    }

    private void HandleInteraction()
    {
        if (!inputHandler.InteractPressed)
            return;

        if (stateController.IsBusy)
            return;

        stateController.SetState(PlayerState.Interacting);

        armsAnimator?.SetTrigger("interact");
    }

    /*
        ==========
        ANIMATION EVENT
        ==========
    */

    public void AnimationEvent_InteractionFinished()
    {
        stateController.SetState(PlayerState.Idle);
    }
}