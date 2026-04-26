using UnityEngine;
using TMPro; // Necesario para TextMeshPro
using UnityEngine.InputSystem;

public class KeypadDoor : MonoBehaviour
{
    [Header("Contraseña y Puerta")]
    public string correctPassword = "8945";
    public Transform doorPivot; // El eje de las bisagras de la puerta
    public Vector3 openRotation = new Vector3(0, 90, 0); // Cuánto rota al abrirse
    
    [Header("Interacción")]
    public Transform playerCamera; // La cámara (Cabeza) del jugador
    public float interactionDistance = 3f;
    public InputActionReference interactAction; // Tecla para interactuar (Ej: E o Clic)

    [Header("Interfaz UI")]
    public GameObject keypadUI; // El Canvas/Panel del teclado
    public TMP_InputField inputField; // Donde se escribe el texto
    
    public LayerMask interactLayerMask;
    
    private bool isDoorOpen = false;
    private bool isUIVisible = false;

    void Start()
    {
        if (keypadUI != null) keypadUI.SetActive(false);
    }

    void Update()
    {
        // Si la puerta está cerrada, la UI no está abierta, y presionamos "Interactuar"
        if (!isDoorOpen && !isUIVisible && interactAction.action.WasPressedThisFrame())
        {
            RaycastHit hit;
            // Lanzamos un láser desde la cámara hacia adelante
            if (Physics.Raycast(
                    playerCamera.position,
                    playerCamera.forward,
                    out hit,
                    interactionDistance,
                    interactLayerMask
                ))
            {
                // Si el láser golpea ESTE objeto (la puerta)
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    OpenKeypad();
                }
            }
        }
    }

    public void OpenKeypad()
    {
        isUIVisible = true;
        keypadUI.SetActive(true);
        inputField.text = ""; // Limpiar intentos anteriores
        
        // Liberar el cursor para poder clickear la interfaz
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Esta función la llamaremos desde un Botón "Confirmar" en la UI
    public void CheckPassword()
    {
        if (inputField.text == correctPassword)
        {
            // ¡Éxito!
            isDoorOpen = true;
            CloseKeypad();
            doorPivot.localRotation = Quaternion.Euler(openRotation); // Abrir puerta
            Debug.Log("¡Puerta Abierta!");
        }
        else
        {
            // Fallo
            inputField.text = ""; // Limpiar para intentar de nuevo
            Debug.Log("Código Incorrecto");
        }
    }

    // Esta función la llamaremos desde un Botón "Salir" en la UI
    public void CloseKeypad()
    {
        isUIVisible = false;
        keypadUI.SetActive(false);
        
        // Volver a bloquear el cursor para el modo FPS
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        if (interactAction != null) interactAction.action.Enable();
    }

    private void OnDisable()
    {
        if (interactAction != null) interactAction.action.Disable();
    }
}