// PlayerFlashlight.cs

using UnityEngine;

public class PlayerFlashlight : MonoBehaviour
{
    [Header("References")]
    public Animator armsAnimator;
    public Light flashlight;

    private PlayerInputHandler inputHandler;
    private PlayerStateController stateController;

    private bool isLightOn = true;
    private bool lightBeforeReload = true;

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

        HandleToggleLight();
        HandleReload();
    }

    private void HandleToggleLight()
    {
        if (!inputHandler.ToggleLightPressed)
            return;

        if (stateController.IsBusy)
            return;

        if (isLightOn)
        {
            armsAnimator?.SetTrigger("turnOff");
        }
        else
        {
            armsAnimator?.SetTrigger("turnOn");
        }

        isLightOn = !isLightOn;
    }

    private void HandleReload()
    {
        if (!inputHandler.ReloadPressed)
            return;

        if (stateController.IsBusy)
            return;

        lightBeforeReload = isLightOn;

        stateController.SetState(PlayerState.Reloading);
        armsAnimator?.SetTrigger("reload");
    }

    /*
        ==========
        ANIMATION EVENTS
        ==========
        Estos métodos deben llamarse desde los clips de animación
    */

    // Evento cuando la mano toca el botón ON/OFF
    public void AnimationEvent_ToggleLight()
    {
        if (flashlight != null)
            flashlight.enabled = isLightOn;
    }

    // Evento cuando se saca la batería
    public void AnimationEvent_ReloadLightOff()
    {
        if (flashlight != null)
            flashlight.enabled = false;

        isLightOn = false;
    }

    // Evento al terminar la recarga
    public void AnimationEvent_ReloadFinished()
    {
        isLightOn = lightBeforeReload;

        if (flashlight != null)
            flashlight.enabled = isLightOn;

        stateController.SetState(PlayerState.Idle);
    }
}