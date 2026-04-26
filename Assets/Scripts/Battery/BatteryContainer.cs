using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BatteryContainer : MonoBehaviour
{   
    [Header("Prompt Settings")]
    [Tooltip("The text that will appear on screen")]
    public string screenMessage = "[E] Pick Battery";
    
    [Header("Interaction")]
    public Transform playerCamera;
    public InputActionReference interactAction;
    public float interactDistance = 3f;

    [Header("Batteries")]
    [Tooltip("Lista de baterías visuales dentro del mueble")]
    public List<GameObject> batteries = new List<GameObject>();

    [Tooltip("Cantidad que suma cada pickup")]
    public int batteryAmountPerPickup = 1;
    
    [Header("Layers")]
    [Tooltip("Lista de layers interactuables")]
    public LayerMask interactLayerMask;

    private bool isPlayerNearby = false;

    private void Update()
    {
        if (!isPlayerNearby)
            return;

        if (interactAction == null || !interactAction.action.WasPressedThisFrame())
            return;

        TryPickupBattery();
    }

    private void TryPickupBattery()
    {
        if (batteries.Count <= 0)
            return;

        RaycastHit hit;
        
        if (Physics.Raycast(
            playerCamera.position,
            playerCamera.forward,
            out hit,
            interactDistance,
            interactLayerMask))
        {
            Debug.Log("test");
            Debug.Log(hit.transform == transform); 
            Debug.Log(hit.transform.IsChildOf(transform));
            Debug.Log(hit.transform.name);
            Debug.Log(hit.collider.name);
            Debug.Log(hit.transform.root.name);
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                PickupOneBattery();
            }
        }
    }

    private void PickupOneBattery()
    {
        GameObject batteryToRemove = batteries[0];

        if (batteryToRemove != null)
        {
            batteryToRemove.SetActive(false);
        }

        batteries.RemoveAt(0);

        FlashlightBatterySystem batterySystem =
            FindObjectOfType<FlashlightBatterySystem>();

        if (batterySystem != null)
        {
            batterySystem.AddBattery(batteryAmountPerPickup);
        }

        if (batteries.Count <= 0)
        {
            if (InteractionUIManager.Instance != null)
            {
                InteractionUIManager.Instance.HidePrompt();
            }
        }

        Debug.Log("Batería recogida. Restantes visuales: " + batteries.Count);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        isPlayerNearby = true;

        if (batteries.Count > 0 &&
            InteractionUIManager.Instance != null)
        {
            InteractionUIManager.Instance.ShowPrompt(screenMessage);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        isPlayerNearby = false;

        if (InteractionUIManager.Instance != null)
        {
            InteractionUIManager.Instance.HidePrompt();
        }
    }

    private void OnEnable()
    {
        if (interactAction != null)
            interactAction.action.Enable();
    }

    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.action.Disable();
    }
}