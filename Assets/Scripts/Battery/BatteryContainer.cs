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

    [Header("Configuración de Sonido (SFX)")]
    public AudioSource pickupSource;
    

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
        // 1. Identificamos qué pila visual vamos a agarrar
        GameObject batteryToRemove = batteries[0];

        if (batteryToRemove != null)
        {
            // --- LA MAGIA DEL AUDIO ---
            // 2. Tomamos el componente AudioSource específico de ESTA pila hija
            AudioSource childSource = batteryToRemove.GetComponent<AudioSource>();

            // 3. Verificamos que todo exista y extraemos el sonido (clip) de la pila
            // para reproducirlo en el parlante seguro del mueble (pickupSource)
            if (pickupSource != null && childSource != null && childSource.clip != null)
            {
                pickupSource.PlayOneShot(childSource.clip);
            }
            // --------------------------

            // 4. Ahora sí, podemos apagar la pila visualmente sin miedo a cortar el audio
            batteryToRemove.SetActive(false);
        }

        // Removemos la pila de la lista lógica
        batteries.RemoveAt(0);

        // Sumamos la carga a la linterna
        FlashlightBatterySystem batterySystem = FindObjectOfType<FlashlightBatterySystem>();
        if (batterySystem != null)
        {
            batterySystem.AddBattery(batteryAmountPerPickup);
        }

        // Si ya no quedan pilas, ocultamos la UI
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