using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference lookAction;
    public InputActionReference toggleLightAction;
    public InputActionReference reloadAction;
    public InputActionReference interactAction;

    public Vector2 MoveInput => moveAction.action.ReadValue<Vector2>();
    public Vector2 LookInput => lookAction.action.ReadValue<Vector2>();

    public bool ToggleLightPressed =>
        toggleLightAction.action.WasPerformedThisFrame();

    public bool ReloadPressed =>
        reloadAction.action.WasPerformedThisFrame();

    public bool InteractPressed =>
        interactAction.action.WasPerformedThisFrame();

    private void OnEnable()
    {
        moveAction?.action.Enable();
        lookAction?.action.Enable();
        toggleLightAction?.action.Enable();
        reloadAction?.action.Enable();
        interactAction?.action.Enable();
    }

    private void OnDisable()
    {
        moveAction?.action.Disable();
        lookAction?.action.Disable();
        toggleLightAction?.action.Disable();
        reloadAction?.action.Disable();
        interactAction?.action.Disable();
    }
}