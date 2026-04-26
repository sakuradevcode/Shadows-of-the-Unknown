using UnityEngine;

public enum PlayerState
{
    Idle,
    Walking,
    Reloading,
    Interacting,
    Disabled
}

public class PlayerStateController : MonoBehaviour
{
    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;

    public bool IsBusy =>
        CurrentState == PlayerState.Reloading ||
        CurrentState == PlayerState.Interacting ||
        CurrentState == PlayerState.Disabled;

    public void SetState(PlayerState newState)
    {
        CurrentState = newState;
    }

    public bool IsState(PlayerState state)
    {
        return CurrentState == state;
    }
}