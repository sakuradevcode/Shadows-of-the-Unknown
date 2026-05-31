using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class EscapeVehicle : MonoBehaviour
{
    [Header("Configuración de Escape")]
    [Tooltip("El nombre exacto de tu escena de victoria")]
    public string victorySceneName = "Victoria"; 
    [Tooltip("El texto que aparecerá en pantalla")]
    public string screenMessage = "[E] Escapar";

    [Header("Input (Teclas)")]
    [Tooltip("Arrastrá acá la acción de Interactuar (Ej: la tecla E)")]
    public InputActionReference interactAction; 

    private bool isPlayerNearby = false;

    void Update()
    {
        // Si el jugador está en la zona invisible Y aprieta la tecla asignada (E)...
        if (isPlayerNearby && interactAction != null && interactAction.action.WasPerformedThisFrame())
        {
            WinGame();
        }
    }

    private void WinGame()
    {
        Debug.Log("¡El jugador escapó! Cargando pantalla de victoria...");
        
        // Escondemos el cartel antes de cambiar de escena por prolijidad
        if (InteractionUIManager.Instance != null)
        {
            InteractionUIManager.Instance.HidePrompt();
        }

        // Cargamos la escena (asegurate de que esté en tus Build Settings)
        SceneManager.LoadScene(victorySceneName);
    }

    // --- Manejo de la Zona y el Cartelito ---
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
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
    
    private void OnDisable()
    {
        if (isPlayerNearby && InteractionUIManager.Instance != null)
        {
            InteractionUIManager.Instance.HidePrompt();
            isPlayerNearby = false;
        }
    }
}