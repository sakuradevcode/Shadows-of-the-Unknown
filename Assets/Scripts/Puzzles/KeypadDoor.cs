using UnityEngine;
using TMPro; // Necesario para TextMeshPro
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
    
public class KeypadDoor : MonoBehaviour
{
    public enum DoorType
    {
        HouseDoor,
        BathroomDoor
    }

    [Header("Configuración de Misiones")]
    public DoorType doorType;
    
    [Header("Botones UI")]
    public Button confirmButton;
    public Button closeButton;
    
    [Header("Contraseña y Puerta")]
    public string correctPassword;
    public Transform doorPivot; // El eje de las bisagras de la puerta
    public Vector3 openRotation = new Vector3(0, 90, 0); // Cuánto rota al abrirse
    public float openSpeed = 2f;
    
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
    
    private Quaternion closedRotation;
    private Quaternion targetOpenRotation;

    void Start()
    {
        if (keypadUI != null) keypadUI.SetActive(false);
        
        if (doorPivot != null)
        {
            closedRotation = doorPivot.localRotation;
            // Calculamos la rotación final sumando la rotación de apertura a la actual
            targetOpenRotation = closedRotation * Quaternion.Euler(openRotation);
        }
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
                    if (doorType == DoorType.HouseDoor)
                    {
                        MissionManager.Instance.TryOpenHouse(); 
                    }
                    else if (doorType == DoorType.BathroomDoor)
                    {
                        MissionManager.Instance.TryOpenBathroom(); 
                    }
                    
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
        
        // Desconectamos la puerta anterior para que no haya conflictos
        confirmButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        // Conectamos los botones a ESTA puerta
        confirmButton.onClick.AddListener(CheckPassword);
        closeButton.onClick.AddListener(CloseKeypad);
        
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
            
            StartCoroutine(OpenDoorSmoothly());
            
            if (doorType == DoorType.HouseDoor)
            {
                MissionManager.Instance.OpenHouse(); 
            }
            else if (doorType == DoorType.BathroomDoor)
            {
                MissionManager.Instance.OpenBathroom(); 
            }
            
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
    
    private IEnumerator OpenDoorSmoothly()
    {
        float timeElapsed = 0f;
        
        // Mientras no hayamos llegado a la rotación objetivo, seguimos girando
        while (timeElapsed < 1f)
        {
            timeElapsed += Time.deltaTime * openSpeed;
            
            // Quaternion.Slerp hace una transición matemática súper suave entre dos rotaciones
            doorPivot.localRotation = Quaternion.Slerp(closedRotation, targetOpenRotation, timeElapsed);
            
            // Esperamos al siguiente frame para continuar
            yield return null; 
        }
        
        // Asegurarnos de que quede exactamente en la posición final al terminar
        doorPivot.localRotation = targetOpenRotation;
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