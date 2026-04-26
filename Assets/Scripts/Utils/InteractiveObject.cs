using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractiveObject : MonoBehaviour
{
    [Header("Prompt Settings")]
    [Tooltip("The text that will appear on screen")]
    public string screenMessage = "[E] Interact";

    private bool isPlayerNearby = false;

    void Start()
    {
        if (TryGetComponent<Collider>(out Collider col))
        {
            col.isTrigger = true;
        }
    }

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