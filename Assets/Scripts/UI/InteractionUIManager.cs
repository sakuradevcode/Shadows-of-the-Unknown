using UnityEngine;
using TMPro;

public class InteractionUIManager : MonoBehaviour
{
    public static InteractionUIManager Instance;

    [Header("UI References")]
    [Tooltip("Drag your InteractPrompt TextMeshPro object here")]
    public TextMeshProUGUI promptText; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        HidePrompt();
    }

    public void ShowPrompt(string customMessage = "[E] Interact")
    {
        if (promptText != null)
        {
            promptText.text = customMessage;
            promptText.gameObject.SetActive(true);
        }
    }

    public void HidePrompt()
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }
}