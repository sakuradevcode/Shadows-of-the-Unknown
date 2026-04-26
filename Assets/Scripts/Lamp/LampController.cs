using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class LampController : MonoBehaviour
{
    [Header("Lamp Lights")]
    [Tooltip("Drag all Point Lights that make up this lamp here")]
    public List<Light> pointLights = new List<Light>();

    [Header("Configuration")]
    public bool isOnByDefault = true;
    [Tooltip("Assign the Interact action (e.g., E key) here")]
    public InputActionReference interactAction; 

    private bool isPlayerNearby = false;
    private bool currentState;

    void Start()
    {
        currentState = isOnByDefault;
        
        if (TryGetComponent<Collider>(out Collider col))
        {
            col.isTrigger = true;
        }
        
        ApplyLightState();
    }

    void Update()
    {
        if (isPlayerNearby && interactAction != null && interactAction.action.WasPerformedThisFrame())
        {
            currentState = !currentState;
            ApplyLightState();
        }
    }
    
    private void ApplyLightState()
    {
        foreach (Light lightSource in pointLights)
        {
            if (lightSource != null)
            {
                lightSource.enabled = currentState;
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}