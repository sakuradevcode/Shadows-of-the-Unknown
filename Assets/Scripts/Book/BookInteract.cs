using UnityEngine;
using UnityEngine.InputSystem; // Necesario para el nuevo Input System

[RequireComponent(typeof(Collider))]
public class BookInteract : MonoBehaviour
{
    [Header("Configuración")]
    public string screenMessage = "[E] Leer libro";
    
    [Header("Input System")]
    [Tooltip("Arrastrá acá tu Action de Interactuar")]
    public InputActionReference interactAction; // Referencia a tu Action configurado
    
    private bool isPlayerNearby = false;
    private bool hasBeenRead = false;

    void Start()
    {
        if (TryGetComponent<Collider>(out Collider col))
        {
            col.isTrigger = true;
        }
    }

    void Update()
    {
        // Verificamos si el action no es nulo, el jugador está cerca, no lo leyó aún, y si apretó el botón
        if (interactAction != null && isPlayerNearby && !hasBeenRead && interactAction.action.WasPressedThisFrame())
        {
            // Reproducir la frase del libro
            if (VoiceManager.Instance != null)
            {
                VoiceManager.Instance.PlayBookPhrase();
            }

            hasBeenRead = true; // Lo marcamos como leído
            
            MissionManager.Instance.ReadBook();
            
            // Ocultamos el cartel de la UI porque ya interactuó
            if (InteractionUIManager.Instance != null)
            {
                InteractionUIManager.Instance.HidePrompt();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Solo mostramos el cartel si es el jugador y si el libro NO fue leído aún
        if (other.CompareTag("Player") && !hasBeenRead)
        {
            isPlayerNearby = true;
            
            if (InteractionUIManager.Instance != null)
            {
                InteractionUIManager.Instance.ShowPrompt(screenMessage);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            
            if (InteractionUIManager.Instance != null)
            {
                InteractionUIManager.Instance.HidePrompt();
            }
        }
    }
}